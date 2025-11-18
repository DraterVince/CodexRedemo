using UnityEngine;
using Photon.Pun;
using TMPro;

/// <summary>
/// Controls networked player behavior
/// Attach this to your player prefab
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Color playerColor = Color.white;

    [Header("Network Settings")]
    [SerializeField] private bool syncPosition = true;
    [SerializeField] private bool syncRotation = true;
    [SerializeField] private float smoothing = 10f;

    private new PhotonView photonView;
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // Custom player properties
    private string playerName;
    private int playerScore;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // If this is the local player
        if (photonView.IsMine)
        {
            // Set player name from PhotonNetwork
            playerName = PhotonNetwork.NickName;
            
            // You can enable local player controls here
            EnablePlayerControls(true);
        }
        else
        {
            // Disable controls for non-local players
            EnablePlayerControls(false);
            
            // Initialize network position
            networkPosition = transform.position;
            networkRotation = transform.rotation;
        }

        // Display player name above character
        if (playerNameText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
        }

        // Set random color (or use player-selected color)
        SetPlayerColor(playerColor);
    }

    private void Update()
    {
        // Smoothly interpolate position for non-local players
        if (!photonView.IsMine && syncPosition)
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * smoothing);
        }

        // Smoothly interpolate rotation for non-local players
        if (!photonView.IsMine && syncRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * smoothing);
        }
    }

    #region IPunObservable Implementation

    /// <summary>
    /// Syncs data across network
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send data to others
            if (syncPosition)
                stream.SendNext(transform.position);
            
            if (syncRotation)
                stream.SendNext(transform.rotation);
            
            stream.SendNext(playerName);
            stream.SendNext(playerScore);
        }
        else
        {
            // Network player, receive data
            if (syncPosition)
                networkPosition = (Vector3)stream.ReceiveNext();
            
            if (syncRotation)
                networkRotation = (Quaternion)stream.ReceiveNext();
            
            playerName = (string)stream.ReceiveNext();
            playerScore = (int)stream.ReceiveNext();

            // Update name display
            if (playerNameText != null)
            {
                playerNameText.text = playerName;
            }
        }
    }

    #endregion

    #region Player Controls

    private void EnablePlayerControls(bool enable)
    {
        // Enable/disable your player control scripts here
        // Example:
        // GetComponent<PlayerMovement>().enabled = enable;
        // GetComponent<PlayerInput>().enabled = enable;
        
        // For now, just log
        Debug.Log($"Player controls {(enable ? "enabled" : "disabled")} for {PhotonNetwork.NickName}");
    }

    #endregion

    #region Player Properties

    /// <summary>
    /// Set player color (synced across network)
    /// </summary>
    public void SetPlayerColor(Color color)
    {
        playerColor = color;
        
        // Apply color to player visual (example with renderer)
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        // If this is our player, sync it
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_SetPlayerColor", RpcTarget.AllBuffered, color.r, color.g, color.b, color.a);
        }
    }

    [PunRPC]
    private void RPC_SetPlayerColor(float r, float g, float b, float a)
    {
        Color color = new Color(r, g, b, a);
        playerColor = color;
        
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    /// <summary>
    /// Add score to player (Master Client only)
    /// </summary>
    public void AddScore(int points)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playerScore += points;
            photonView.RPC("RPC_UpdateScore", RpcTarget.AllBuffered, playerScore);
        }
    }

    [PunRPC]
    private void RPC_UpdateScore(int newScore)
    {
        playerScore = newScore;
        Debug.Log($"{playerName} score: {playerScore}");
    }

    #endregion

    #region Example RPC Methods

    /// <summary>
    /// Example: Send a chat message
    /// </summary>
    public void SendChatMessage(string message)
    {
        photonView.RPC("RPC_ChatMessage", RpcTarget.All, PhotonNetwork.NickName, message);
    }

    [PunRPC]
    private void RPC_ChatMessage(string senderName, string message)
    {
        Debug.Log($"[{senderName}]: {message}");
        // You can display this in a chat UI
    }

    /// <summary>
    /// Example: Play animation across network
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayAnimation", RpcTarget.All, animationName);
        }
    }

    [PunRPC]
    private void RPC_PlayAnimation(string animationName)
    {
        // Play animation
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(animationName);
        }
        
        Debug.Log($"Playing animation: {animationName}");
    }

    #endregion

    #region Photon Callbacks

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Handle custom player properties updates
        if (targetPlayer == photonView.Owner)
        {
            // Update properties for this player
        }
    }

    #endregion
}
