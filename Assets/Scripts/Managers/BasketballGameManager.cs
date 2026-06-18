using System.Collections;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BasketballGameManager : MonoBehaviourPunCallbacks
{
    public static BasketballGameManager Instance;
    public Transform spawnPoint;

    [Header("UI & Visuals")]
    public GameObject floatingTextPrefab;

    [Header("Score VFX Prefabs")]
    public GameObject vfx1PointPrefab;
    public GameObject vfx2PointPrefab;
    public GameObject vfx3PointPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private bool hasSpawned = false;

#if UNITY_EDITOR
    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.endKey.wasPressedThisFrame && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Score", 50 } });
            Debug.Log("[CHEAT] Win triggered.");
        }
    }
#endif

    private void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady);
        SpawnGameObjects();
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
            GameObject ballObj = PhotonNetwork.Instantiate("Ball", spawnPos + Vector3.forward * 2f, Quaternion.identity);
            StartCoroutine(GiveBallToRPSWinner(ballObj.GetComponent<BallController>()));
        }

        Hashtable props = new Hashtable { { "Score", 0 } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private IEnumerator GiveBallToRPSWinner(BallController ball)
    {
        if (ball == null) yield break;

        object winnerActorObj;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("RPSWinner", out winnerActorObj))
        {
            Debug.LogWarning("BasketballGameManager: No RPSWinner found in room properties. Ball not assigned.");
            yield break;
        }

        int winnerActorNumber = (int)winnerActorObj;
        if (winnerActorNumber <= 0) yield break;
        
        PlayerController winnerPc = null;
        float timeout = 5f;
        while (timeout > 0f && winnerPc == null)
        {
            foreach (PlayerController pc in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            {
                if (pc.photonView.Owner != null && pc.photonView.Owner.ActorNumber == winnerActorNumber)
                {
                    winnerPc = pc;
                    break;
                }
            }
            if (winnerPc != null) break;
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }

        if (ball == null) yield break;

        if (winnerPc != null)
        {
            int winnerViewID = winnerPc.photonView.ViewID;
            ball.photonView.RPC("RPC_PickupBall", RpcTarget.All, winnerViewID);
            Debug.Log($"BasketballGameManager: Ball given to RPS winner (Actor {winnerActorNumber}, ViewID {winnerViewID})");
        }
        else
        {
            Debug.LogWarning($"BasketballGameManager: Could not find PlayerController for RPS winner actor {winnerActorNumber} within timeout.");
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            int score = (int)changedProps["Score"];
            Debug.Log($"Player {targetPlayer.ActorNumber} score is now: {score}");

            if (score >= 50)
            {
                Debug.Log($"Player {targetPlayer.ActorNumber} WINS with {score} points!");
                
                if (EndGameUIManager.Instance != null)
                {
                    bool localPlayerWon = targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
                    EndGameUIManager.Instance.ShowEndGame(localPlayerWon);
                }
                else
                {
                    Debug.LogWarning("BasketballGameManager: EndGameUIManager.Instance is null!");
                }
            }
        }
    }

    public void HandleScore(BallController ball, Vector3 vfxPos)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int thrower = ball.lastThrowerActorNumber;
        if (thrower != -1)
        {
            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(thrower);
            if (player != null)
            {
                int currentScore = player.CustomProperties.ContainsKey("Score") ? (int)player.CustomProperties["Score"] : 0;
                int pointsToAdd = ball.isFireBall ? 3 : (ball.isPenaltyBall ? 1 : 2);
                currentScore += pointsToAdd;

                player.SetCustomProperties(new Hashtable { { "Score", currentScore } });
                Debug.Log($"Player {thrower} Scored {pointsToAdd} points!");

                string scoreText = "+" + pointsToAdd;
                Color textColor = Color.white;
                if (ball.isFireBall)
                {
                    scoreText = "+3 FIRE!";
                    textColor = new Color(1f, 0.3f, 0f);
                }
                else if (pointsToAdd == 2)
                {
                    textColor = Color.green;
                }
                else if (pointsToAdd == 1)
                {
                    textColor = Color.yellow;
                }

                ball.photonView.RPC("RPC_SpawnFloatingText", RpcTarget.All, scoreText, textColor.r, textColor.g, textColor.b, ball.transform.position.x, ball.transform.position.y, ball.transform.position.z, pointsToAdd);
                photonView.RPC("RPC_SpawnScoreVFX", RpcTarget.All, pointsToAdd, vfxPos.x, vfxPos.y, vfxPos.z);
            }
            
            ball.photonView.RPC("RPC_ResetBallState", RpcTarget.All);
            ball.photonView.RPC("RPC_SetAllowedPickup", RpcTarget.All, thrower);
        }
    }

    [PunRPC]
    public void RPC_SpawnScoreVFX(int points, float x, float y, float z)
    {
        GameObject prefabToSpawn = null;
        if (points == 1) prefabToSpawn = vfx1PointPrefab;
        else if (points == 2) prefabToSpawn = vfx2PointPrefab;
        else if (points >= 3) prefabToSpawn = vfx3PointPrefab;

        if (prefabToSpawn != null)
        {
            GameObject vfxObj = Instantiate(prefabToSpawn, new Vector3(x, y, z), Quaternion.identity);
            
            ScoreArea scoreArea = Object.FindAnyObjectByType<ScoreArea>();
            if (scoreArea != null)
            {
                if (scoreArea.vfxSpawnPoint != null)
                    vfxObj.transform.SetParent(scoreArea.vfxSpawnPoint, true);
                else
                    vfxObj.transform.SetParent(scoreArea.transform, true);
            }

            Destroy(vfxObj, 2f);
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
                ball.photonView.RPC("RPC_SpawnFloatingText", RpcTarget.All, "-1", 1f, 0f, 0f, ball.transform.position.x, ball.transform.position.y, ball.transform.position.z, -1);
            }
        }
    }
}