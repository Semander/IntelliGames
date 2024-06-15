using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class TestManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage testImage;  // Reference to the UI RawImage component for the background image
    public List<Button> answerButtons;  // List of answer buttons
    public GameObject testUI;  // GameObject containing the test UI
    public GameObject gameUI;  // GameObject containing the game UI

    private List<TestData> testDatabase;
    private int currentTestIndex = 0;
    private TestData currentTest;

    // Firebase variables
    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private FirebaseUser user;

    void Start()
    {
        InitializeFirebase();
        LoadTestData();  // Load test data from ScriptableObject or JSON
        DisplayTest(currentTestIndex);  // Display the first test
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        user = auth.CurrentUser;
    }

    void LoadTestData()
    {
        // Example of loading from ScriptableObject
        TestDatabase database = Resources.Load<TestDatabase>("TestDatabase");
        testDatabase = database.tests;

        // Alternatively, you can load from JSON
        // string json = Resources.Load<TextAsset>("test_data").text;
        // testDatabase = JsonUtility.FromJson<List<TestData>>(json);
    }

    void DisplayTest(int index)
    {
        if (index < 0 || index >= testDatabase.Count) return;
        currentTest = testDatabase[index];

        // Load and set the test image
        Texture2D texture = Resources.Load<Texture2D>(currentTest.imagePath);
        testImage.texture = texture;

        // Set up answer buttons
        for (int i = 0; i < answerButtons.Count; i++)
        {
            int answerIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(answerIndex));
        }
    }

    void CheckAnswer(int answerIndex)
    {
        if (answerIndex == currentTest.correctAnswerIndex)
        {
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Incorrect!");
        }

        // Load next test or transition to game UI
        currentTestIndex++;
        if (currentTestIndex < testDatabase.Count)
        {
            DisplayTest(currentTestIndex);
        }
        else
        {
            EndTests();
        }
    }

    void EndTests()
    {
        // Hide the test UI and show the game UI
        testUI.SetActive(false);
        gameUI.SetActive(true);

        // Save initial session data to Firebase
        SaveInitialSessionData();

        // Call GameManager to start the game
        GameManager.instance.StartGame();
    }

    void SaveInitialSessionData()
    {
        if (user == null) return;

        string userId = user.UserId;
        int gameIndex = Random.Range(0, 3); // Randomly choose a game (0: Flow, 1: One Line, 2: Plumber)

        SessionData sessionData = new SessionData
        {
            userId = userId,
            gameIndex = gameIndex,
            levelsComplete = null,
            scoreImprovement = null
        };

        string json = JsonUtility.ToJson(sessionData);
        dbReference.Child("sessions").Push().SetRawJsonValueAsync(json);
    }

    public void SavePostGameSessionData(int levelsComplete, int scoreImprovement)
    {
        if (user == null) return;

        // Retrieve the last session added for the current user
        dbReference.Child("sessions").OrderByChild("userId").EqualTo(user.UserId).LimitToLast(1).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot child in snapshot.Children)
                {
                    child.Reference.Child("levelsComplete").SetValueAsync(levelsComplete);
                    child.Reference.Child("scoreImprovement").SetValueAsync(scoreImprovement);
                }
            }
        });
    }
}

[System.Serializable]
public class SessionData
{
    public string userId;
    public int gameIndex;
    public int? levelsComplete;
    public int? scoreImprovement;
}
