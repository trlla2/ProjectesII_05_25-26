using UnityEngine;
using DG.Tweening;
public class BlockadeVisual : MonoBehaviour
{
    [Header("Blockade Sprites")]
    [SerializeField] private Sprite[] blockadeSprites;
    [SerializeField] private SpriteRenderer blockadeRenderer;

    private bool isActive;
    private int currentSpriteIndex;
    private float currentProgress;

    public bool IsActive => isActive;

    private void Awake()
    {
        if (blockadeRenderer == null)
            blockadeRenderer = GetComponent<SpriteRenderer>();

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

        //Use DoTween to perform a shake animation to the sr
        if (blockadeRenderer != null && currentProgress != progress)
        {
            blockadeRenderer.transform.DOShakePosition(0.2f + progress, new Vector3(0.1f, 0.1f, 0), (int)(10 * progress * 2), 90);
        }

        // progress > 0 immediately advances to the first breaking sprite.
        int spriteIndex = Mathf.CeilToInt(progress * (blockadeSprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, blockadeSprites.Length - 1);

        if (spriteIndex != currentSpriteIndex || blockadeRenderer.sprite == null)
        {
            currentSpriteIndex = spriteIndex;
            blockadeRenderer.sprite = blockadeSprites[spriteIndex];
        }
        currentProgress = progress;
    }
}