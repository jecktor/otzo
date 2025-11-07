using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [Header("Configuración UI")]
    public float charDelay = 0.05f;
    public float commaPause = 0.2f;
    public float sentencePause = 0.5f;

    [Header("Estilo del Cuadro de Diálogo")]
    public Color panelColor = new Color(0f, 0f, 0f, 0.85f);
    public Color textColor = Color.white;
    public Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Font textFont;
    public int fontSize = 24;
    public float borderThickness = 3f;
    public Sprite panelBackgroundSprite;

    [Header("Posición y Tamaño")]
    [Range(0f, 1f)]
    public float panelWidth = 0.9f;
    [Range(0f, 1f)]
    public float panelHeight = 0.2f;
    [Range(0f, 1f)]
    public float panelYPosition = 0.1f;

    private GameObject dialogPanel;
    private GameObject borderPanel;
    private Text dialogText;
    private bool isShowingDialog = false;
    private Coroutine currentDialogCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDialogSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDialogSystem()
    {
        CreateDialogUI();
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    void CreateDialogUI()
    {
        GameObject canvasObj = new GameObject("DialogCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        dialogPanel = new GameObject("DialogPanel");
        dialogPanel.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRT = dialogPanel.AddComponent<RectTransform>();

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float panelWidthPixels = screenWidth * panelWidth;
        float panelHeightPixels = screenHeight * panelHeight;
        float panelX = (screenWidth - panelWidthPixels) / 2f;
        float panelY = screenHeight * panelYPosition;

        panelRT.sizeDelta = new Vector2(panelWidthPixels, panelHeightPixels);
        panelRT.anchoredPosition = new Vector2(0, panelY);

        Image panelImage = dialogPanel.AddComponent<Image>();
        panelImage.color = panelColor;

        if (panelBackgroundSprite != null)
        {
            panelImage.sprite = panelBackgroundSprite;
            panelImage.type = Image.Type.Sliced;
        }
        else
        {
            panelImage.sprite = CreateRoundedSprite();
            panelImage.type = Image.Type.Sliced;
        }

        borderPanel = new GameObject("Border");
        borderPanel.transform.SetParent(dialogPanel.transform, false);

        RectTransform borderRT = borderPanel.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.sizeDelta = new Vector2(borderThickness * 2, borderThickness * 2);
        borderRT.anchoredPosition = Vector2.zero;

        Image borderImage = borderPanel.AddComponent<Image>();
        borderImage.color = borderColor;
        borderImage.sprite = CreateRoundedSprite();

        GameObject textObj = new GameObject("DialogText");
        textObj.transform.SetParent(dialogPanel.transform, false);

        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.05f, 0.1f);
        textRT.anchorMax = new Vector2(0.95f, 0.9f);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = Vector2.zero;

        dialogText = textObj.AddComponent<Text>();
        dialogText.color = textColor;
        dialogText.font = textFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogText.fontSize = fontSize;
        dialogText.alignment = TextAnchor.MiddleCenter;
        dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogText.verticalOverflow = VerticalWrapMode.Overflow;
        dialogText.fontStyle = FontStyle.Bold;
        dialogText.lineSpacing = 1.1f;
    }

    private Sprite CreateRoundedSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        float radius = size / 4f;
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void ShowDialog(string message, float displayDuration = 5f)
    {
        if (isShowingDialog)
        {
            StopCoroutine(currentDialogCoroutine);
        }

        currentDialogCoroutine = StartCoroutine(ShowDialogCoroutine(message, displayDuration));
    }

    private IEnumerator ShowDialogCoroutine(string message, float displayDuration)
    {
        isShowingDialog = true;

        if (dialogPanel == null)
        {
            CreateDialogUI();
        }

        dialogPanel.SetActive(true);

        dialogText.text = "";
        string currentText = "";

        for (int i = 0; i < message.Length; i++)
        {
            currentText += message[i];
            dialogText.text = currentText;

            if (message[i] == ',')
            {
                yield return new WaitForSeconds(commaPause);
            }
            else if (message[i] == '.' || message[i] == '!' || message[i] == '?')
            {
                yield return new WaitForSeconds(sentencePause);
            }
            else
            {
                yield return new WaitForSeconds(charDelay);
            }
        }

        yield return new WaitForSeconds(displayDuration);
        dialogPanel.SetActive(false);
        isShowingDialog = false;
    }

    public void ShowInstantDialog(string message, float displayDuration = 5f)
    {
        if (isShowingDialog)
        {
            StopCoroutine(currentDialogCoroutine);
        }

        currentDialogCoroutine = StartCoroutine(ShowInstantDialogCoroutine(message, displayDuration));
    }

    private IEnumerator ShowInstantDialogCoroutine(string message, float displayDuration)
    {
        isShowingDialog = true;

        if (dialogPanel == null)
        {
            CreateDialogUI();
        }

        if (dialogPanel != null && dialogText != null)
        {
            dialogPanel.SetActive(true);
            dialogText.text = message;

            yield return new WaitForSeconds(displayDuration);

            dialogPanel.SetActive(false);
        }

        isShowingDialog = false;
    }

    public bool IsShowingDialog()
    {
        return isShowingDialog;
    }

    public void HideDialog()
    {
        if (isShowingDialog && currentDialogCoroutine != null)
        {
            StopCoroutine(currentDialogCoroutine);
        }

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        isShowingDialog = false;
    }

    public void UpdateDialogStyle(Color newPanelColor, Color newTextColor, Color newBorderColor)
    {
        panelColor = newPanelColor;
        textColor = newTextColor;
        borderColor = newBorderColor;

        if (dialogPanel != null)
        {
            Image panelImage = dialogPanel.GetComponent<Image>();
            if (panelImage != null) panelImage.color = panelColor;
        }

        if (borderPanel != null)
        {
            Image borderImage = borderPanel.GetComponent<Image>();
            if (borderImage != null) borderImage.color = borderColor;
        }

        if (dialogText != null)
        {
            dialogText.color = textColor;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
        }
    }
}