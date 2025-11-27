using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CustomerBehavior : MonoBehaviour
{
    public static Action<float> OnCustomerReady;
    public static Action OnCustomerLeft;

    [Header("Store navigation")]
    [Tooltip("Assign shelf/pickup points here")]
    public Transform[] storeLocations;

    [Tooltip("Assign store exit here")]
    public Transform exit;

    [Tooltip("Assign the cash register object here (has QueueManager)")]
    public Transform cashRegister;

    [Tooltip("Reference to grocery manager")]
    public GroceryManager groceryManager;

    [Tooltip("Player's current money (debug only)")]
    public float playerMoney = 0f;

    public ScanMiniGame scanMiniGame;
    public AudioSource groceries;

    NavMeshAgent agent;
    Animator anim;

    bool isShopping = true;
    bool isBusy = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Ensure NavMeshAgent settings are safe
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.isStopped = false;

        // Start by going to a random store shelf
        GoToStoreLocationReserved();
    }

    void Update()
    {
        // --- Update animation based on velocity ---
        float speed = agent.velocity.magnitude;
        anim.SetFloat("Speed", speed);

        // --- Handle arrival logic ---
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isBusy)
        {
            if (isShopping)
            {
                // Finished shopping trip to shelf
                StartCoroutine(PickUpRoutine());
            }
            else
            {
                // Already in queue, check if it's our turn
                QueueManager qm = cashRegister.GetComponent<QueueManager>();
                int myPos = qm.GetPositionInQueue(this);

                if (myPos == 0) // First in line
                    StartCoroutine(CheckoutRoutine());
            }
        }
    }

    // --- Choose a random shelf and go there ---
    void GoToStoreLocationReserved()
    {
        var qm = cashRegister.GetComponent<QueueManager>();
        if (!qm) return;

        var spot = qm.RequestStoreSpot(this);
        if (spot != null)
        {
            agent.SetDestination(spot.position);
        }
        else
        {
            // No free shelves: simple fallback → go queue now (or retry, see below)
            isShopping = false;
            var slot = qm.RequestSlot(this);
            if (slot) agent.SetDestination(slot.position);
        }
    }

    public IEnumerator Leave()
    {
        // Hide groceries again
        if (groceryManager != null)
            groceryManager.HideAllGroceries();

        // 🧹 Hide UI prompt when done
        OnCustomerLeft?.Invoke();

        // Notify queue manager that this customer is done
        QueueManager qm = cashRegister.GetComponent<QueueManager>();
        qm.OnCustomerFinished(this);

        // Head to exit
        agent.isStopped = false;
        agent.SetDestination(exit.position);

        yield return CoroutineUtils.WaitForEither(
            () => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance,
            4f
        );

        isBusy = false;

        Destroy(gameObject);
    }

    // --- Play the pickup animation at the shelf ---
    IEnumerator PickUpRoutine()
    {
        isBusy = true;
        agent.isStopped = true;

        // Trigger animation
        anim.SetTrigger("PickUp");

        // Wait for animation to play (adjust to match clip length)
        yield return new WaitForSeconds(5f);

        agent.isStopped = false;

        // After pickup, move toward the queue
        if (isShopping)
        {
            isShopping = false;
            var qm = cashRegister.GetComponent<QueueManager>();
            if (qm != null) qm.ReleaseStoreSpot(this);

            isShopping = false;
            if (qm != null)
            {
                Transform slot = qm.RequestSlot(this);
                if (slot) agent.SetDestination(slot.position);
            }
        }

        isBusy = false;
    }

    // --- Handle checkout when at front of queue ---
    IEnumerator CheckoutRoutine()
    {
        isBusy = true;
        agent.isStopped = true;

        // Play the checkout animation
        anim.SetTrigger("PickUp");
        yield return new WaitForSeconds(3f);
        groceries.Play();

        // Show groceries and calculate total
        float payment = 0f;
        if (groceryManager != null)
        {
            payment = groceryManager.ShowRandomGroceries();
            Debug.Log($"Customer shows groceries worth ${payment}");
        }

        // 🔔 Notify UI that a customer is ready and how much money player will earn
        OnCustomerReady?.Invoke(payment);

        // Wait for player confirmation (only if in checkout zone)
        Debug.Log($"{name} waiting for player to confirm checkout...");
        yield return new WaitUntil(() =>
            CheckoutZone.playerInZone && Input.GetKeyDown(KeyCode.E));

        // --- Apply sleep penalty BEFORE mini-game ---
        float sleepMultiplier = GetSleepMultiplier();
        float basePayment = payment;
        payment *= sleepMultiplier;

        if (sleepMultiplier < 1f)
        {
            Debug.Log($"😴 Penalidad por sueño: {sleepMultiplier:P0}. Pago base: ${basePayment:F2} -> Pago real: ${payment:F2}");
        }

        // --- Begin Scan Mini-Game ---
        if (groceryManager != null)
        {
            // Prepare the items we'll scan (only the active ones)
            List<GameObject> activeItems = new List<GameObject>();
            foreach (var g in groceryManager.groceries)
                if (g.activeSelf) activeItems.Add(g);

            // Run the mini-game
            if (scanMiniGame != null)
            {
                yield return StartCoroutine(scanMiniGame.Run(activeItems, payment, (earned) =>
                {
                    payment = earned;

                    // --- Apply customer attitude bonus AFTER mini-game ---
                    float finalPayment = ApplyCustomerAttitudeBonus(payment);

                    if (finalPayment > payment)
                    {
                        Debug.Log($"🌟 Bono de actitud de clientes: ${payment:F2} -> ${finalPayment:F2}");
                    }

                    payment = finalPayment;
                }));
            }
        }

        // Player gets money
        playerMoney += payment;
        Debug.Log($"💰 Pago final: ${payment:F2} | Total del jugador: ${playerMoney}");

        PlayerWallet wallet = FindFirstObjectByType<PlayerWallet>();
        if (wallet != null)
            wallet.AddMoney(payment);

        yield return StartCoroutine(Leave());
    }

    private float GetSleepMultiplier()
    {
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            float sleepQuality = GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality;

            if (sleepQuality >= 80f) return 1.0f;      
            if (sleepQuality >= 60f) return 0.9f;     
            if (sleepQuality >= 40f) return 0.75f;  
            if (sleepQuality >= 20f) return 0.6f;  
            return 0.5f;                            
        }

        return 1.0f; 
    }

    private float ApplyCustomerAttitudeBonus(float payment)
    {
        if (ShopManager.Instance != null &&
            ShopManager.Instance.IsUpgradePurchased("🌟 Mejorar Actitud Clientes"))
        {
            return payment * 1.5f;
        }

        return payment;
    }
}