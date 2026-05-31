using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Recorder))]
public class PlayerMicBinder : MonoBehaviour
{
    private Recorder recorder;
    private PhotonView photonView;

    private void Awake()
    {
        recorder = GetComponent<Recorder>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;
        ApplySelectedDevice();
    }

    public void ApplySelectedDevice()
    {
        if (recorder == null || !photonView.IsMine) return;
        if (MicSelector.Instance == null) return;

        var dev = MicSelector.Instance.GetDeviceInfo();
        if (recorder.MicrophoneDevice == dev) return;

        recorder.MicrophoneDevice = dev;
        if (recorder.RecordingEnabled) recorder.RestartRecording();
    }
}
