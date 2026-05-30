using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallController : MonoBehaviourPunCallbacks, IPunObservable
{
    public int bounceCount = 0;
    public bool isHeld = false;
    
    public bool isPenaltyBall = false;
    public bool touchedHoop = false;
    public int lastThrowerActorNumber = -1;

    private Rigidbody rb;
    private SphereCollider col;
    private Transform currentHolder;
    private MeshRenderer meshRenderer;

    [Header("Materials")]
    public Material orangeMat;
    public Material yellowMat;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = isPenaltyBall ? yellowMat : orangeMat;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hoop"))
        {
            if (photonView.IsMine)
            {
                touchedHoop = true;
            }
            return;
        }

        if (!isHeld && !collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Hoop"))
        {
            if (photonView.IsMine)
            {
                if (!touchedHoop && lastThrowerActorNumber != -1)
                {
                    if (BasketballGameManager.Instance != null && PhotonNetwork.IsMasterClient)
                    {
                        BasketballGameManager.Instance.HandleMiss(lastThrowerActorNumber, this);
                    }
                    else if (BasketballGameManager.Instance != null && !PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("RPC_MasterHandleMiss", RpcTarget.MasterClient, lastThrowerActorNumber);
                    }
                    
                    lastThrowerActorNumber = -1;
                    bounceCount = 0;
                    isPenaltyBall = false;
                    UpdateVisuals();
                    
                    return;
                }

                bounceCount++;
                
                if (isPenaltyBall && bounceCount < 3)
                {
                    isPenaltyBall = false;
                    UpdateVisuals();
                }

                if (bounceCount >= 3 && !isPenaltyBall)
                {
                    isPenaltyBall = true;
                    UpdateVisuals();
                }
            }
        }
    }

    [PunRPC]
    public void RPC_MasterHandleMiss(int actorNumber)
    {
        if (PhotonNetwork.IsMasterClient && BasketballGameManager.Instance != null)
        {
            BasketballGameManager.Instance.HandleMiss(actorNumber, this);
        }
    }

    [PunRPC]
    public void RPC_SpawnFloatingText(string text, float r, float g, float b, float x, float y, float z)
    {
        if (BasketballGameManager.Instance != null && BasketballGameManager.Instance.floatingTextPrefab != null)
        {
            GameObject textObj = Instantiate(BasketballGameManager.Instance.floatingTextPrefab, new Vector3(x, y, z), Quaternion.identity);
            FloatingText ft = textObj.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Initialize(text, new Color(r, g, b));
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isHeld) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine && !player.hasBall)
        {
            photonView.RequestOwnership();
            photonView.RPC("RPC_PickupBall", RpcTarget.All, player.photonView.ViewID);
        }
    }

    public void TryPickup(PlayerController player)
    {
        if (isHeld) return;

        if (player != null && player.photonView.IsMine && !player.hasBall)
        {
            photonView.RequestOwnership();
            photonView.RPC("RPC_PickupBall", RpcTarget.All, player.photonView.ViewID);
        }
    }


    [PunRPC]
    public void RPC_PickupBall(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView != null)
        {
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (player != null)
            {
                isHeld = true;
                rb.isKinematic = true;
                col.enabled = false;

                isPenaltyBall = bounceCount >= 3;
                UpdateVisuals();

                bounceCount = 0;
                touchedHoop = false;
                lastThrowerActorNumber = -1;

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
    public void RPC_ThrowBall(Vector3 throwForce, int actorNumber)
    {
        isHeld = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;
        
        lastThrowerActorNumber = actorNumber;

        if (photonView.IsMine)
        {
            rb.AddForce(throwForce, ForceMode.Impulse);
        }

        if (currentHolder != null)
        {
            PlayerController pc = currentHolder.GetComponent<PlayerController>();
            if (pc != null) pc.hasBall = false;
            currentHolder = null;
        }
    }

    [PunRPC]
    public void RPC_ResetBallState()
    {
        touchedHoop = false;
        lastThrowerActorNumber = -1;
        bounceCount = 0;
        isPenaltyBall = false;
        UpdateVisuals();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(bounceCount);
            stream.SendNext(isPenaltyBall);
        }
        else
        {
            bounceCount = (int)stream.ReceiveNext();
            bool wasPenalty = isPenaltyBall;
            isPenaltyBall = (bool)stream.ReceiveNext();
            if (wasPenalty != isPenaltyBall)
            {
                UpdateVisuals();
            }
        }
    }
}