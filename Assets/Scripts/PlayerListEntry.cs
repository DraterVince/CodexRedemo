using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Individual player entry in the multiplayer lobby
/// Displays player name, ready status, and kick button (for host)
/// </summary>
public class PlayerListEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI readyStatusText;
    [SerializeField] private Button kickButton;
    [SerializeField] private GameObject hostIndicator;

    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.yellow;
    [SerializeField] private Color kickButtonNormalColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color kickButtonDisabledColor = new Color(0.5f, 0.5f, 0.5f);

    private Player player;
    private System.Action<Player> onKickCallback;

    public void Initialize(Player player, bool isHost, bool canKick, System.Action<Player> kickCallback)
    {
        this.player = player;
        this.onKickCallback = kickCallback;

        UpdateDisplay(isHost, canKick);

        if (kickButton != null)
        {
            kickButton.onClick.RemoveAllListeners();
            kickButton.onClick.AddListener(OnKickButtonClicked);
        }
    }

    public void UpdateDisplay(bool isHost, bool canKick)
    {
        if (player == null) return;

        if (playerNameText != null)
        {
            playerNameText.text = player.NickName;
        }

        if (hostIndicator != null)
        {
            hostIndicator.SetActive(isHost);
        }

        UpdateReadyStatus();

        if (kickButton != null)
        {
            kickButton.gameObject.SetActive(canKick);
            kickButton.interactable = canKick;

            var colors = kickButton.colors;
            colors.normalColor = canKick ? kickButtonNormalColor : kickButtonDisabledColor;
            kickButton.colors = colors;
        }
    }

    public void UpdateReadyStatus()
    {
        if (readyStatusText == null || player == null) return;

        bool isReady = false;
        if (player.CustomProperties.ContainsKey("IsReady"))
        {
            isReady = (bool)player.CustomProperties["IsReady"];
        }

        // Use simple ASCII characters that all fonts support
        readyStatusText.text = isReady ? "[READY]" : "[Not Ready]";
        readyStatusText.color = isReady ? readyColor : notReadyColor;
    }

    private void OnKickButtonClicked()
    {
        if (player != null && onKickCallback != null)
        {
            onKickCallback.Invoke(player);
        }
    }

    public Player GetPlayer()
    {
        return player;
    }
}