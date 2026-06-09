using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerUI : MonoBehaviour
{
    public PlayerController targetPlayer;
    public GameObject uiContainer;
    public Image powerBarFill;
    public GameObject powerBarBg;
    public TMPro.TextMeshProUGUI hostScoreText;
    public TMPro.TextMeshProUGUI clientScoreText;

    private int lastHostScore = int.MinValue;
    private int lastClientScore = int.MinValue;

    private void Start()
    {
        if (uiContainer == null) uiContainer = gameObject;
        if (powerBarFill == null)
        {
            Transform bg = transform.Find("PowerBarBg");
            if (bg != null)
            {
                if (powerBarBg == null) powerBarBg = bg.gameObject;
                Transform fill = bg.Find("PowerBarFill");
                if (fill != null) powerBarFill = fill.GetComponent<UnityEngine.UI.Image>();
            }
        }
        if (hostScoreText == null)
        {
            Transform hostT = transform.Find("HostScoreText");
            if (hostT != null) hostScoreText = hostT.GetComponent<TMPro.TextMeshProUGUI>();
        }
        if (clientScoreText == null)
        {
            Transform clientT = transform.Find("ClientScoreText");
            if (clientT != null) clientScoreText = clientT.GetComponent<TMPro.TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (targetPlayer == null)
        {
            targetPlayer = PlayerController.Local;
            if (targetPlayer == null) return;
        }

        if (!uiContainer.activeSelf) uiContainer.SetActive(true);

        if (powerBarFill != null)
        {
            powerBarFill.fillAmount = targetPlayer.chargePower / targetPlayer.maxChargePower;
            if (powerBarFill.gameObject.activeSelf != targetPlayer.isCharging)
            {
                powerBarFill.gameObject.SetActive(targetPlayer.isCharging);
            }
        }

        if (powerBarBg != null)
        {
            if (powerBarBg.activeSelf != targetPlayer.isCharging)
            {
                powerBarBg.SetActive(targetPlayer.isCharging);
            }
        }

        UpdateScores();
    }

    private void UpdateScores()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        Photon.Realtime.Player host = PhotonNetwork.CurrentRoom.GetPlayer(1);
        Photon.Realtime.Player client = PhotonNetwork.CurrentRoom.GetPlayer(2);

        // Only touch TextMeshPro when the value actually changes — avoids 2 string
        // allocations every frame (and the associated TMP work) while idle.
        if (hostScoreText != null && host != null)
        {
            int hostScore = host.CustomProperties.ContainsKey("Score") ? (int)host.CustomProperties["Score"] : 0;
            if (hostScore != lastHostScore)
            {
                hostScoreText.text = $"<color=#0000FF>{hostScore}</color>";
                lastHostScore = hostScore;
            }
        }

        if (clientScoreText != null && client != null)
        {
            int clientScore = client.CustomProperties.ContainsKey("Score") ? (int)client.CustomProperties["Score"] : 0;
            if (clientScore != lastClientScore)
            {
                clientScoreText.text = $"<color=#FF0000>{clientScore}</color>";
                lastClientScore = clientScore;
            }
        }
    }
}
