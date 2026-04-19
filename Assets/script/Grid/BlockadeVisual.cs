using UnityEngine;
using DG.Tweening;

public class BlockadeVisual : MonoBehaviour
{
    [Header("Blockade Sprites")]
    [SerializeField] private Sprite[] blockadeSprites;
    [SerializeField] private SpriteRenderer blockadeRenderer;

    [Header("Prgress Effects")]
    [SerializeField] private ParticleSystem progressParticles;
    [SerializeField] private AudioClip progresSoundClip;

    [Header("Break Effects")]
    [SerializeField] private ParticleSystem breakParticles;
    [SerializeField] private AudioClip breakSoundClip;



    [SerializeField] private AudioSource audioSource;

    private bool isActive;
    private int currentSpriteIndex;
    private float currentProgress;

    public bool IsActive => isActive;

    private void Awake()
    {
        if (blockadeRenderer == null)
            blockadeRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (blockadeRenderer != null)
            blockadeRenderer.enabled = active;

        if (active)
            UpdateProgress(0f);
    }

    public void UpdateProgress(float progress)
    {
        if (!isActive || blockadeSprites == null || blockadeSprites.Length == 0)
            return;

        progress = Mathf.Clamp01(progress);

        int spriteIndex;
        if (progress >= 1f)
            spriteIndex = blockadeSprites.Length - 1;
        else if (progress <= 0f)
            spriteIndex = 0;
        else
            spriteIndex = Mathf.Clamp(
                Mathf.CeilToInt(progress * (blockadeSprites.Length - 2)),
                1, blockadeSprites.Length - 2);

        if (spriteIndex != currentSpriteIndex || blockadeRenderer.sprite == null)
        {
            bool isFullyBroken = spriteIndex == blockadeSprites.Length - 1 && currentProgress > 0f;

            if (isFullyBroken)
                PlayBreakEffect();
            else
                PlayProgressEffect();

            currentSpriteIndex = spriteIndex;
            blockadeRenderer.sprite = blockadeSprites[spriteIndex];
        }
        if (blockadeRenderer != null && currentProgress != progress)
        {
            blockadeRenderer.transform.DOShakePosition(0.3f + progress * 2, new Vector3(0.1f, 0.1f, 0), (int)(10 * progress * 2), 90);
            blockadeRenderer.transform.DOShakeRotation(0.3f + progress * 2, new Vector3(0, 0, 5), (int)(10 * progress * 2), 90);
        }

        currentProgress = progress;
    }

    private void PlayProgressEffect()
    {     
        if (progressParticles != null)
        {
            ParticleSystem particles;
            particles = Instantiate(progressParticles, transform.position, Quaternion.identity);
            particles.transform.position = transform.position;
            particles.Play();
            Destroy(particles.gameObject, progressParticles.main.duration);
        }
        if (progresSoundClip != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(progresSoundClip);
        }
    }

    private void PlayBreakEffect()
    {
        if (breakParticles != null)
        {
            ParticleSystem particles;
            particles = Instantiate(breakParticles, transform.position, Quaternion.identity);
            particles.transform.position = transform.position;
            particles.Play();
            Destroy(particles.gameObject, breakParticles.main.duration);
        }

        if (breakSoundClip != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(breakSoundClip);
        }
    }
}