using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class QueueManager : MonoBehaviour
{
	[Header("Queue slots (front to back)")]
	public Transform[] queueSlots;

	[Header("Where newcomers wait while the line advances")]
	public Transform queueEntryPoint;

	// Logical queue front=0
	private readonly List<CustomerBehavior> queue = new List<CustomerBehavior>();
	private readonly HashSet<int> occupiedSlots = new HashSet<int>();

	// Newcomers held while advancing
	private readonly Queue<CustomerBehavior> waitingJoiners = new Queue<CustomerBehavior>();

	private bool isAdvancing;

	public bool IsQueueFull => queue.Count >= queueSlots.Length;

	// === Public API ===

	/// Called by a shopper to join the queue. Returns the Transform they should path to now.
	public Transform RequestSlot(CustomerBehavior customer)
	{
		// Already tracked? return their assigned slot
		if (queue.Contains(customer))
			return GetAssignedSlot(customer);

		// If we're mid-advance or no free slot, hold the newcomer at the entry point
		if (isAdvancing || IsQueueFull)
		{
			if (!queue.Contains(customer))
				waitingJoiners.Enqueue(customer);
			return queueEntryPoint != null ? queueEntryPoint : queueSlots[queueSlots.Length - 1];
		}

		// Join queue and assign the first free slot from front to back
		queue.Add(customer);
		int slotIndex = GetFirstFreeSlotIndex();
		if (slotIndex == -1) slotIndex = Mathf.Min(queue.Count - 1, queueSlots.Length - 1);

		occupiedSlots.Add(slotIndex);
		return queueSlots[slotIndex];
	}

	/// Called by the front customer after checkout.
	public void OnCustomerFinished(CustomerBehavior finishedCustomer)
	{
		if (!queue.Contains(finishedCustomer))
			return;

		int idx = queue.IndexOf(finishedCustomer);
		queue.RemoveAt(idx);

		if (idx < queueSlots.Length)
			occupiedSlots.Remove(idx);

		// Start a serialized advance
		if (!isAdvancing) StartCoroutine(AdvanceQueueSequentially());
	}

	public int GetPositionInQueue(CustomerBehavior c) => queue.IndexOf(c);
	public int GetQueueCount() => queue.Count;

	/// Clear queue quickly (e.g., stress meltdown). Everyone leaves at once.
	public void ClearAllCustomersInstant()
	{
		foreach (var c in queue)
			if (c != null) c.StartCoroutine(c.Leave());
		queue.Clear();
		occupiedSlots.Clear();
		waitingJoiners.Clear();
		isAdvancing = false;
	}

	// === Internals ===

	private IEnumerator AdvanceQueueSequentially()
	{
		isAdvancing = true;

		// Recompute target slots: i -> slot i
		occupiedSlots.Clear();

		for (int i = 0; i < queue.Count; i++)
		{
			var customer = queue[i];
			if (customer == null) continue;

			int slotIndex = Mathf.Min(i, queueSlots.Length - 1);
			occupiedSlots.Add(slotIndex);

			var agent = customer.GetComponent<NavMeshAgent>();
			if (agent == null) continue;

			// (Optional) Give movers forward priority so they don't deadlock with new arrivals
			agent.avoidancePriority = 30; // lower number = higher priority

			Vector3 dest = queueSlots[slotIndex].position;
			agent.SetDestination(dest);

			// Wait until THIS customer reaches their new slot before moving the next
			yield return StartCoroutine(WaitNavmeshArrival(agent, 0.05f));
		}

		// Reopen the gate and admit one newcomer (or as many as slots allow)
		while (waitingJoiners.Count > 0 && !IsQueueFull)
		{
			var newcomer = waitingJoiners.Dequeue();
			if (newcomer == null) continue;

			queue.Add(newcomer);
			int slotIndex = GetFirstFreeSlotIndex();
			if (slotIndex == -1) slotIndex = Mathf.Min(queue.Count - 1, queueSlots.Length - 1);

			occupiedSlots.Add(slotIndex);

			var agent = newcomer.GetComponent<NavMeshAgent>();
			if (agent != null)
			{
				agent.avoidancePriority = 60; // newcomers yield to in-line movers
				agent.SetDestination(queueSlots[slotIndex].position);
			}
		}

		isAdvancing = false;
	}

	private IEnumerator WaitNavmeshArrival(NavMeshAgent agent, float epsilon)
	{
		// small safety timeout in case of partial paths
		float t = 0f, timeout = 5f;
		while (t < timeout)
		{
			if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + epsilon)
				break;
			t += Time.deltaTime;
			yield return null;
		}
	}

	private int GetFirstFreeSlotIndex()
	{
		for (int i = 0; i < queueSlots.Length; i++)
			if (!occupiedSlots.Contains(i)) return i;
		return -1;
	}

	private Transform GetAssignedSlot(CustomerBehavior customer)
	{
		int index = queue.IndexOf(customer);
		if (index >= 0 && index < queueSlots.Length)
			return queueSlots[index];
		return queueSlots[queueSlots.Length - 1];
	}
}
