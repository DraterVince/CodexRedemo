using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIAuth : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_InputField usernameInput;

    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;

    public Button registerButton;
    public Button loginButton;

    public GameObject regSucPanel;
    public GameObject regFailPanel;

    public GameObject logSucPanel;
    public GameObject logFailPanel;
    
    [Header("Additional Auth Buttons")]
    public Button googleSignInButton;
    public Button guestButton;
    
    [Header("WebGL Loading (Optional)")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;
    
    [Header("Scene Navigation")]
    public string sceneToLoadAfterLogin = "MainMenu";
    public float delayBeforeSceneLoad = 1f; // Reduced from 2f to 1f for faster response
  
    private bool hasEnabledButtons = false;
    private bool isCheckingExistingSession = false;

    private void Start()
    {
        if (AuthManager.Instance == null)
        {
            return;
        }

        // Setup new buttons
  if (googleSignInButton != null)
 {
       googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);
   }
     
      if (guestButton != null)
  {
    guestButton.onClick.AddListener(OnGuestClicked);
}

#if UNITY_WEBGL && !UNITY_EDITOR
    if (loadingPanel != null) loadingPanel.SetActive(true);
        if (loadingText != null) loadingText.text = "Initializing Supabase...";

  if (registerButton != null) registerButton.interactable = false;
  if (loginButton != null) loginButton.interactable = false;
   
 // CRITICAL: Check if user just logged out
        int justLoggedOut = PlayerPrefs.GetInt("JustLoggedOut", 0);
        if (justLoggedOut == 1)
     {
    Debug.Log("[UIAuth] User just logged out - skipping auto-login");
      PlayerPrefs.DeleteKey("JustLoggedOut");
   PlayerPrefs.Save();
      EnableButtons();
        return;
   }
   
 if (SupabaseReadyManager.IsSupabaseReady())
    {
   // CRITICAL FIX: Start session check using coroutine
   StartSessionCheck();
  }
   else
        {
       StartCoroutine(CheckSupabaseReady());
  }
#else
        EnableButtons();
#endif
    }

    private System.Collections.IEnumerator CheckSupabaseReady()
    {
        int attempts = 0;
while (!SupabaseReadyManager.IsSupabaseReady() && attempts < 100)
        {
            yield return new WaitForSeconds(0.1f);
     attempts++;
   }
  
        if (SupabaseReadyManager.IsSupabaseReady())
   {
      // CRITICAL FIX: Start session check using coroutine
       StartSessionCheck();
        }
  else
 {
   if (loadingText != null) loadingText.text = "Failed to initialize. Please refresh.";
   }
    }
    
  /// <summary>
    /// CRITICAL FIX: Start the session check coroutine
    /// </summary>
    private void StartSessionCheck()
    {
     if (!isCheckingExistingSession)
        {
      isCheckingExistingSession = true;
  StartCoroutine(CheckForExistingSessionEnumerator());
   }
    }
    
    /// <summary>
    /// CRITICAL FIX: Check if user is already logged in (after Google OAuth redirect)
    /// </summary>
    private void CheckForExistingSessionCo()
    {
   StartCoroutine(CheckForExistingSessionEnumerator());
    }
    
    /// <summary>
    /// CRITICAL FIX: Check for existing Supabase session and auto-login
  /// Using Coroutine instead of async Task because WebGL has issues with async/await
    /// </summary>
    private System.Collections.IEnumerator CheckForExistingSessionEnumerator()
    {
        if (loadingText != null) loadingText.text = "Checking session...";
   
        Debug.Log("[UIAuth] Checking for existing session...");
   
#if UNITY_WEBGL && !UNITY_EDITOR
        // Call JavaScript to check if user is already logged in
        Application.ExternalCall("getCurrentUser");
    
  // Wait 1.5 seconds for JavaScript callback to complete
        Debug.Log("[UIAuth] Waiting for JavaScript callback to complete...");
      yield return new WaitForSeconds(1.5f);
    
        Debug.Log("[UIAuth] ===== DELAY COMPLETE, STARTING CHECKS =====");
    
        // Check multiple times over 2.5 seconds (reduced for faster response)
        int attempts = 0;
        int maxAttempts = 5;
    
        while (attempts < maxAttempts)
        {
            attempts++;
        
            Debug.Log($"[UIAuth] Session check attempt {attempts}/{maxAttempts}...");
        
            // Log current state of PlayerDataManager
            if (PlayerDataManager.Instance != null)
          {
 var currentData = PlayerDataManager.Instance.GetCurrentPlayerData();
         if (currentData != null)
   {
           Debug.Log($"[UIAuth] ? PlayerData found! User: {currentData.username}, ID: {currentData.user_id}");
         
       // SUCCESS - User data is loaded, redirect to main menu
    if (loadingText != null) loadingText.text = "Welcome back! Loading...";
        
       // Set user ID (if NewAndLoadGameManager exists)
     if (NewAndLoadGameManager.Instance != null)
   {
          NewAndLoadGameManager.Instance.SetUserId(currentData.user_id);
     Debug.Log($"[UIAuth] User ID set: {currentData.user_id}");
          }
         else
       {
             Debug.LogWarning("[UIAuth] NewAndLoadGameManager.Instance is null!");
  }
      
       yield return new WaitForSeconds(0.5f);
  
         // Redirect to main menu
             Debug.Log($"[UIAuth] Loading scene: {sceneToLoadAfterLogin}");
           SceneManager.LoadScene(sceneToLoadAfterLogin);
        yield break;
                }
           else
  {
             Debug.Log("[UIAuth] PlayerDataManager exists but GetCurrentPlayerData() is null");
       }
 }
       else
        {
        Debug.Log("[UIAuth] PlayerDataManager.Instance is null");
   }
   
      // Wait before next attempt
            yield return new WaitForSeconds(0.5f);
        }
    
  Debug.Log("[UIAuth] ? No session found after all attempts");
#endif
    
  // No existing session - show login screen
        Debug.Log("[UIAuth] No existing session found, showing login screen");
     EnableButtons();
        yield break;
    }

    private void EnableButtons()
    {
   if (hasEnabledButtons) return;
        hasEnabledButtons = true;
     
        if (registerButton != null) registerButton.interactable = true;
        if (loginButton != null) loginButton.interactable = true;
 
      if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public async void OnRegisterClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!SupabaseReadyManager.IsSupabaseReady())
        {
  if (loadingText != null) loadingText.text = "Please wait...";
    return;
        }
