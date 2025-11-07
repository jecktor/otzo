using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private MainCharacterScript mainCharacterScript;
    private GUIStyle guiStyle;
    private Texture2D backgroundTex;

    private GameObject computerCanvas;
    private GameObject shopPanel;
    private GameObject brotherPCPanel;
    private Text moneyText;

    private int playerMoney = 2500;
    private List<ShopItem> shopItems = new List<ShopItem>();
    private List<ShopItem> brotherPCItems = new List<ShopItem>();
    private List<string> purchasedBrotherItems = new List<string>();

    private GameObject tiendaBtn;
    private GameObject pcBtn;
    private bool isShopActive = true;

    void Start()
    {
        FindPlayer();
        mainCamera = Camera.main;
        SetupCollider();
        CreateGUIStyle();
        InitializeShopData();
        EnsureShopManagerExists();
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
    }

    void InitializeShopData()
    {
        shopItems.Add(new ShopItem("🌟 Mejorar Actitud Clientes", 800, "Clientes más felices y generosos"));
        shopItems.Add(new ShopItem("📦 Expansión de Inventario", 1200, "Próximamente"));
        shopItems.Add(new ShopItem("🎯 Sistema de Fidelidad", 1000, "Próximamente"));
        shopItems.Add(new ShopItem("📱 Marketing Digital", 1500, "Próximamente"));
        shopItems.Add(new ShopItem("🚚 Servicio a Domicilio", 900, "Próximamente"));

        brotherPCItems.Add(new ShopItem("🎮 Tarjeta Gráfica RTX", 800, "Mejora el rendimiento gaming"));
        brotherPCItems.Add(new ShopItem("💾 Memoria RAM 16GB", 450, "16GB DDR4 adicionales"));
        brotherPCItems.Add(new ShopItem("⚡ SSD NVMe 1TB", 600, "Velocidad de carga ultra rápida"));
        brotherPCItems.Add(new ShopItem("🖥️ Monitor 4K 27\"", 1200, "Calidad de imagen premium"));
        brotherPCItems.Add(new ShopItem("⌨️ Teclado Mecánico", 350, "Mejor respuesta táctil"));
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
            new Vector2(0, 0), new Vector2(1920, 1080), backgroundColor);

        GameObject headerPanel = CreatePanel(mainContainer.transform, "Header",
            new Vector2(0, 450), new Vector2(1920, 120), primaryColor);

        RectTransform headerRT = headerPanel.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 0.5f);
        headerRT.anchorMax = new Vector2(1, 0.5f);
        headerRT.anchoredPosition = new Vector2(0, 450);
        headerRT.sizeDelta = new Vector2(0, 120);

        CreateText(headerPanel.transform, "AppTitle", "🛍️ MEJORAS TIENDA", 42,
            new Vector2(-700, 20), new Vector2(500, 50), TextAnchor.MiddleLeft, Color.white);

        CreateText(headerPanel.transform, "TimeInfo", "1:34 (Noche - Reloj U.5x)", 24,
            new Vector2(-700, -30), new Vector2(500, 35), TextAnchor.MiddleLeft, Color.white);

        moneyText = CreateText(headerPanel.transform, "MoneyText", $"💰 Saldo: ${playerMoney}", 32,
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

        CreateShopPanel(mainContainer.transform);
        CreateBrotherPCPanel(mainContainer.transform);

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
                pcImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);

                tiendaText.color = Color.white;
                pcText.color = textGray;

                Button tiendaButton = tiendaBtn.GetComponent<Button>();
                Button pcButton = pcBtn.GetComponent<Button>();

                if (tiendaButton != null)
                {
                    ColorBlock tiendaColors = tiendaButton.colors;
                    tiendaColors.normalColor = primaryColor;
                    tiendaColors.highlightedColor = Color.Lerp(primaryColor, Color.white, 0.2f);
                    tiendaColors.pressedColor = Color.Lerp(primaryColor, Color.black, 0.2f);
                    tiendaButton.colors = tiendaColors;
                }

                if (pcButton != null)
                {
                    ColorBlock pcColors = pcButton.colors;
                    pcColors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                    pcColors.highlightedColor = Color.Lerp(new Color(0.9f, 0.9f, 0.9f, 1f), Color.white, 0.2f);
                    pcColors.pressedColor = Color.Lerp(new Color(0.9f, 0.9f, 0.9f, 1f), Color.black, 0.2f);
                    pcButton.colors = pcColors;
                }

                AddBeautifulShadow(tiendaBtn, new Color(0, 0, 0, 0.2f));
                RemoveShadow(pcBtn);
            }
            else
            {
                tiendaImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                pcImg.color = primaryColor;

                tiendaText.color = textGray;
                pcText.color = Color.white;

                Button tiendaButton = tiendaBtn.GetComponent<Button>();
                Button pcButton = pcBtn.GetComponent<Button>();

                if (tiendaButton != null)
                {
                    ColorBlock tiendaColors = tiendaButton.colors;
                    tiendaColors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                    tiendaColors.highlightedColor = Color.Lerp(new Color(0.9f, 0.9f, 0.9f, 1f), Color.white, 0.2f);
                    tiendaColors.pressedColor = Color.Lerp(new Color(0.9f, 0.9f, 0.9f, 1f), Color.black, 0.2f);
                    tiendaButton.colors = tiendaColors;
                }

                if (pcButton != null)
                {
                    ColorBlock pcColors = pcButton.colors;
                    pcColors.normalColor = primaryColor;
                    pcColors.highlightedColor = Color.Lerp(primaryColor, Color.white, 0.2f);
                    pcColors.pressedColor = Color.Lerp(primaryColor, Color.black, 0.2f);
                    pcButton.colors = pcColors;
                }

                AddBeautifulShadow(pcBtn, new Color(0, 0, 0, 0.2f));
                RemoveShadow(tiendaBtn);
            }
        }
    }

    void RemoveShadow(GameObject target)
    {
        foreach (Transform child in target.transform.parent)
        {
            if (child.name == "BeautifulShadow" && child.GetComponent<RectTransform>().anchoredPosition == target.GetComponent<RectTransform>().anchoredPosition + new Vector2(4, -4))
            {
                Destroy(child.gameObject);
            }
        }
    }

    void CreateShopPanel(Transform parent)
    {
        shopPanel = CreatePanel(parent, "ShopPanel",
            new Vector2(0, -50), new Vector2(1800, 700), new Color(0, 0, 0, 0));

        CreateText(shopPanel.transform, "SectionTitle", "🌟 Mejoras Disponibles para tu Tienda", 34,
            new Vector2(0, 250), new Vector2(900, 50), TextAnchor.MiddleCenter, textDark);
        float startY = 150;
        float spacing = 140;

        for (int i = 0; i < shopItems.Count; i++)
        {
            bool isPurchased = ShopManager.Instance != null && ShopManager.Instance.IsUpgradePurchased(shopItems[i].name);
            bool isAvailable = i == 0;
            CreateBeautifulProductCard(shopPanel.transform, shopItems[i], startY - (i * spacing), isAvailable, isPurchased, false);
        }

        shopPanel.SetActive(true);
    }

    void CreateBrotherPCPanel(Transform parent)
    {
        brotherPCPanel = CreatePanel(parent, "BrotherPCPanel",
            new Vector2(0, -50), new Vector2(1800, 700), new Color(0, 0, 0, 0));

        CreateText(brotherPCPanel.transform, "SectionTitle", "💻 Mejoras para PC del Hermano", 34,
            new Vector2(0, 250), new Vector2(800, 50), TextAnchor.MiddleCenter, textDark);

        float startY = 150;
        float spacing = 140;

        for (int i = 0; i < brotherPCItems.Count; i++)
        {
            bool isPurchased = purchasedBrotherItems.Contains(brotherPCItems[i].name);
            CreateBeautifulProductCard(brotherPCPanel.transform, brotherPCItems[i], startY - (i * spacing), true, isPurchased, true);
        }

        brotherPCPanel.SetActive(false);
    }

    void CreateBeautifulProductCard(Transform parent, ShopItem item, float yPos, bool isAvailable, bool isPurchased, bool isPCItem)
    {
        GameObject card = CreatePanel(parent, $"Product_{item.name}",
            new Vector2(0, yPos), new Vector2(1500, 130), cardColor);

        AddBeautifulShadow(card, new Color(0.9f, 0.9f, 0.9f, 1f));

        GameObject infoContainer = CreatePanel(card.transform, "InfoContainer",
            new Vector2(-350, 0), new Vector2(700, 120), new Color(0, 0, 0, 0));

        string nameText = item.name;
        CreateText(infoContainer.transform, "Name", nameText, 32,
            new Vector2(0, 25), new Vector2(680, 45), TextAnchor.MiddleLeft, textDark);

        string descText = isAvailable ? item.description : "🔒 Disponible próximamente";
        CreateText(infoContainer.transform, "Desc", descText, 20,
            new Vector2(0, -30), new Vector2(680, 35), TextAnchor.MiddleLeft, isAvailable ? textGray : disabledColor);

        GameObject actionContainer = CreatePanel(card.transform, "ActionContainer",
            new Vector2(350, 0), new Vector2(300, 120), new Color(0, 0, 0, 0));

        GameObject priceContainer = CreatePanel(actionContainer.transform, "PriceContainer",
            new Vector2(-90, 0), new Vector2(180, 70), new Color(1f, 0.95f, 0.9f, 1f));
        AddBeautifulShadow(priceContainer, new Color(1f, 0.8f, 0.6f, 0.3f));

        CreateText(priceContainer.transform, "Price", $"${item.price}", 32,
            new Vector2(0, 0), new Vector2(170, 60), TextAnchor.MiddleCenter, priceColor);

        // BOTÓN MÁS GRANDE
        if (isPurchased)
        {
            CreateBeautifulButton(actionContainer.transform, "BuyBtn", "✓ COMPRADO", 26,
                new Vector2(100, 0), new Vector2(180, 60), () => { },
                successColor, Color.white, false);
        }
        else if (!isAvailable)
        {
            CreateBeautifulButton(actionContainer.transform, "BuyBtn", "🔒 PRÓXIMO", 24,
                new Vector2(100, 0), new Vector2(180, 60), () => { },
                disabledColor, Color.white, false);
        }
        else
        {
            CreateBeautifulButton(actionContainer.transform, "BuyBtn", "🛒 COMPRAR", 28,
                new Vector2(100, 0), new Vector2(180, 60), () => {
                    if (isPCItem)
                        BuyBrotherPCItem(item);
                    else
                        BuyShopItem(item);
                },
                secondaryColor, Color.white, true);
        }

        CreatePanel(card.transform, "CardDivider",
            new Vector2(0, -65), new Vector2(1450, 2), new Color(0.95f, 0.95f, 0.95f, 1f));
    }

    void CreateBeautifulButton(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size, System.Action action, Color color, Color textColor, bool isInteractive)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = buttonObj.AddComponent<Image>();
        img.color = color;

        // AGREGAR SOMBRA AL BOTÓN
        AddBeautifulShadow(buttonObj, new Color(0, 0, 0, 0.2f));

        if (isInteractive)
        {
            Button btn = buttonObj.AddComponent<Button>();
            btn.onClick.AddListener(() => action());

            ColorBlock colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.15f);
            colors.disabledColor = Color.Lerp(color, Color.gray, 0.5f);
            btn.colors = colors;
        }

        GameObject textObj = CreateText(buttonObj.transform, "ButtonText", text, fontSize,
            Vector2.zero, size, TextAnchor.MiddleCenter, textColor);

        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(0, 0, 0, 0.3f);
        textOutline.effectDistance = new Vector2(1, -1);
    }

    void AddBeautifulShadow(GameObject target, Color shadowColor)
    {
        GameObject shadow = new GameObject("BeautifulShadow");
        shadow.transform.SetParent(target.transform.parent, false);

        RectTransform rt = shadow.AddComponent<RectTransform>();
        rt.anchoredPosition = target.GetComponent<RectTransform>().anchoredPosition + new Vector2(4, -4);
        rt.sizeDelta = target.GetComponent<RectTransform>().sizeDelta;

        Image img = shadow.AddComponent<Image>();
        img.color = shadowColor;

        shadow.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
    }

    GameObject CreateNavButton(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size, System.Action action, bool isActive)
    {
        Color btnColor = isActive ? primaryColor : new Color(0.9f, 0.9f, 0.9f, 1f);
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

        ColorBlock colors = btn.colors;
        colors.normalColor = btnColor;
        colors.highlightedColor = Color.Lerp(btnColor, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(btnColor, Color.black, 0.2f);
        btn.colors = colors;

        GameObject textObj = CreateText(buttonObj.transform, "ButtonText", text, fontSize,
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

        GameObject textObj = CreateText(buttonObj.transform, "ButtonText", text, fontSize,
            Vector2.zero, size, TextAnchor.MiddleCenter, textColor);

        ColorBlock colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        btn.colors = colors;

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
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = fontSize;

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
        if (playerMoney >= item.price && ShopManager.Instance != null)
        {
            playerMoney -= item.price;
            UpdateMoneyText();

            ShopManager.Instance.PurchaseUpgrade(item.name);

            CreateComputerUI();
            computerCanvas.SetActive(true);
            ShowShopPanel();
            UpdateNavButtons(true);
        }
    }

    void BuyBrotherPCItem(ShopItem item)
    {
        if (playerMoney >= item.price)
        {
            playerMoney -= item.price;
            UpdateMoneyText();

            if (!purchasedBrotherItems.Contains(item.name))
            {
                purchasedBrotherItems.Add(item.name);
            }

            CreateComputerUI();
            computerCanvas.SetActive(true);
            ShowBrotherPCPanel();
            UpdateNavButtons(false);
        }
    }

    void UpdateMoneyText()
    {
        if (moneyText != null)
            moneyText.text = $"💰 Saldo: ${playerMoney}";
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

        UpdateMoneyText();

        if (mainCharacterScript != null)
            mainCharacterScript.enabled = false;
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

        if (mainCharacterScript != null)
            mainCharacterScript.enabled = true;
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

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            mainCharacterScript = player.GetComponent<MainCharacterScript>();
        }
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