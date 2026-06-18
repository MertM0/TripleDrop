using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;

    [Header("Settings Panel")]
    public Button settingsButton;
    public GameObject settingsPanel;
    public Button backButton;
    public Slider sfxSlider;
    public Slider musicSlider;

    private bool isProcessing = false;

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(OnHostButtonClicked);
        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClicked);

        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (backButton != null) backButton.onClick.AddListener(CloseSettings);

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = SoundManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
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

    private void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            if (sfxSlider != null) sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            if (musicSlider != null) musicSlider.value = SoundManager.Instance.GetMusicVolume();
        }
    }

    private void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }
}