using UnityEngine;
using Photon.Pun;

public class BasketballGameManager : MonoBehaviourPunCallbacks
{
    public Transform spawnPoint;

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            SpawnPlayer();
        }
        else
        {
            Debug.LogWarning("Started in SampleScene without being in a room. Waiting to join a room to spawn player.");
        }
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0, 1, 0);
        Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        PhotonNetwork.Instantiate("Player", spawnPos + randomOffset, Quaternion.identity);

        if (PhotonNetwork.IsMasterClient)
        {
            if (GameObject.FindFirstObjectByType<BallController>() == null)
            {
                PhotonNetwork.Instantiate("Ball", spawnPos + Vector3.forward * 2f, Quaternion.identity);
            }
        }
    }
}