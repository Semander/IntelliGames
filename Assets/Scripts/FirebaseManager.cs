using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using System;

public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference dbReference;

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    [Header("UserData")]
    public TMP_InputField usernameField;
    public TMP_InputField iqImproveField;
    public TMP_InputField gameField;
    public TMP_InputField scoreField;
    public GameObject scoreElement;
    public Transform scoreboardContent;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void ClearLoginFields()
    {
        emailLoginField.text = string.Empty;
        passwordLoginField.text = string.Empty;
    }

    public void ClearRegisterFields()
    {
        usernameRegisterField.text = string.Empty;
        emailRegisterField.text = string.Empty;
        passwordRegisterField.text = string.Empty;
        passwordRegisterVerifyField.text = string.Empty;
    }

    public void LoginButton() => StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    public void RegisterButton() => StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    public void SignOutButton()
    {
        auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearRegisterFields();
        ClearLoginFields();
    }
    public void SaveDataButton()
    {
        StartCoroutine(UpdateUserData(usernameField.text, int.Parse(iqImproveField.text), int.Parse(gameField.text), int.Parse(scoreField.text)));
    }
    public void ScoreboardButton() => StartCoroutine(LoadScoreboardData());

    private IEnumerator Login(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            HandleAuthError(loginTask.Exception, warningLoginText, "Login Failed!");
        }
        else
        {
            user = loginTask.Result.User;
            Debug.Log($"User signed in successfully: {user.DisplayName} ({user.Email})");
            warningLoginText.text = string.Empty;
            confirmLoginText.text = "Logged In";
            StartCoroutine(LoadUserData());

            yield return new WaitForSeconds(2);

            usernameField.text = user.DisplayName;
            UIManager.instance.UserDataScreen();
            confirmLoginText.text = string.Empty;
            ClearLoginFields();
            ClearRegisterFields();
        }
    }

    private IEnumerator Register(string email, string password, string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (password != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                HandleAuthError(registerTask.Exception, warningRegisterText, "Register Failed!");
            }
            else
            {
                user = registerTask.Result.User;

                if (user != null)
                {
                    var profile = new UserProfile { DisplayName = username };
                    var profileTask = user.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(() => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        Debug.LogWarning($"Failed to set username: {profileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = string.Empty;
                        ClearRegisterFields();
                        ClearLoginFields();
                    }
                }
            }
        }
    }

    private IEnumerator UpdateUserData(string username, int iqImprove, int game, int score)
    {
        yield return UpdateDatabaseValue("username", username);
        yield return UpdateDatabaseValue("IQimprove", iqImprove);
        yield return UpdateDatabaseValue("game", game);
        yield return UpdateDatabaseValue("score", score);
    }

    private IEnumerator UpdateDatabaseValue<T>(string key, T value)
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).Child(key).SetValueAsync(value);
        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogWarning($"Failed to update {key}: {dbTask.Exception}");
        }
    }

    private IEnumerator LoadUserData()
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load user data: {dbTask.Exception}");
        }
        else if (dbTask.Result.Value == null)
        {
            iqImproveField.text = "0";
            gameField.text = "0";
            scoreField.text = "0";
        }
        else
        {
            var snapshot = dbTask.Result;
            iqImproveField.text = snapshot.Child("IQimprove").Value.ToString();
            gameField.text = snapshot.Child("game").Value.ToString();
            scoreField.text = snapshot.Child("score").Value.ToString();
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        var dbTask = dbReference.Child("users").OrderByChild("game").GetValueAsync();
        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load scoreboard data: {dbTask.Exception}");
        }
        else
        {
            foreach (Transform child in scoreboardContent)
            {
                Destroy(child.gameObject);
            }

            var snapshot = dbTask.Result;
            foreach (var childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                var username = childSnapshot.Child("username").Value.ToString();
                var game = int.Parse(childSnapshot.Child("game").Value.ToString());
                var score = int.Parse(childSnapshot.Child("score").Value.ToString());
                var iqImprove = int.Parse(childSnapshot.Child("IQimprove").Value.ToString());

                var scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, game, score, iqImprove);
            }

            UIManager.instance.ScoreboardScreen();
        }
    }

    private void HandleAuthError(AggregateException exception, TMP_Text warningText, string defaultMessage)
    {
        var firebaseEx = exception.GetBaseException() as FirebaseException;
        var errorCode = (AuthError)firebaseEx.ErrorCode;

        var message = defaultMessage;
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "Missing Email";
                break;
            case AuthError.MissingPassword:
                message = "Missing Password";
                break;
            case AuthError.WrongPassword:
                message = "Wrong Password";
                break;
            case AuthError.InvalidEmail:
                message = "Invalid Email";
                break;
            case AuthError.UserNotFound:
                message = "Account does not exist";
                break;
            case AuthError.WeakPassword:
                message = "Weak Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Email Already In Use";
                break;
        }
        warningText.text = message;
    }
}
