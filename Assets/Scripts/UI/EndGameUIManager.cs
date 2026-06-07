using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class EndGameUIManager : MonoBehaviourPunCallbacks
{
    public static EndGameUIManager Instance;

    [Header("Panels")]
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject failPanel;

    [Header("Win Panel Buttons")]
    public Button winBackToLobbyButton;
    public Button winExitButton;

    [Header("Fail Panel Buttons")]
    public Button failBackToLobbyButton;
    public Button failExitButton;

    private const string WANTS_LOBBY_KEY = "WantsLobby";
    private const string BackLabelFormat = "Lobiye Dön ({0}/{1})";

    // Back-to-lobby button labels, resolved at runtime (panels are inactive at Start,
    // so we must include inactive children).
    private TextMeshProUGUI winBackLabel;
    private TextMeshProUGUI failBackLabel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (gamePanel != null) gamePanel.SetActive(true);
        if (winPanel != null)  winPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);

        if (winBackToLobbyButton != null)
        {
            winBackToLobbyButton.onClick.AddListener(OnBackToLobby);
            winBackLabel = winBackToLobbyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        if (failBackToLobbyButton != null)
        {
            failBackToLobbyButton.onClick.AddListener(OnBackToLobby);
            failBackLabel = failBackToLobbyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (winExitButton != null)  winExitButton.onClick.AddListener(OnExit);
        if (failExitButton != null) failExitButton.onClick.AddListener(OnExit);
    }

    /// <summary>
    /// Called by BasketballGameManager when a player reaches the score threshold.
    /// </summary>
    public void ShowEndGame(bool isWinner)
    {
        Debug.Log($"EndGameUIManager: ShowEndGame called. IsWinner={isWinner}");

        if (gamePanel != null) gamePanel.SetActive(false);

        if (isWinner)
        {
            if (winPanel != null)  winPanel.SetActive(true);
            if (failPanel != null) failPanel.SetActive(false);
        }
        else
        {
            if (winPanel != null)  winPanel.SetActive(false);
            if (failPanel != null) failPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fresh confirmation state for this end-game round.
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { WANTS_LOBBY_KEY, false } });
        SetBackButtonsInteractable(true);
        RefreshLobbyCount();
    }

    private void OnBackToLobby()
    {
        Debug.Log("EndGameUIManager: Back to Lobby confirmed. Waiting for all players...");

        // This client confirms; the actual scene load waits until EVERYONE has confirmed.
        SetBackButtonsInteractable(false);

        Hashtable props = new Hashtable
        {
            { WANTS_LOBBY_KEY, true  },
            { "Score",        0      },
            { "IsReady",      false  },
            { "RPS_Choice",  -1      }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        RefreshLobbyCount();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!changedProps.ContainsKey(WANTS_LOBBY_KEY)) return;

        RefreshLobbyCount();
        TryLoadLobby();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // A leaving player lowers the required total; re-evaluate so a lone player isn't stuck.
        RefreshLobbyCount();
        TryLoadLobby();
    }

    private void TryLoadLobby()
    {
        if (PhotonNetwork.IsMasterClient && AllPlayersWantLobby())
        {
            // Reset shared room state, then everyone follows the master via AutomaticallySyncScene.
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "RPSWinner", 0 } });
            Invoke(nameof(LoadLobbyScene), 0.5f);
        }
    }

    private int CountPlayersWantingLobby()
    {
        int count = 0;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue(WANTS_LOBBY_KEY, out object v) && (bool)v)
                count++;
        }
        return count;
    }

    private bool AllPlayersWantLobby()
    {
        if (PhotonNetwork.CurrentRoom == null) return false;
        int total = PhotonNetwork.CurrentRoom.PlayerCount;
        return total > 0 && CountPlayersWantingLobby() >= total;
    }

    private void RefreshLobbyCount()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        int count = CountPlayersWantingLobby();
        int total = PhotonNetwork.CurrentRoom.PlayerCount;
        string label = string.Format(BackLabelFormat, count, total);

        if (winBackLabel != null)  winBackLabel.text = label;
        if (failBackLabel != null) failBackLabel.text = label;
    }

    private void SetBackButtonsInteractable(bool interactable)
    {
        if (winBackToLobbyButton != null)  winBackToLobbyButton.interactable = interactable;
        if (failBackToLobbyButton != null) failBackToLobbyButton.interactable = interactable;
    }

    private void LoadLobbyScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("EndGameUIManager: All confirmed. Loading LobbyScene...");
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }

    private void OnExit()
    {
        Debug.Log("EndGameUIManager: Exit clicked. Quitting application.");
        PhotonNetwork.Disconnect();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
