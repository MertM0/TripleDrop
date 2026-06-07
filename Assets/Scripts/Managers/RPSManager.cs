using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RPSManager : MonoBehaviourPunCallbacks
{
    public static RPSManager Instance;

    [Header("RPS UI")]
    public GameObject rpsPanel;
    public TextMeshProUGUI statusText;
    public Button rockButton;
    public Button paperButton;
    public Button scissorsButton;

    // RPS choice constants
    private const int ROCK = 0;
    private const int PAPER = 1;
    private const int SCISSORS = 2;
    private const string RPS_CHOICE_KEY = "RPS_Choice";
    private const string RPS_WINNER_KEY = "RPSWinner";

    private bool hasChosen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (rpsPanel != null)
            rpsPanel.SetActive(false);
    }

    /// <summary>
    /// Called by LobbyManager when all players are ready.
    /// </summary>
    public void StartRPS()
    {
        hasChosen = false;

        Hashtable clearProps = new Hashtable { { RPS_CHOICE_KEY, -1 } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(clearProps);

        if (rpsPanel != null) rpsPanel.SetActive(true);
        if (statusText != null) statusText.text = "Choose your move!";

        SetButtonsInteractable(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("RPSManager: RPS panel opened.");
    }

    public void OnChooseRock()     => SubmitChoice(ROCK);
    public void OnChoosePaper()    => SubmitChoice(PAPER);
    public void OnChooseScissors() => SubmitChoice(SCISSORS);

    private void SubmitChoice(int choice)
    {
        if (hasChosen) return;
        hasChosen = true;

        SetButtonsInteractable(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        string choiceName = choice == ROCK ? "Rock" : choice == PAPER ? "Paper" : "Scissors";
        if (statusText != null) statusText.text = $"You chose {choiceName}. Waiting for opponent...";
        Debug.Log($"RPSManager: Local player chose {choiceName}.");

        Hashtable props = new Hashtable { { RPS_CHOICE_KEY, choice } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!changedProps.ContainsKey(RPS_CHOICE_KEY)) return;

        // Check if all players have made a choice (choice != -1)
        bool allChosen = true;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(RPS_CHOICE_KEY, out object choiceObj))
            {
                if ((int)choiceObj == -1)
                {
                    allChosen = false;
                    break;
                }
            }
            else
            {
                allChosen = false;
                break;
            }
        }

        if (allChosen && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            EvaluateResult();
        }
    }

    private void EvaluateResult()
    {
        // Collect choices
        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length < 2) return;

        Player p1 = players[0];
        Player p2 = players[1];

        int c1 = (int)p1.CustomProperties[RPS_CHOICE_KEY];
        int c2 = (int)p2.CustomProperties[RPS_CHOICE_KEY];

        Debug.Log($"RPSManager: Player {p1.ActorNumber} chose {c1}, Player {p2.ActorNumber} chose {c2}");

        if (c1 == c2)
        {
            // Draw
            Debug.Log("RPSManager: Draw! Restarting RPS...");
            if (statusText != null) statusText.text = "Draw! Choose again...";

            // Reset choices and restart
            Invoke(nameof(ResetAndRestart), 1.5f);
        }
        else
        {
            // Determine winner: Rock(0) beats Scissors(2), Paper(1) beats Rock(0), Scissors(2) beats Paper(1)
            bool p1Wins = (c1 == ROCK && c2 == SCISSORS) ||
                          (c1 == PAPER && c2 == ROCK) ||
                          (c1 == SCISSORS && c2 == PAPER);

            Player winner = p1Wins ? p1 : p2;
            Debug.Log($"RPSManager: Player {winner.ActorNumber} wins RPS!");

            if (statusText != null)
                statusText.text = $"Player {winner.ActorNumber} wins! Loading game...";

            // Only MasterClient sets room property and loads scene
            if (PhotonNetwork.IsMasterClient)
            {
                Hashtable roomProps = new Hashtable { { RPS_WINNER_KEY, winner.ActorNumber } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                Invoke(nameof(LoadGameScene), 2f);
            }
        }
    }

    private void ResetAndRestart()
    {
        hasChosen = false;
        SetButtonsInteractable(true);

        Hashtable clearProps = new Hashtable { { RPS_CHOICE_KEY, -1 } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(clearProps);

        if (statusText != null) statusText.text = "Choose your move!";
    }

    private void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("RPSManager: Loading SampleScene...");
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (rockButton != null)     rockButton.interactable = interactable;
        if (paperButton != null)    paperButton.interactable = interactable;
        if (scissorsButton != null) scissorsButton.interactable = interactable;
    }
}
