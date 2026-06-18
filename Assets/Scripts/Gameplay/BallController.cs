using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallController : MonoBehaviourPunCallbacks, IPunObservable
{
    public int bounceCount = 0;
    public bool isHeld = false;

    public bool isPenaltyBall = false;
    public bool touchedHoop = false;
    public bool enteredHoopFromBelow = false;
    public int lastThrowerActorNumber = -1;
    public bool isFireBall = false;
    public bool thrownAsPenalty = false;
    public int allowedPickupActor = -1;

    private Rigidbody rb;
    private SphereCollider col;
    private Transform currentHolder;
    private MeshRenderer meshRenderer;
    private float baseMass = 1f;
    private MaterialPropertyBlock ballPropBlock;

    [Header("Materials")]
    public Material orangeMat;
    public Material yellowMat;

    [Header("Audio Settings")]
    public AudioClip bounceClip;
    public AudioClip pickupClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("Score Sound Settings")]
    public AudioClip scoreMinusOneClip;
    public AudioClip scorePlusOneClip;
    public AudioClip scorePlusTwoClip;
    public AudioClip scorePlusThreeClip;

    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 30f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
        if (rb != null)
        {
            baseMass = rb.mass;
        }
        UpdateVisuals();
    }

    private void Update()
    {
        if (isHeld && currentHolder != null)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    private void UpdateVisuals()
    {
        if (meshRenderer == null) return;
        if (ballPropBlock == null) ballPropBlock = new MaterialPropertyBlock();

        Material targetMat;
        Color tint;

        if (isFireBall)
        {
            targetMat = orangeMat;
            tint = new Color(1f, 0.3f, 0f);
        }
        else
        {
            targetMat = isPenaltyBall ? yellowMat : orangeMat;
            tint = Color.white;
        }

        if (targetMat != null)
            meshRenderer.sharedMaterial = targetMat;

        meshRenderer.GetPropertyBlock(ballPropBlock);
        meshRenderer.SetPropertyBlock(ballPropBlock);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            float force = collision.relativeVelocity.magnitude;
            if (force > 0.5f)
            {
                photonView.RPC(nameof(RPC_PlayBounceSound), RpcTarget.All, force);
            }
        }

        if (collision.gameObject.CompareTag("Hoop"))
        {
            if (photonView.IsMine)
            {
                touchedHoop = true;
                if (isPenaltyBall)
                {
                    isPenaltyBall = false;
                    bounceCount = 0;
                    UpdateVisuals();
                }
            }
            return;
        }

        enteredHoopFromBelow = false;

        if (!isHeld && !collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Hoop"))
        {
            if (photonView.IsMine)
            {
                if (lastThrowerActorNumber != -1 && allowedPickupActor == 0)
                {
                    photonView.RPC(nameof(RPC_HandleTurnover), RpcTarget.All, lastThrowerActorNumber);
                }

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
                    bounceCount = 0;
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
    public void RPC_PlayBounceSound(float force)
    {
        if (audioSource != null && bounceClip != null)
        {
            float volume = Mathf.Clamp(force / 10f, 0.15f, 1f);
            SoundManager.Instance.PlaySFXOnSource(audioSource, bounceClip, volume, minPitch, maxPitch);
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
    public void RPC_SpawnFloatingText(string text, float r, float g, float b, float x, float y, float z, int points)
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

        AudioClip targetClip = null;
        switch (points)
        {
            case -1:
                targetClip = scoreMinusOneClip;
                break;
            case 1:
                targetClip = scorePlusOneClip;
                break;
            case 2:
                targetClip = scorePlusTwoClip;
                break;
            case 3:
                targetClip = scorePlusThreeClip;
                break;
        }

        if (targetClip != null)
        {
            SoundManager.Instance.PlaySFXAtPoint(targetClip, new Vector3(x, y, z), 1f, minPitch, maxPitch);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHeld) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine && !player.hasBall && CanBePickedUpBy(player))
        {
            photonView.RequestOwnership();
            photonView.RPC("RPC_PickupBall", RpcTarget.All, player.photonView.ViewID);
        }
    }

    public void TryPickup(PlayerController player)
    {
        if (isHeld) return;

        if (player != null && player.photonView.IsMine && !player.hasBall && CanBePickedUpBy(player))
        {
            photonView.RequestOwnership();
            photonView.RPC("RPC_PickupBall", RpcTarget.All, player.photonView.ViewID);
        }
    }

    private bool CanBePickedUpBy(PlayerController player)
    {
        if (allowedPickupActor == -1) return true;
        return player.photonView.Owner != null && player.photonView.Owner.ActorNumber == allowedPickupActor;
    }

    [PunRPC]
    public void RPC_HandleTurnover(int thrower)
    {
        if (lastThrowerActorNumber != thrower) return;
        allowedPickupActor = FindOpponentActor(thrower);
    }

    [PunRPC]
    public void RPC_SetAllowedPickup(int actor)
    {
        allowedPickupActor = actor;
    }

    private static int FindOpponentActor(int thrower)
    {
        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber != thrower) return p.ActorNumber;
        }
        return -1;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (allowedPickupActor == otherPlayer.ActorNumber)
        {
            allowedPickupActor = -1;
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
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                col.enabled = false;

                isPenaltyBall = bounceCount >= 3;
                isFireBall = false;
                if (rb != null)
                {
                    rb.mass = baseMass;
                }
                UpdateVisuals();

                bounceCount = 0;
                thrownAsPenalty = false;
                touchedHoop = false;
                enteredHoopFromBelow = false;
                lastThrowerActorNumber = -1;
                allowedPickupActor = playerView.Owner != null ? playerView.Owner.ActorNumber : -1;

                Transform holdPoint = player.cameraTransform.Find("HoldPoint");
                if (holdPoint == null) holdPoint = player.cameraTransform;

                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                player.hasBall = true;
                player.heldBall = this;
                currentHolder = player.transform;

                if (audioSource != null && pickupClip != null)
                {
                    SoundManager.Instance.PlaySFXOnSource(audioSource, pickupClip, 1f, minPitch, maxPitch);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_ThrowBall(Vector3 throwForce, int actorNumber, float massMultiplier, bool isFire)
    {
        isHeld = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;

        lastThrowerActorNumber = actorNumber;
        isFireBall = isFire;
        thrownAsPenalty = isPenaltyBall;
        allowedPickupActor = 0;

        if (rb != null)
        {
            rb.mass = baseMass * massMultiplier;
        }

        UpdateVisuals();

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
        enteredHoopFromBelow = false;
        lastThrowerActorNumber = -1;
        bounceCount = 0;
        isPenaltyBall = false;
        isFireBall = false;
        thrownAsPenalty = false;
        if (rb != null)
        {
            rb.mass = baseMass;
        }
        UpdateVisuals();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(bounceCount);
            stream.SendNext(isPenaltyBall);
            stream.SendNext(isFireBall);
            stream.SendNext(allowedPickupActor);
        }
        else
        {
            bounceCount = (int)stream.ReceiveNext();
            bool wasPenalty = isPenaltyBall;
            isPenaltyBall = (bool)stream.ReceiveNext();
            bool wasFire = isFireBall;
            isFireBall = (bool)stream.ReceiveNext();
            allowedPickupActor = (int)stream.ReceiveNext();
            if (wasPenalty != isPenaltyBall || wasFire != isFireBall)
            {
                UpdateVisuals();
            }
        }
    }
}
