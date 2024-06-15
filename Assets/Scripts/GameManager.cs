using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        int gameIndex = Random.Range(0, 3); // Randomly choose a game (0: Flow, 1: One Line, 2: Plumber)
        switch (gameIndex)
        {
            case 0:
                LoadGame("FlowGameScene");
                break;
            case 1:
                LoadGame("OneLineGameScene");
                break;
            case 2:
                LoadGame("PlumberGameScene");
                break;
        }
    }

    private void LoadGame(string sceneName)
    {
        // Load the chosen game scene
        SceneManager.LoadScene(sceneName);
    }

    public void EndGame(int levelsComplete, int scoreImprovement)
    {
        // Save post-game session data
        TestManager testManager = FindObjectOfType<TestManager>();
        if (testManager != null)
        {
            testManager.SavePostGameSessionData(levelsComplete, scoreImprovement);
        }

        // Return to main menu or next test
        // For example:
        SceneManager.LoadScene("MainMenuScene");
    }
}
