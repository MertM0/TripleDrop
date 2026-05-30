using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BasketballGameManager : MonoBehaviourPunCallbacks
{
    public static BasketballGameManager Instance;
    public Transform spawnPoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private bool hasSpawned = false;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.InRoom)
            {
                SpawnGameObjects();
            }
            else
            {
                PhotonNetwork.JoinRandomOrCreateRoom();
            }
        }
        else
        {
            Debug.Log("Not connected to Photon. Connecting automatically for testing...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        SpawnGameObjects();
    }

    private void SpawnGameObjects()
    {
        if (hasSpawned) return;
        hasSpawned = true;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0, 1, 0);
        PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Ball", spawnPos + Vector3.forward * 2f, Quaternion.identity);
        }
        
        Hashtable props = new Hashtable { { "Score", 0 } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            int score = (int)changedProps["Score"];
            Debug.Log($"Player {targetPlayer.ActorNumber} score is now: {score}");

            if (score >= 50)
            {
                Debug.Log($"Player {targetPlayer.ActorNumber} WINS!");
            }
        }
    }

    public void HandleScore(BallController ball)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (ball.lastThrowerActorNumber != -1)
        {
            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(ball.lastThrowerActorNumber);
            if (player != null)
            {
                int currentScore = player.CustomProperties.ContainsKey("Score") ? (int)player.CustomProperties["Score"] : 0;
                int pointsToAdd = ball.isPenaltyBall ? 1 : 2;
                currentScore += pointsToAdd;

                player.SetCustomProperties(new Hashtable { { "Score", currentScore } });
                Debug.Log($"Player {ball.lastThrowerActorNumber} Scored {pointsToAdd} points!");

                ball.photonView.RPC("RPC_SpawnFloatingText", RpcTarget.All, $"+{pointsToAdd}", 0f, 1f, 0f, ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
            }
            
            ball.photonView.RPC("RPC_ResetBallState", RpcTarget.All);
        }
    }

    public void HandleMiss(int actorNumber, BallController ball)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (player != null)
        {
            int currentScore = player.CustomProperties.ContainsKey("Score") ? (int)player.CustomProperties["Score"] : 0;
            currentScore -= 1;

            player.SetCustomProperties(new Hashtable { { "Score", currentScore } });
            Debug.Log($"Player {actorNumber} Missed! -1 point.");

            if (ball != null)
            {
                ball.photonView.RPC("RPC_SpawnFloatingText", RpcTarget.All, "-1", 1f, 0f, 0f, ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
            }
        }
    }





    [Header("UI & Visuals")]
    public GameObject floatingTextPrefab;
}