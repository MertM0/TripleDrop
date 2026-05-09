using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public UnityEngine.UI.Button hostButton;
    public UnityEngine.UI.Button joinButton;

    private bool isProcessing = false;

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(OnHostButtonClicked);
        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    public void OnHostButtonClicked()
    {
        if (isProcessing || NetworkManager.Instance == null) return;
        isProcessing = true;
        NetworkManager.Instance.HostGame();
        Invoke(nameof(ResetProcessing), 1f);
    }

    public void OnJoinButtonClicked()
    {
        if (isProcessing || NetworkManager.Instance == null) return;

        isProcessing = true;
        NetworkManager.Instance.JoinGame();
        Invoke(nameof(ResetProcessing), 1f);
    }

    private void ResetProcessing() => isProcessing = false;
}