using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance;

    [Header("Configuración Tiempo")]
    public float dayDurationInMinutes = 5f;
    public int startHour = 8;

    private float gameTime;
    private int currentHour;
    private int currentMinute;
    private GUIStyle clockStyle;
    private bool fivePMTriggered = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeClock();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeClock()
    {
        currentHour = startHour;
        currentMinute = 0;
        gameTime = 0f;
        fivePMTriggered = false;

        clockStyle = new GUIStyle();
        clockStyle.fontSize = 20;
        clockStyle.normal.textColor = Color.white;
        clockStyle.alignment = TextAnchor.UpperRight;
        clockStyle.fontStyle = FontStyle.Bold;
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        CalculateGameTime();
        CheckFor5PM();
    }

    void CalculateGameTime()
    {
        float realSecondsPerGameHour = (dayDurationInMinutes * 60f) / 24f;
        float totalGameHours = gameTime / realSecondsPerGameHour;

        int totalGameMinutes = Mathf.FloorToInt(totalGameHours * 60f);
        currentHour = (startHour + (totalGameMinutes / 60)) % 24;
        currentMinute = totalGameMinutes % 60;
    }

    void CheckFor5PM()
    {
        if (currentHour == 17 && currentMinute == 0 && !fivePMTriggered)
        {
            fivePMTriggered = true;
            ChangeToNightScene();
        }

        if (currentHour == 17 && currentMinute == 1 && fivePMTriggered)
        {
            fivePMTriggered = false;
        }
    }

    void ChangeToNightScene()
    {
        string nightScene = "room";

        if (IsSceneInBuildSettings(nightScene))
        {
            SceneManager.LoadScene(nightScene);
        }
        else
        {
            Debug.LogError($"Escena '{nightScene}' no encontrada");
        }
    }

    void ChangeToDayScene()
    {
        string dayScene = "SampleScene";

        if (IsSceneInBuildSettings(dayScene))
        {
            SceneManager.LoadScene(dayScene);
        }
        else
        {
            Debug.LogError($"Escena '{dayScene}' no encontrada");
        }
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (buildSceneName == sceneName)
                return true;
        }
        return false;
    }

    void OnGUI()
    {
        string timeString = $"{currentHour:00}:{currentMinute:00}";
        GUI.Label(new Rect(Screen.width - 120, 10, 110, 30), timeString, clockStyle);
    }
}