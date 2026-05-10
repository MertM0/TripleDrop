using UnityEngine;
using Photon.Pun;

namespace TripleDrop.PowerUps
{
    public enum PowerUpType
    {
        SpeedUP,
        JumpUP,
        PowerUP,
        LowGravity,
        HighGravity,
        HeavyBall,
        LightBall,
        FireBall,
        PlatformPanic
    }

    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Collider))]
    public class PowerUp : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        private PowerUpType type;
        private bool isCollected = false;

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;
            if (data != null && data.Length > 0)
            {
                type = (PowerUpType)data[0];
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            PhotonView otherView = other.GetComponent<PhotonView>();
            if (otherView != null && otherView.IsMine && other.CompareTag("Player"))
            {
                PlayerPowerUpController controller = other.GetComponent<PlayerPowerUpController>();
                if (controller != null)
                {
                    controller.ApplyPowerUp(type);
                }

                photonView.RPC(nameof(RPC_CollectPowerUp), RpcTarget.All);
            }
        }

        [PunRPC]
        private void RPC_CollectPowerUp()
        {
            isCollected = true;
            gameObject.SetActive(false);
            
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}