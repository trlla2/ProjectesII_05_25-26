using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Header("Music Settings")]
    [SerializeField]
    private string musicParam = "MusicVol";
    [SerializeField]
    private Image musicBtnImage;
    private bool musicMuted;

    [Header("SFX Settings")]
    [SerializeField]
    private string sfxParam = "SFXVol";
    [SerializeField]
    private Image sfxBtnImage;
    private bool sfxMuted;

    void Start()
    {
        musicMuted = IsMutedInMixer(musicParam);
        sfxMuted = IsMutedInMixer(sfxParam);

        UpdateVisuals(musicBtnImage, musicMuted);
        UpdateVisuals(sfxBtnImage, sfxMuted);
    }

    public void ToggleMusic()
    {
        musicMuted = !musicMuted;
        audioMixer.SetFloat(musicParam, musicMuted ? -80f : 0f);
        UpdateVisuals(musicBtnImage, musicMuted);
    }

    public void ToggleSFX()
    {
        sfxMuted = !sfxMuted;
        audioMixer.SetFloat(sfxParam, sfxMuted ? -80f : 0f);
        UpdateVisuals(sfxBtnImage, sfxMuted);
    }

    bool IsMutedInMixer(string param)
    {
        float val;
        audioMixer.GetFloat(param, out val);
        return val <= -79f;
    }

    void UpdateVisuals(Image img, bool muted)
    {
        img.gameObject.SetActive(!muted);
    }
}
