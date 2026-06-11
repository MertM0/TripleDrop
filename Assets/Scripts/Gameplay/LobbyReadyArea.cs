using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace TripleDrop.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class LobbyReadyArea : MonoBehaviourPunCallbacks
    {
        [Header("Settings")]
        public float countdownTime = 5f;

        [Header("Visual")]
        public Renderer areaRenderer;

        private static readonly Color IdleColor   = new Color(0.2f, 0.6f, 1f,  0.35f);
        private static readonly Color ActiveColor = new Color(1f,   0.75f, 0f,  0.50f);
        private static readonly Color ReadyColor  = new Color(0.1f, 1f,   0.2f, 0.65f);

        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock _propBlock;

        private Coroutine countdownCoroutine;
        private const string AREA_KEY = "InArea_";

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
                col.isTrigger = true;

            SetAreaColor(IdleColor);
            StartCoroutine(ResetPresenceOnLoad());
        }

        private IEnumerator ResetPresenceOnLoad()
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady);
            SetMyPresence(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.photonView.IsMine)
                SetMyPresence(true);
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.photonView.IsMine)
                SetMyPresence(false);
        }

        private void SetMyPresence(bool inArea)
        {
            if (!PhotonNetwork.InRoom) return;
            string key = AREA_KEY + PhotonNetwork.LocalPlayer.ActorNumber;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { key, inArea } });
        }

        public override void OnRoomPropertiesUpdate(Hashtable changedProps)
        {
            EvaluateArea();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            EvaluateArea();
        }

        private void EvaluateArea()
        {
            if (!PhotonNetwork.InRoom) return;
            if (IsPlayerReady()) return;

            if (AllPlayersInArea())
                BeginCountdown();
            else
                CancelCountdown();
        }

        private bool AllPlayersInArea()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return false;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                string key = AREA_KEY + p.ActorNumber;
                if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object val)
                    || !(bool)val)
                    return false;
            }
            return true;
        }

        private void BeginCountdown()
        {
            if (countdownCoroutine != null) return;
            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }

        private void CancelCountdown()
        {
            if (countdownCoroutine == null) return;
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
            SetAreaColor(IdleColor);
            Debug.Log("LobbyReadyArea: Birisi ayrıldı, sayım sıfırlandı.");
        }

        private IEnumerator CountdownRoutine()
        {
            float timer = countdownTime;
            Debug.Log($"LobbyReadyArea: İki oyuncu da alanda! {countdownTime}s sayım başlıyor.");

            while (timer > 0)
            {
                float progress = 1f - (timer / countdownTime);
                SetAreaColor(Color.Lerp(IdleColor, ActiveColor, progress));
                yield return null;
                timer -= Time.deltaTime;
            }

            SetAreaColor(ReadyColor);
            SetPlayerReady(true);
            countdownCoroutine = null;
            Debug.Log("LobbyReadyArea: Hazır!");
        }

        private void SetAreaColor(Color color)
        {
            if (areaRenderer == null) return;
            areaRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorID, color);
            areaRenderer.SetPropertyBlock(_propBlock);
        }

        private bool IsPlayerReady()
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out object v))
                return (bool)v;
            return false;
        }

        private void SetPlayerReady(bool ready)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "IsReady", ready } });
        }
    }
}
