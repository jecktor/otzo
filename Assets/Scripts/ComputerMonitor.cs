using EasyPeasyFirstPersonController;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputerMonitor : MonoBehaviour
{
    [Header("Configuración Monitor")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("Configuración UI")]
    public Color primaryColor = new Color(0.9f, 0.3f, 0.2f, 1f);
    public Color secondaryColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    public Color backgroundColor = new Color(0.98f, 0.98f, 0.98f, 1f);
    public Color cardColor = Color.white;
    public Color textDark = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color textGray = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color priceColor = new Color(0.9f, 0.4f, 0.1f, 1f);
    public Color disabledColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color successColor = new Color(0.2f, 0.8f, 0.3f, 1f);

    private Transform player;
    private bool isNearMonitor = false;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isUsingComputer = false;
    private FirstPersonController playerController;
    private GUIStyle guiStyle;
    private Texture2D backgroundTex;

    private GameObject computerCanvas;
    private GameObject shopPanel;
    private GameObject brotherPCPanel;
    private Text moneyText;

    private List<ShopItem> shopItems = new List<ShopItem>();
    private List<ShopItem> brotherPCItems = new List<ShopItem>();

    private GameObject tiendaBtn;
    private GameObject pcBtn;
    private bool isShopActive = true;

    public bool IsUsingComputer => isUsingComputer;

    void Start()
    {
        FindPlayer();
        mainCamera = Camera.main;
        SetupCollider();
        CreateGUIStyle();
        InitializeShopData();
        EnsureShopManagerExists();

        Debug.Log($"💰 Dinero inicial en ComputerMonitor: ${PlayerWallet.totalMoney}");
    }

    void EnsureShopManagerExists()
    {
        if (ShopManager.Instance == null)
        {
            GameObject shopManagerObj = new GameObject("ShopManager");
            shopManagerObj.AddComponent<ShopManager>();
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isNearMonitor = distance < 3f;
        }

        if (isNearMonitor && Input.GetKeyDown(interactionKey))
        {
            if (!isUsingComputer)
            {
                StartComputer();
            }
            else
            {
                ExitComputer();
            }
        }

        if (isUsingComputer && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitComputer();
        }

        if (isUsingComputer && moneyText != null)
        {
            UpdateMoneyText();
        }

        if (isUsingComputer)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ShowShopPanel();
                UpdateNavButtons(true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ShowBrotherPCPanel();
                UpdateNavButtons(false);
            }
        }
    }

    void InitializeShopData()
    {
        shopItems.Add(new ShopItem("🌟 Mejorar Actitud Clientes", 800, "Clientes más felices y generosos"));
        shopItems.Add(new ShopItem("📦 Expansión de Inventario", 1200, "Próximamente"));
        shopItems.Add(new ShopItem("🎯 Sistema de Fidelidad", 1000, "Próximamente"));
        shopItems.Add(new ShopItem("📱 Marketing Digital", 1500, "Próximamente"));
        shopItems.Add(new ShopItem("🚚 Servicio a Domicilio", 900, "Próximamente"));

        brotherPCItems.Add(new ShopItem("🎮 Tarjeta Gráfica RTX", 800, "Próximamente"));
        brotherPCItems.Add(new ShopItem("💾 Memoria RAM 16GB", 450, "Próximamente"));
        brotherPCItems.Add(new ShopItem("⚡ SSD NVMe 1TB", 600, "Próximamente"));
        brotherPCItems.Add(new ShopItem("🖥️ Monitor 4K 27\"", 1200, "Próximamente"));
        brotherPCItems.Add(new ShopItem("⌨️ Teclado Mecánico", 350, "Próximamente"));
    }

    void CreateComputerUI()
    {
        if (computerCanvas != null)
        {
            Destroy(computerCanvas);
        }

        computerCanvas = new GameObject("ComputerCanvas");
        Canvas canvas = computerCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = computerCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1.0f;

        computerCanvas.AddComponent<GraphicRaycaster>();

        GameObject mainContainer = CreatePanel(computerCanvas.transform, "MainContainer",
            Vector2.zero, Vector2.zero, backgroundColor);

        RectTransform mainRect = mainContainer.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;

        GameObject headerPanel = CreatePanel(mainContainer.transform, "Header",
            new Vector2(0, 450), new Vector2(0, 120), primaryColor);

        RectTransform headerRT = headerPanel.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 1f);
        headerRT.anchorMax = new Vector2(1, 1f);
        headerRT.anchoredPosition = new Vector2(0, -60);
        headerRT.sizeDelta = new Vector2(0, 120);

        CreateText(headerPanel.transform, "AppTitle", "🛍️ MEJORAS TIENDA", 42,
            new Vector2(-700, 0), new Vector2(500, 50), TextAnchor.MiddleLeft, Color.white);

        // USANDO LA VARIABLE ESTÁTICA DIRECTAMENTE
        moneyText = CreateText(headerPanel.transform, "MoneyText", $"💰 Saldo: ${PlayerWallet.totalMoney:F2}", 32,
            new Vector2(500, 0), new Vector2(400, 50), TextAnchor.MiddleRight, Color.white).GetComponent<Text>();

        CreateButton(headerPanel.transform, "CloseBtn", "✕", 36,
            new Vector2(850, 0), new Vector2(80, 80), () => ExitComputer(),
            new Color(0.8f, 0.2f, 0.2f, 1f), Color.white);

        GameObject navPanel = CreatePanel(mainContainer.transform, "Navigation",
            new Vector2(0, 300), new Vector2(700, 60), new Color(0, 0, 0, 0));

        tiendaBtn = CreateNavButton(navPanel.transform, "TiendaBtn", "🏪 MEJORAS TIENDA", 26,
            new Vector2(-150, 0), new Vector2(280, 50), () => {
                ShowShopPanel();
                UpdateNavButtons(true);
            }, true);

        pcBtn = CreateNavButton(navPanel.transform, "PcBtn", "💻 PC HERMANO", 26,
            new Vector2(150, 0), new Vector2(220, 50), () => {
                ShowBrotherPCPanel();
                UpdateNavButtons(false);
            }, false);

        CreatePanel(mainContainer.transform, "Divider",
            new Vector2(0, 240), new Vector2(1600, 3), new Color(0.7f, 0.7f, 0.7f, 1f));

        GameObject contentContainer = CreatePanel(mainContainer.transform, "ContentContainer",
            new Vector2(0, -50), new Vector2(1800, 700), new Color(0, 0, 0, 0));

        CreateShopPanel(contentContainer.transform);
        CreateBrotherPCPanel(contentContainer.transform);

        ShowShopPanel();
        UpdateNavButtons(true);
    }

    void UpdateNavButtons(bool shopActive)
    {
        isShopActive = shopActive;

        if (tiendaBtn != null && pcBtn != null)
        {
            Image tiendaImg = tiendaBtn.GetComponent<Image>();
            Image pcImg = pcBtn.GetComponent<Image>();

            Text tiendaText = tiendaBtn.GetComponentInChildren<Text>();
            Text pcText = pcBtn.GetComponentInChildren<Text>();

            if (shopActive)
            {
                tiendaImg.color = primaryColor;
                tiendaText.color = Color.white;
                pcImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                pcText.color = textGray;
            }
            else
            {
                tiendaImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                tiendaText.color = textGray;
                pcImg.color = primaryColor;
                pcText.color = Color.white;
            }
        }
    }

    void CreateShopPanel(Transform parent)
    {
        shopPanel = CreatePanel(parent, "ShopPanel",
            new Vector2(0, 0), new Vector2(1800, 700), new Color(0, 0, 0, 0));

        CreateText(shopPanel.transform, "SectionTitle", "🌟 Mejoras Disponibles para tu Tienda", 34,
            new Vector2(0, 250), new Vector2(900, 50), TextAnchor.MiddleCenter, textDark);

        float startY = 150;
        float spacing = 140;

        for (int i = 0; i < shopItems.Count; i++)
        {
            bool isPurchased = ShopManager.Instance != null && ShopManager.Instance.IsUpgradePurchased(shopItems[i].name);
            // SOLO la primera mejora está disponible, las demás SIEMPRE bloqueadas
            bool isAvailable = i == 0;
            CreateProductCard(shopPanel.transform, shopItems[i], startY - (i * spacing), isAvailable, isPurchased, false);
        }

        shopPanel.SetActive(true);
    }

    void CreateBrotherPCPanel(Transform parent)
    {
        brotherPCPanel = CreatePanel(parent, "BrotherPCPanel",
            new Vector2(0, 0), new Vector2(1800, 700), new Color(0, 0, 0, 0));

        CreateText(brotherPCPanel.transform, "SectionTitle", "💻 Mejoras para PC del Hermano", 34,
            new Vector2(0, 250), new Vector2(800, 50), TextAnchor.MiddleCenter, textDark);

        CreateText(brotherPCPanel.transform, "LockedInfo", "🔒 Estas mejoras estarán disponibles próximamente", 28,
            new Vector2(0, 180), new Vector2(1000, 40), TextAnchor.MiddleCenter, disabledColor);

        float startY = 100;
        float spacing = 140;

        for (int i = 0; i < brotherPCItems.Count; i++)
        {
            bool isPurchased = false;
            bool isAvailable = false; // Todas las mejoras de PC están bloqueadas
            CreateProductCard(brotherPCPanel.transform, brotherPCItems[i], startY - (i * spacing), isAvailable, isPurchased, true);
        }

        brotherPCPanel.SetActive(false);
    }

    void CreateProductCard(Transform parent, ShopItem item, float yPos, bool isAvailable, bool isPurchased, bool isPCItem)
    {
        GameObject card = CreatePanel(parent, $"Product_{item.name}",
            new Vector2(0, yPos), new Vector2(1500, 130), cardColor);

        GameObject infoContainer = CreatePanel(card.transform, "InfoContainer",
            new Vector2(-350, 0), new Vector2(700, 120), new Color(0, 0, 0, 0));

        CreateText(infoContainer.transform, "Name", item.name, 32,
            new Vector2(0, 25), new Vector2(680, 45), TextAnchor.MiddleLeft, isAvailable ? textDark : new Color(0.6f, 0.6f, 0.6f, 1f));

        string descText = item.description;

        if (isPurchased)
        {
            descText = "✅ Ya comprado - Beneficios activos";
        }
        else if (!isAvailable)
        {
            descText = "🔒 Próximamente - Disponible en futuras actualizaciones";
        }

        CreateText(infoContainer.transform, "Desc", descText, 20,
            new Vector2(0, -30), new Vector2(680, 35), TextAnchor.MiddleLeft, isAvailable ? textGray : disabledColor);

        GameObject actionContainer = CreatePanel(card.transform, "ActionContainer",
            new Vector2(350, 0), new Vector2(300, 120), new Color(0, 0, 0, 0));

        if (isAvailable && !isPurchased)
        {
            GameObject priceContainer = CreatePanel(actionContainer.transform, "PriceContainer",
                new Vector2(-90, 0), new Vector2(180, 70), new Color(1f, 0.95f, 0.9f, 1f));

            CreateText(priceContainer.transform, "Price", $"${item.price}", 32,
                new Vector2(0, 0), new Vector2(170, 60), TextAnchor.MiddleCenter, priceColor);
        }

        if (isPurchased)
        {
            CreateButton(actionContainer.transform, "BuyBtn", "✓ COMPRADO", 26,
                new Vector2(100, 0), new Vector2(180, 60), () => { },
                successColor, Color.white);
        }
        else if (!isAvailable)
        {
            CreateButton(actionContainer.transform, "BuyBtn", "🔒 PRÓXIMAMENTE", 20,
                new Vector2(100, 0), new Vector2(180, 60), () => {
                    Debug.Log($"🔒 {item.name} estará disponible próximamente.");
                },
                disabledColor, Color.white);
        }
        else
        {
            CreateButton(actionContainer.transform, "BuyBtn", "🛒 COMPRAR", 28,
                new Vector2(100, 0), new Vector2(180, 60), () => {
                    if (isPCItem)
                        BuyBrotherPCItem(item);
                    else
                        BuyShopItem(item);
                },
                secondaryColor, Color.white);
        }
    }

    GameObject CreateNavButton(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size, System.Action action, bool isActive)
    {
        Color btnColor = isActive ? primaryColor : new Color(0.8f, 0.8f, 0.8f, 1f);
        Color textCol = isActive ? Color.white : textGray;

        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = buttonObj.AddComponent<Image>();
        img.color = btnColor;

        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(() => action());

        CreateText(buttonObj.transform, "ButtonText", text, fontSize,
            Vector2.zero, size, TextAnchor.MiddleCenter, textCol);

        return buttonObj;
    }

    GameObject CreateButton(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size, System.Action action, Color color, Color textColor)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = buttonObj.AddComponent<Image>();
        img.color = color;

        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(() => action());

        CreateText(buttonObj.transform, "ButtonText", text, fontSize,
            Vector2.zero, size, TextAnchor.MiddleCenter, textColor);

        return buttonObj;
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = color;

        return panel;
    }

    GameObject CreateText(Transform parent, string name, string content, int fontSize, Vector2 position, Vector2 size, TextAnchor alignment, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = FontStyle.Bold;

        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        return textObj;
    }

    void ShowShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        if (brotherPCPanel != null) brotherPCPanel.SetActive(false);
    }

    void ShowBrotherPCPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (brotherPCPanel != null) brotherPCPanel.SetActive(true);
    }

    void BuyShopItem(ShopItem item)
    {
        if (item.name != "🌟 Mejorar Actitud Clientes")
        {
            Debug.Log($"🔒 {item.name} está bloqueada permanentemente.");
            return;
        }

        if (ShopManager.Instance != null && ShopManager.Instance.IsUpgradePurchased(item.name))
        {
            Debug.Log($"⚠️ {item.name} ya está comprado. No se puede comprar de nuevo.");
            return;
        }

        // VERIFICAR si tiene suficiente dinero
        if (PlayerWallet.totalMoney >= item.price)
        {
            // Buscar el PlayerWallet para usar su método SpendMoney
            PlayerWallet wallet = FindObjectOfType<PlayerWallet>();
            if (wallet != null)
            {
                if (wallet.SpendMoney(item.price))
                {
                    PurchaseSuccess(item);
                }
            }
            else
            {
                // Fallback: restar directamente si no encuentra el Wallet
                PlayerWallet.totalMoney -= item.price;
                PlayerPrefs.SetFloat("PlayerMoney", PlayerWallet.totalMoney);
                PlayerPrefs.Save();
                PurchaseSuccess(item);
            }
        }
        else
        {
            Debug.Log($"❌ No tienes suficiente dinero. Necesitas: ${item.price}, Tienes: ${PlayerWallet.totalMoney}");
        }
    }

    void PurchaseSuccess(ShopItem item)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.PurchaseUpgrade(item.name);
        }

        Debug.Log($"✅ Comprado: {item.name} Nuevo saldo: ${PlayerWallet.totalMoney}");

        // Actualizar el texto del dinero inmediatamente
        UpdateMoneyText();

        // Recargar la UI para mostrar los cambios
        RefreshUI();
    }

    void BuyBrotherPCItem(ShopItem item)
    {
        Debug.Log("🔒 Las mejoras del PC del hermano estarán disponibles próximamente");
    }

    void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            // ACTUALIZAR DIRECTAMENTE CON LA VARIABLE ESTÁTICA
            moneyText.text = $"💰 Saldo: ${PlayerWallet.totalMoney:F2}";
        }
    }

    // Método para recargar la UI sin recrear completamente
    void RefreshUI()
    {
        if (shopPanel != null && shopPanel.transform.parent != null)
        {
            // Destruir el panel actual
            Destroy(shopPanel);

            // Volver a crear el panel con los datos actualizados
            Transform parent = shopPanel.transform.parent;
            CreateShopPanel(parent);

            // Mostrar el panel correcto según la navegación actual
            if (isShopActive)
            {
                ShowShopPanel();
                UpdateNavButtons(true);
            }
            else
            {
                ShowBrotherPCPanel();
                UpdateNavButtons(false);
            }
        }
    }

    void StartComputer()
    {
        isUsingComputer = true;
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        if (computerCanvas == null)
            CreateComputerUI();

        Vector3 screenPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.3f;
        mainCamera.transform.position = screenPos;
        mainCamera.transform.LookAt(transform.position);

        computerCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Actualizar el dinero al abrir
        UpdateMoneyText();

        if (playerController != null)
        {
            playerController.SetControl(false);
            playerController.SetCursorVisibility(true);
        }
    }

    void ExitComputer()
    {
        isUsingComputer = false;

        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;

        if (computerCanvas != null)
        {
            computerCanvas.SetActive(false);
            Destroy(computerCanvas);
            computerCanvas = null;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerController != null)
        {
            playerController.SetControl(true);
            playerController.SetCursorVisibility(false);
        }
    }

    void CreateGUIStyle()
    {
        guiStyle = new GUIStyle();
        guiStyle.normal.textColor = Color.white;
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.fontStyle = FontStyle.Bold;

        backgroundTex = new Texture2D(1, 1);
        backgroundTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        backgroundTex.Apply();
        guiStyle.normal.background = backgroundTex;
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = player.GetComponent<FirstPersonController>();
        }
    }

    void SetupCollider()
    {
        Collider existing = GetComponent<Collider>();
        if (existing != null && existing is MeshCollider)
        {
            DestroyImmediate(existing);
        }

        BoxCollider col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3f, 2f, 3f);
        col.center = new Vector3(0f, 1f, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearMonitor = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearMonitor = false;
        }
    }

    void OnGUI()
    {
        if (UIManager.Instance != null && UIManager.Instance.CurrentState == UIManager.GameState.Paused)
            return;

        if (isNearMonitor && !isUsingComputer)
        {
            GUI.Box(new Rect(Screen.width / 2 - 200, 100, 400, 50),
                   "Presiona E para usar el computador", guiStyle);
        }
    }
    void OnDestroy()
    {
        if (computerCanvas != null) Destroy(computerCanvas);
        if (backgroundTex != null) Destroy(backgroundTex);
    }
}

[System.Serializable]
public class ShopItem
{
    public string name;
    public int price;
    public string description;

    public ShopItem(string name, int price, string description)
    {
        this.name = name;
        this.price = price;
        this.description = description;
    }
}