#endif

        if (string.IsNullOrEmpty(emailInput.text))
      {
            regFailPanel.SetActive(true);
            regSucPanel.SetActive(false);
            return;
      }

        if (string.IsNullOrEmpty(usernameInput.text))
        {
   regFailPanel.SetActive(true);
            regSucPanel.SetActive(false);
        return;
        }

        if (string.IsNullOrEmpty(passwordInput.text))
  {
            regFailPanel.SetActive(true);
        regSucPanel.SetActive(false);
     return;
        }

        if (AuthManager.Instance == null)
 {
            regFailPanel.SetActive(true);
  regSucPanel.SetActive(false);
     return;
        }

        bool success = await AuthManager.Instance.Register(
       emailInput.text, 
       passwordInput.text, 
            confirmPasswordInput.text,
      usernameInput.text
        );
   
        if (success)
    {
  regSucPanel.SetActive(true);
            regFailPanel.SetActive(false);
        }
        else
        {
 regSucPanel.SetActive(false);
         regFailPanel.SetActive(true);
}
    }

    public async void OnLoginClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
   if (!SupabaseReadyManager.IsSupabaseReady())
{
   if (loadingText != null) loadingText.text = "Please wait...";
    return;
        }
#endif
      
   bool success = await AuthManager.Instance.Login(loginEmailInput.text, loginPasswordInput.text);
   
 if (success)
        {
    logSucPanel.SetActive(true);
     logFailPanel.SetActive(false);
      
if (loadingPanel != null) 
  {
   loadingPanel.SetActive(true);
    if (loadingText != null) loadingText.text = "Loading...";
  }
        
   await Task.Delay((int)(delayBeforeSceneLoad * 1000));
    
         if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
      {
     string userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
    
    if (NewAndLoadGameManager.Instance != null)
{
NewAndLoadGameManager.Instance.SetUserId(userId);
 Debug.Log("[UIAuth] User ID set in NewAndLoadGameManager");
   }
         }
 
 if (loadingText != null) loadingText.text = $"Loading {sceneToLoadAfterLogin}...";
            
   SceneManager.LoadScene(sceneToLoadAfterLogin);
      }
        else
  {
     logFailPanel.SetActive(true);
        logSucPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Google Sign-In Handler - OPTIMIZED FOR SPEED
    /// </summary>
    public async void OnGoogleSignInClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!SupabaseReadyManager.IsSupabaseReady())
        {
            if (loadingText != null) loadingText.text = "Please wait...";
            return;
        }
        
        Debug.Log("[UIAuth] Google Sign-In button clicked");
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingText != null) loadingText.text = "Opening Google Sign-In...";
        }
        
        // Disable buttons to prevent double-click
        if (googleSignInButton != null) googleSignInButton.interactable = false;
        if (loginButton != null) loginButton.interactable = false;
        if (registerButton != null) registerButton.interactable = false;
        
        Debug.Log("[UIAuth] Calling AuthManager.SignInWithGoogle()...");
        
        // The popup will handle the auth - this returns quickly once popup closes
        bool success = await AuthManager.Instance.SignInWithGoogle();
        
  Debug.Log($"[UIAuth] Google Sign-In result: {success}");
     
        if (success)
        {
 Debug.Log("[UIAuth] ? Google Sign-In successful!");
      
         if (loadingText != null) loadingText.text = "Loading...";
            
  // Quick wait for PlayerDataManager (reduced from 30 attempts to 10)
      int attempts = 0;
            while (attempts < 10) // Max 1 second wait
            {
         if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
    {
           break;
                }
await Task.Delay(100);
attempts++;
     }
            
            // Set user ID
  if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
            {
                string userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
       string userName = PlayerDataManager.Instance.GetCurrentPlayerData().username;
    
     Debug.Log($"[UIAuth] Setting user ID: {userId}, Username: {userName}");
     
          if (NewAndLoadGameManager.Instance != null)
 {
               NewAndLoadGameManager.Instance.SetUserId(userId);
       Debug.Log("[UIAuth] User ID set in NewAndLoadGameManager");
     }
            }
else
         {
      Debug.LogWarning("[UIAuth] PlayerDataManager not ready - proceeding anyway");
 }
  
      // Load main menu quickly
            if (loadingText != null) loadingText.text = $"Loading {sceneToLoadAfterLogin}...";
 
            // Reduced delay for faster transition
            await Task.Delay((int)(delayBeforeSceneLoad * 1000));
            
         Debug.Log($"[UIAuth] Loading scene: {sceneToLoadAfterLogin}");
     SceneManager.LoadScene(sceneToLoadAfterLogin);
 }
      else
 {
          Debug.LogError("[UIAuth] ? Google Sign-In failed!");
       if (loadingPanel != null) loadingPanel.SetActive(false);
       
            if (logFailPanel != null)
            {
   logFailPanel.SetActive(true);
      }
      
     // Re-enable buttons
            if (googleSignInButton != null) googleSignInButton.interactable = true;
            if (loginButton != null) loginButton.interactable = true;
       if (registerButton != null) registerButton.interactable = true;
        }
#else
        Debug.LogWarning("[UIAuth] Google Sign-In only works in WebGL builds");
        await Task.CompletedTask; // Satisfy async requirement
#endif
    }

    /// <summary>
    /// Guest Mode Handler
  /// </summary>
    public async void OnGuestClicked()
    {
        if (loadingPanel != null)
        {
       loadingPanel.SetActive(true);
            if (loadingText != null) loadingText.text = "Creating guest account...";
        }
    
     bool success = await AuthManager.Instance.LoginAsGuest();
        
        if (success)
        {
  Debug.Log("[UIAuth] Guest login successful!");
            
            // Guest accounts have local user ID
  string guestId = PlayerPrefs.GetString("GuestId", "");
       if (NewAndLoadGameManager.Instance != null && !string.IsNullOrEmpty(guestId))
   {
        NewAndLoadGameManager.Instance.SetUserId(guestId);
            }
         
    // Load main menu
        await Task.Delay((int)(delayBeforeSceneLoad * 1000));
     SceneManager.LoadScene(sceneToLoadAfterLogin);
        }
        else
 {
          Debug.LogError("[UIAuth] Guest login failed!");
            if (loadingPanel != null) loadingPanel.SetActive(false);
  }
    }
}
