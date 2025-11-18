using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    [Header("Profile UI")]
    public Text userEmailText;
    public Button logoutButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Setup logout button
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        
        // Display current user email
        string email = PlayerPrefs.GetString("email", "Not logged in");
        SetUser(email);
    }

    public void SetUser(string email)
    {
        Debug.Log("User set: " + email);
        if (userEmailText != null)
            userEmailText.text = "Logged in as: " + email;
    }

    private async void OnLogoutClicked()
    {
        Debug.Log("Logout button clicked");
        
        // Show confirmation dialog (optional)
        // You can add a confirmation panel here if desired
        
        if (AuthManager.Instance != null)
        {
            await AuthManager.Instance.Logout();
            
            // Go back to login screen
            SceneManager.LoadScene("LoginScene"); // Change to your login scene name
        }
        else
        {
            Debug.LogError("AuthManager not found!");
        }
    }
}