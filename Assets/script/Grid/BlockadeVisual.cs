using UnityEngine;

public class BlockadeVisual : MonoBehaviour
{
    [Header("Blockade Sprites")]
    [SerializeField] private Sprite[] blockadeSprites;
    [SerializeField] private SpriteRenderer blockadeRenderer;

    private bool isActive;
    private int currentSpriteIndex;

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

        // Calculate sprite index based on progress form 0 to 1
        int spriteIndex = Mathf.FloorToInt(progress * (blockadeSprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, blockadeSprites.Length - 1);

        if (spriteIndex != currentSpriteIndex || blockadeRenderer.sprite == null)
        {
            currentSpriteIndex = spriteIndex;
            blockadeRenderer.sprite = blockadeSprites[spriteIndex];
        }
    }

    public void ResetVisual()
    {
        currentSpriteIndex = 0;
        if (blockadeSprites != null && blockadeSprites.Length > 0 && blockadeRenderer != null)
            blockadeRenderer.sprite = blockadeSprites[0];
    }
}