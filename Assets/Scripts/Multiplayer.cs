using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MultiplayerSceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string multiplayerLobbyScene = "Multiplayer";
    [SerializeField] private string mainMenuScene = "MainMenu";

    /// <summary>
    /// Load the multiplayer lobby scene
    /// </summary>
    public void LoadMultiplayerLobby()
    {
        Debug.Log("Loading Multiplayer Lobby...");
        
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(multiplayerLobbyScene);
            }
            else
            {
                Debug.LogWarning("[MultiplayerSceneLoader] Only the Master Client can change scenes while in a room.");
            }
        }
        else
        {
            SceneManager.LoadSceneAsync(multiplayerLobbyScene);
        }
    }

    /// <summary>
    /// Return to main menu (disconnects from Photon)
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Log stack trace to see what's calling this
        Debug.LogWarning($"[MultiplayerSceneLoader] ReturnToMainMenu() called! Stack trace:\n{System.Environment.StackTrace}");
        
        // Disconnect from Photon if connected
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.Disconnect();
        }

        Debug.Log("Returning to Main Menu...");
        SceneManager.LoadSceneAsync(mainMenuScene);
    }

    /// <summary>
    /// Quick join multiplayer (connects and joins random room)
    /// </summary>
    public void QuickJoinMultiplayer()
    {
        // Load multiplayer scene
        LoadMultiplayerLobby();

        // After scene loads, auto-connect and join
        StartCoroutine(AutoConnectAfterSceneLoad());
    }

    private IEnumerator AutoConnectAfterSceneLoad()
    {
        // Wait for scene to load
        yield return new WaitForSeconds(1f);

        // Set default nickname if not set
        if (NetworkManager.Instance != null)
        {
            string nickname = PlayerPrefs.GetString("PlayerNickname", "Player_" + Random.Range(1000, 9999));
            NetworkManager.Instance.SetPlayerNickname(nickname);
            
            // Connect to Photon
            NetworkManager.Instance.ConnectToPhoton();
        }
    }
}

// Keep old class for backwards compatibility
[System.Obsolete("Use MultiplayerSceneLoader instead")]
public class LoadGame : MonoBehaviour
{
    public void LoadLoadGame()
    {
        SceneManager.LoadSceneAsync("Multiplayer");
    }
}