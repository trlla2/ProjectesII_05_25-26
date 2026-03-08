using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private Slider mainSlider;

    private void Start()
    {
        SetMainVolume();
    }

    public void SetMainVolume()
    {
        audioMixer.SetFloat("MainVolume", Mathf.Log10(mainSlider.value) * 20);
    }
}
