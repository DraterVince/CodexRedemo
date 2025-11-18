using UnityEngine;
using System;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;
    public Supabase.Client Supabase { get; private set; }
    
    private static TaskCompletionSource<bool> loginTaskSource;
    private static TaskCompletionSource<bool> registerTaskSource;
    private static TaskCompletionSource<bool> googleSignInTaskSource;

    private void Awake()
    {
  if (Instance == null)
  {
     Instance = this;
 DontDestroyOnLoad(gameObject);
     }
    else
        {
     Destroy(gameObject);
        }
    }

    private async void Start()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Supabase = new Supabase.Client(
    "https://bpjyqsfggliwehnqcbhy.supabase.co",
   "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJwanlxc2ZnZ2xpd2VobnFjYmh5Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTQ3OTQ4MjksImV4cCI6MjA3MDM3MDgyOX0.0Kfae4pN-paqxcRAycmQIhuAHrGZe2gs6xTKoAy7-5c"
        );

    await Supabase.InitializeAsync();
#endif
    }

    public async Task<bool> Register(string email, string password, string confirmPassword, string username)
    {
        if (password != confirmPassword)
        {
      return false;
   }

     if (string.IsNullOrEmpty(username))
 {
    return false;
 }

#if !UNITY_WEBGL || UNITY_EDITOR
        try
 {
         var session = await Supabase.Auth.SignUp(email, password);

 if (session?.User == null)
{
      return false;
 }

            if (PlayerDataManager.Instance != null)
      {
  await PlayerDataManager.Instance.CreatePlayerData(
     session.User.Id,
          email,
 username
       );
}

       return true;
        }
        catch (Exception)
        {
            return false;
   }
#else
        SetLocalStorage("pendingUsername", username);
   SetLocalStorage("pendingEmail", email);
  
        registerTaskSource = new TaskCompletionSource<bool>();
  SupabaseRegister(email, password);
        return await registerTaskSource.Task;
#endif
    }

    public async Task<bool> Login(string email, string password)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
try
        {
     var session = await Supabase.Auth.SignIn(email, password);

     if (session?.User == null)
  {
       return false;
       }

   if (PlayerDataManager.Instance != null)
   {
 await PlayerDataManager.Instance.LoadPlayerData(session.User.Id);
    }

       return true;
        }
        catch (Exception)
        {
            return false;
        }
#else
        loginTaskSource = new TaskCompletionSource<bool>();
SupabaseLogin(email, password);
   
        var result = await loginTaskSource.Task;
    return result;
#endif
    }
    
    /// <summary>
    /// NEW: Google Sign-In
    /// </summary>
    public async Task<bool> SignInWithGoogle()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debug.LogWarning("[AuthManager] Google Sign-In only works in WebGL builds");
        await Task.CompletedTask; // Satisfy async requirement
        return false;
#else
        googleSignInTaskSource = new TaskCompletionSource<bool>();
        SupabaseGoogleSignIn();
        return await googleSignInTaskSource.Task;
#endif
    }
    
    /// <summary>
    /// NEW: Guest Mode Login
    /// Creates a local-only account that doesn't sync to Supabase
    /// </summary>
    public async Task<bool> LoginAsGuest()
    {
        try
        {
     // Generate a unique guest ID
      string guestId = "guest_" + System.Guid.NewGuid().ToString();
      string guestUsername = "Guest" + UnityEngine.Random.Range(1000, 9999);
      
            Debug.Log($"[AuthManager] Creating guest account: {guestUsername}");
            
            // Mark as guest mode
       PlayerPrefs.SetInt("IsGuestMode", 1);
PlayerPrefs.SetString("GuestId", guestId);
  PlayerPrefs.SetString("username", guestUsername);
            PlayerPrefs.Save();
          
        // Create local player data (doesn't sync to Supabase)
   if (PlayerDataManager.Instance != null)
        {
           await PlayerDataManager.Instance.CreateGuestPlayerData(guestId, guestUsername);
            }
         
 return true;
        }
        catch (Exception ex)
        {
        Debug.LogError($"[AuthManager] Guest login failed: {ex.Message}");
   return false;
        }
    }
    
    /// <summary>
    /// Check if current session is guest mode
    /// </summary>
    public bool IsGuestMode()
    {
return PlayerPrefs.GetInt("IsGuestMode", 0) == 1;
    }

    public async Task Logout()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
      try
        {
            if (PlayerDataManager.Instance != null)
       {
      await PlayerDataManager.Instance.SyncGameData();
          }

            await Supabase.Auth.SignOut();
        ClearUserData();
   }
        catch (Exception)
        {
        }
#else
        SupabaseLogout();
