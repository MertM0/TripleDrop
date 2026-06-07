using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private bool hasSpawned = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        // Photon pauses the network during scene loading, so IsConnectedAndReady
        // can be false even though InRoom becomes true shortly after. Wait for both.
        yield return new WaitUntil(() => PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady);
        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        if (hasSpawned) return;
        hasSpawned = true;

        // Reset per-round state when entering lobby (IsReady + the end-game return vote)
        Hashtable resetProps = new Hashtable { { "IsReady", false }, { "WantsLobby", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);

        // Pick spawn point based on actor number (1 or 2)
        int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints != null && spawnPoints.Length > spawnIndex && spawnPoints[spawnIndex] != null
            ? spawnPoints[spawnIndex].position
            : Vector3.zero;

        PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);
        Debug.Log($"LobbyManager: Spawned local player at {spawnPos}");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("IsReady")) return;

        // Check if ALL players in the room are ready
        bool allReady = true;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("IsReady", out object isReadyObj))
            {
                if (!(bool)isReadyObj)
                {
                    allReady = false;
                    break;
                }
            }
            else
            {
                // Property not set yet -> not ready
                allReady = false;
                break;
            }
        }

        // Also require at least 2 players in the room
        if (allReady && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            Debug.Log("LobbyManager: All players are ready! Starting RPS...");
            if (RPSManager.Instance != null)
            {
                RPSManager.Instance.StartRPS();
            }
            else
            {
                Debug.LogError("LobbyManager: RPSManager.Instance is null! Cannot start RPS.");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"LobbyManager: Player {newPlayer.NickName} joined the lobby.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"LobbyManager: Player {otherPlayer.NickName} left the lobby.");
    }
}
