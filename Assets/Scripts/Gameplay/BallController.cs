using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallController : MonoBehaviourPunCallbacks, IPunObservable
{
    public int bounceCount = 0;
    public bool isHeld = false;

    private Rigidbody rb;
    private SphereCollider col;
    private Transform currentHolder;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        if (!photonView.IsMine && !isHeld)
        {
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isHeld && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (photonView.IsMine)
            {
                bounceCount++;
            }
        }

        if (isHeld) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        TryPickup(player);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHeld) return;

        PlayerController player = other.GetComponent<PlayerController>();
        TryPickup(player);
    }

    private float lastPickupRequestTime = 0f;

    public void TryPickup(PlayerController player)
    {
        if (player != null && player.photonView.IsMine && !player.hasBall && !isHeld)
        {
            if (Time.time - lastPickupRequestTime < 0.5f) return;
            lastPickupRequestTime = Time.time;

            photonView.RequestOwnership();
            photonView.RPC("RPC_PickupBall", RpcTarget.All, player.photonView.ViewID);
        }
    }

    [PunRPC]
    public void RPC_PickupBall(int playerViewID)
    {
        if (isHeld) return;

        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView != null)
        {
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (player != null)
            {
                isHeld = true;
                rb.isKinematic = true;
                col.enabled = false;
                bounceCount = 0;

                PhotonTransformView ptv = GetComponent<PhotonTransformView>();
                if (ptv != null) ptv.enabled = false;

                Transform holdPoint = player.cameraTransform.Find("HoldPoint");
                if (holdPoint == null) holdPoint = player.cameraTransform;

                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                player.hasBall = true;
                player.heldBall = this;
                currentHolder = player.transform;
            }
        }
    }

    [PunRPC]
    public void RPC_ThrowBall(Vector3 throwForce)
    {
        isHeld = false;
        transform.SetParent(null);
        col.enabled = true;

        PhotonTransformView ptv = GetComponent<PhotonTransformView>();
        if (ptv != null) ptv.enabled = true;

        if (photonView.IsMine)
        {
            rb.isKinematic = false;
            rb.AddForce(throwForce, ForceMode.Impulse);
        }
        else
        {
            rb.isKinematic = true;
        }

        if (currentHolder != null)
        {
            PlayerController pc = currentHolder.GetComponent<PlayerController>();
            if (pc != null) pc.hasBall = false;
            currentHolder = null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(bounceCount);
        }
        else
        {
            int receivedBounces = (int)stream.ReceiveNext();
            if (!isHeld)
            {
                bounceCount = receivedBounces;
            }
        }
    }
}