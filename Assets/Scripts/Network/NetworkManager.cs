using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("Connecting to Master...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master.");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");
    }

    public void HostGame()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby && PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            Debug.LogWarning("Still connecting to Photon servers... Current State: " + PhotonNetwork.NetworkClientState);
            return;
        }

        if (PhotonNetwork.InRoom) return;

        Debug.Log("Hosting a new room...");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public void JoinGame()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby && PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            Debug.LogWarning("Still connecting to Photon servers... Current State: " + PhotonNetwork.NetworkClientState);
            return;
        }

        if (PhotonNetwork.InRoom) return;

        Debug.Log("Joining an existing room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("JoinRandomRoom Failed: " + message + ". No active games found.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room. Loading SampleScene...");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message}");
    }
}