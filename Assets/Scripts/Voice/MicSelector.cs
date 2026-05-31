using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Voice.Unity;

public class MicSelector : MonoBehaviour
{
    public static MicSelector Instance { get; private set; }

    private const string PrefsKey = "VoiceMicDevice";
    private const string DefaultLabel = "[Sistem Varsayılanı]";

    [SerializeField] private TMP_Dropdown dropdown;

    public string SelectedDeviceName { get; private set; }

    private readonly List<string> deviceNames = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SelectedDeviceName = PlayerPrefs.GetString(PrefsKey, string.Empty);
    }

    private void Start()
    {
        if (dropdown != null) RefreshDropdown();
    }

    public void RefreshDropdown()
    {
        if (dropdown == null) return;

        deviceNames.Clear();
        deviceNames.Add(string.Empty);
        foreach (var d in Microphone.devices) deviceNames.Add(d);

        var options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData(DefaultLabel));
        for (int i = 1; i < deviceNames.Count; i++) options.Add(new TMP_Dropdown.OptionData(deviceNames[i]));

        dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int idx = deviceNames.IndexOf(SelectedDeviceName);
        dropdown.SetValueWithoutNotify(idx >= 0 ? idx : 0);
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int idx)
    {
        if (idx < 0 || idx >= deviceNames.Count) return;
        SetDevice(deviceNames[idx]);
    }

    public void SetDevice(string deviceName)
    {
        SelectedDeviceName = deviceName ?? string.Empty;
        PlayerPrefs.SetString(PrefsKey, SelectedDeviceName);
        PlayerPrefs.Save();

        foreach (var binder in FindObjectsByType<PlayerMicBinder>(FindObjectsSortMode.None))
        {
            binder.ApplySelectedDevice();
        }
    }

    public Photon.Voice.DeviceInfo GetDeviceInfo()
    {
        if (string.IsNullOrEmpty(SelectedDeviceName)) return Photon.Voice.DeviceInfo.Default;
        return new Photon.Voice.DeviceInfo(SelectedDeviceName);
    }
}
