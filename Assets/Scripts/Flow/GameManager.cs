using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector]
    public int CurrentStage;
    [HideInInspector]
    public int CurrentLevel;
    [HideInInspector]
    public string StageName;

    public bool IsLevelUnlocked(int level)
    {
        string levelName = $"Level{CurrentStage}{level}";

        if (level == 1)
        {
            PlayerPrefs.SetInt(levelName, 1);
            return true;
        }

        if (PlayerPrefs.HasKey(levelName))
        {
            return PlayerPrefs.GetInt(levelName) == 1;
        }

        PlayerPrefs.SetInt(levelName, 0);
        return false;
    }

    public void UnlockLevel()
    {
        CurrentLevel++;

        if (CurrentLevel == 51)
        {
            CurrentLevel = 1;
            CurrentStage++;

            if (CurrentStage == 8)
            {
                CurrentStage = 1;
                GoToMainMenu();
                return;
            }
        }

        string levelName = $"Level{CurrentStage}{CurrentLevel}";
        PlayerPrefs.SetInt(levelName, 1);
    }

    [SerializeField]
    private LevelData DefaultLevel;
    [SerializeField]
    private LevelList _allLevels;

    private Dictionary<string, LevelData> Levels;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGameManager();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGameManager()
    {
        CurrentStage = 1;
        CurrentLevel = 1;

        Levels = new Dictionary<string, LevelData>();

        foreach (var item in _allLevels.Levels)
        {
            Levels[item.LevelName] = item;
        }
    }

    public LevelData GetLevel()
    {
        string levelName = $"Level{CurrentStage}{CurrentLevel}";

        if (Levels.ContainsKey(levelName))
        {
            return Levels[levelName];
        }

        return DefaultLevel;
    }

    private const string MainMenuScene = "MainMenu";
    private const string GameplayScene = "Gameplay";

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenuScene);
    }

    public void GoToGameplay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameplayScene);
    }
}