#endif
    }

    private void ClearUserData()
    {
     Debug.Log("[AuthManager] ClearUserData called");
   
 PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("email");
  PlayerPrefs.DeleteKey("levelAt");
   PlayerPrefs.DeleteKey("moneyCount");
   PlayerPrefs.DeleteKey("unlockedCosmetics");
    PlayerPrefs.DeleteKey("IsGuestMode");
        PlayerPrefs.DeleteKey("GuestId");
        
        // CRITICAL: Set flag to prevent auto-login on next scene load
        PlayerPrefs.SetInt("JustLoggedOut", 1);
        
 if (NewAndLoadGameManager.Instance != null)
   {
    NewAndLoadGameManager.Instance.ClearAllSlots();
         Debug.Log("[AuthManager] Cleared all save slots");
    }

        if (PlayerDataManager.Instance != null)
      {
       PlayerDataManager.Instance.ClearPlayerDataCache();
   Debug.Log("[AuthManager] Cleared PlayerDataManager cache");
    }
        
     PlayerPrefs.Save();
        Debug.Log("[AuthManager] PlayerPrefs saved and cleared");
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SupabaseRegister(string email, string password);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SupabaseLogin(string email, string password);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SupabaseGoogleSignIn();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SupabaseLogout();
  
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void getCurrentUser();
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetLocalStorage(string key, string value);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string GetLocalStorage(string key);
#endif

    public void OnRegisterSuccess(string jsonData)
    {
        try
     {
     var authResponse = JsonUtility.FromJson<SupabaseAuthResponse>(jsonData);
   if (authResponse != null && authResponse.user != null)
   {
#if UNITY_WEBGL && !UNITY_EDITOR
   string username = GetLocalStorage("pendingUsername");
    string email = GetLocalStorage("pendingEmail");
#else
string username = PlayerPrefs.GetString("pendingUsername", "Player");
 string email = PlayerPrefs.GetString("pendingEmail", authResponse.user.email);
#endif
   
     if (string.IsNullOrEmpty(username))
       {
            username = "Player";
      }
       
        if (PlayerDataManager.Instance != null)
        {
            _ = PlayerDataManager.Instance.CreatePlayerData(
                authResponse.user.id,
                email,
                username
            );
        }
   
#if UNITY_WEBGL && !UNITY_EDITOR
       SetLocalStorage("pendingUsername", "");
    SetLocalStorage("pendingEmail", "");
       registerTaskSource?.TrySetResult(true);
#else
       PlayerPrefs.DeleteKey("pendingUsername");
            PlayerPrefs.DeleteKey("pendingEmail");
     PlayerPrefs.Save();
#endif
      }
     }
        catch (Exception)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
          registerTaskSource?.TrySetResult(false);
#endif
}
 }

    public void OnRegisterError(string errorMessage)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
registerTaskSource?.TrySetResult(false);
#endif
    }

    public void OnLoginSuccess(string jsonData)
    {
        try
    {
     var authResponse = JsonUtility.FromJson<SupabaseAuthResponse>(jsonData);
  if (authResponse != null && authResponse.user != null)
     {
            if (PlayerDataManager.Instance != null)
            {
                _ = PlayerDataManager.Instance.LoadPlayerData(authResponse.user.id);
            }
         
#if UNITY_WEBGL && !UNITY_EDITOR
     loginTaskSource?.TrySetResult(true);
#endif
    }
 }
        catch (Exception)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
      loginTaskSource?.TrySetResult(false);
#endif
  }
    }

    public void OnLoginError(string errorMessage)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
     loginTaskSource?.TrySetResult(false);
#endif
    }
    
    /// <summary>
    /// NEW: Google Sign-In Success Callback
    /// </summary>
    public void OnGoogleSignInSuccess(string jsonData)
    {
   try
        {
var authResponse = JsonUtility.FromJson<SupabaseAuthResponse>(jsonData);
       if (authResponse != null && authResponse.user != null)
  {
    Debug.Log($"[AuthManager] ? Google Sign-In successful! User: {authResponse.user.email}");
                
          // Extract username from email or use metadata
    string username = authResponse.user.email.Split('@')[0];
        
 if (PlayerDataManager.Instance != null)
    {
            // Check if player data exists, if not create it
            _ = PlayerDataManager.Instance.LoadOrCreatePlayerData(
                authResponse.user.id,
                authResponse.user.email,
                username
            );
  
        Debug.Log($"[AuthManager] Player data loaded for Google user: {username}");
    }
       
#if UNITY_WEBGL && !UNITY_EDITOR
   googleSignInTaskSource?.TrySetResult(true);
#endif
      }
        else
        {
       Debug.LogError("[AuthManager] Google sign-in response missing user data");
#if UNITY_WEBGL && !UNITY_EDITOR
         googleSignInTaskSource?.TrySetResult(false);
#endif
        }
        }
     catch (Exception ex)
{
       Debug.LogError($"[AuthManager] Google sign-in error: {ex.Message}");
#if UNITY_WEBGL && !UNITY_EDITOR
      googleSignInTaskSource?.TrySetResult(false);
#endif
  }
    }
    
    /// <summary>
  /// NEW: Google Sign-In Error Callback
    /// </summary>
    public void OnGoogleSignInError(string errorMessage)
    {
     Debug.LogError($"[AuthManager] Google sign-in failed: {errorMessage}");
#if UNITY_WEBGL && !UNITY_EDITOR
        googleSignInTaskSource?.TrySetResult(false);
#endif
    }

    public void OnLogoutSuccess(string message)
    {
        Debug.Log("[AuthManager] ====== LOGOUT SUCCESS ======");
        Debug.Log("[AuthManager] Clearing all user data...");
   ClearUserData();
        Debug.Log("[AuthManager] User data cleared successfully");
        Debug.Log("[AuthManager] ====== LOGOUT COMPLETE ======");
  }

    public void OnLogoutError(string errorMessage)
    {
 Debug.LogError($"[AuthManager] ? Logout error: {errorMessage}");
   // Still try to clear local data even if Supabase logout failed
        Debug.LogWarning("[AuthManager] Attempting to clear local data anyway...");
      ClearUserData();
 }

    public void OnGetUserSuccess(string jsonData)
    {
    }

    public void OnGetUserError(string errorMessage)
    {
    }
}

[System.Serializable]
public class SupabaseAuthResponse
{
    public SupabaseUser user;
    public SupabaseSession session;
}

[System.Serializable]
public class SupabaseUser
{
    public string id;
    public string email;
}

[System.Serializable]
public class SupabaseSession
{
    public string access_token;
    public string refresh_token;
}