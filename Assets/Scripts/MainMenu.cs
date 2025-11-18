using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button logoutButton;
    public Text userEmailText; // Optional: shows logged in user
    public Button leaderboardButton; // Button to open leaderboard
    
    [Header("Leaderboard")]
    public LeaderboardPanel leaderboardPanel; // Reference to leaderboard panel script

    private void Start()
    {
        // Setup logout button
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        
        // Setup leaderboard button
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        }
        
        EnsureUserIdSet();
        // Display user email (optional)
        DisplayUserInfo();
        
        // Start menu music using MusicManager
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMenuMusic(); // Uses menuMusic if assigned, otherwise uses battleMusic
        }
    }
    
    /// <summary>
    /// Handle leaderboard button click
    /// </summary>
    private void OnLeaderboardClicked()
    {
        Debug.Log("[MainMenu] Leaderboard button clicked");
        
        if (leaderboardPanel != null)
        {
            Debug.Log("[MainMenu] Calling leaderboardPanel.OpenLeaderboard()");
            // Pass the button reference so LeaderboardPanel can hide it after verifying panel is visible
            leaderboardPanel.OpenLeaderboard(leaderboardButton);
        }
        else
        {
            Debug.LogError("[MainMenu] LeaderboardPanel script not assigned! Make sure to assign it in the Inspector.");
        }
    }
    
    /// <summary>
    /// Show the leaderboard button (called by LeaderboardPanel when closed)
    /// </summary>
    public void ShowLeaderboardButton()
    {
        if (leaderboardButton != null)
        {
            leaderboardButton.gameObject.SetActive(true);
        }
    }

    private void EnsureUserIdSet()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
        {
            string userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
            
            if (NewAndLoadGameManager.Instance != null && !string.IsNullOrEmpty(userId))
            {
                NewAndLoadGameManager.Instance.SetUserId(userId);
            }
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    /// <summary>
    /// Display logged in user information
    /// </summary>
    private void DisplayUserInfo()
    {
        string email = PlayerPrefs.GetString("email", "");
        
        if (!string.IsNullOrEmpty(email) && userEmailText != null)
        {
            userEmailText.text = "Logged in as: " + email;
        }
        else if (userEmailText != null)
        {
            userEmailText.text = "Not logged in";
        }
    }

    /// <summary>
    /// Handle logout button click
    /// </summary>
    private async void OnLogoutClicked()
    {
        Debug.Log("Logout button clicked from Main Menu");
        
        if (AuthManager.Instance != null)
        {
            // Perform logout (clears all data)
            await AuthManager.Instance.Logout();
            
            // Go back to login screen
            SceneManager.LoadScene("LoginScene"); // Change to your login scene name
        }
        else
        {
            Debug.LogError("AuthManager not found! Cannot logout.");
        }
    }
}