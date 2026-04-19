using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour 
{
    [Header("precharged SFX")]
    [SerializeField] private List<AudioClip> prechargedSFX;

    private AudioSource sfxAS;


    public static SFXManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        sfxAS = GetComponent<AudioSource>();
    }


    public void PlaySFXByName(string name)
    {
        AudioClip sfx = prechargedSFX.Find(clip => clip.name == name);
        if (sfx == null)
        {
            Debug.LogError($"SFX MANAGER: THere is no sfx called \"{name}\"");
            return;
        }

        sfxAS.PlayOneShot(sfx);
    }

}
