using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public struct SpellEffectActivationData
{
    public string SpellName;
    public AudioClip Sound;
    public ParticleSystem[] VFXPrefab;
}

public class SpellEffectHandler : MonoBehaviour
{
    [Header("Spell Effects")]
    [SerializeField] private List<SpellEffectActivationData> spellEffects = new List<SpellEffectActivationData>();

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private SpellPathAnimator spellPathAnimator;

    [Header("Thunder Settings")]
    [SerializeField] private Sprite thunderMarkerSprite;
    [SerializeField] private float thunderDelayBetweenElements = 0.15f;
    [SerializeField] private LigthningPanel panel;

    [Header("Freeze Settings")]
    [SerializeField] private FreezeVignette feezeVignette;

    private TutorialManager tutorialManager;

    private const float FreezeDuration = 3f;
    private const int HealAmount = 5;

    public void Initialize(TutorialManager tutorial)
    {
        tutorialManager = tutorial;
    }

    public void HandleSpellEffects(Spell spell)
    {
        PlayEffectsForSpell(spell.Name);

        switch (spell.Name)
        {
            case "HEAL":
                GameManager.Instance.NotifyPlayerMove();
                GameManager.Instance.CombatHandler.HealPlayer(HealAmount);
                break;

            case "FREEZE":
                GameManager.Instance.NotifyPlayerMove();
                TryFreezeEnemy();
                break;

            case "THUNDER":
                StartCoroutine(TryThunderStrikeCoroutine());
                break;

            default:
                GameManager.Instance.NotifyPlayerMove();
                break;
        }

        if (tutorialManager != null)
            tutorialManager.NotifySpellCast();
    }

    public void PlayEffectsForSpell(string spellName)
    {
        SpellEffectActivationData data = spellEffects.Find(e => e.SpellName == spellName);
        if (data.SpellName == null) return;

        if (audioSource != null && data.Sound != null)
            audioSource.PlayOneShot(data.Sound);

        if (data.VFXPrefab != null)
        {
            foreach (ParticleSystem ps in data.VFXPrefab)
            {
                ps.Play();
            }
        }
        if(data.SpellName == "THUNDER" && panel != null)
        {
            panel.PlayAnimation();
        }
        if(data.SpellName == "FREEZE" && feezeVignette != null)
        {
            feezeVignette.PerformFreezePaneleffect(FreezeDuration);
        }
    }

    private void TryFreezeEnemy(float duration = FreezeDuration)
    {
        EnemyBehaviour enemy = GameManager.Instance?.CombatHandler?.GetEnemyRef();
        if (enemy == null) return;

        IEnemySpell activeSpell = enemy.ActiveSpell;
        bool isTimedEnemy = activeSpell is TimedDestroySpellBase || activeSpell is MultipleTimedSpellBase || activeSpell is BlockerSpell || activeSpell is SwapperSpell;

        if (isTimedEnemy)
            enemy.FreezeForDuration(FreezeDuration);
        else
            Debug.Log("FREEZE: Enemy has no active timed spell – no effect.");
    }

    private IEnumerator TryThunderStrikeCoroutine()
    {
        if (grid == null)
        {
            // guard: still notify so the enemy doesn't stall
            GameManager.Instance.NotifyPlayerMove();
            yield break;
        }

        if (spellPathAnimator == null)
        {
            GameManager.Instance.NotifyPlayerMove();
            yield break;
        }

        // Wait for the spell cast's own refill to complete
        while (grid.CurrentState != GridStateType.Idle)
            yield return null;

        // Collect all non-blocked Yellow elements
        List<Vector2Int> strikePositions = new List<Vector2Int>();
        List<Element> strikeElements = new List<Element>();

        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);

                if (element == null || visual == null || visual.IsBlocked)
                    continue;

                if (element.Type == ElementType.Yellow || element.Type == ElementType.Orange)
                {
                    strikePositions.Add(pos);
                    strikeElements.Add(element);
                }
            }
        }

        if (strikePositions.Count == 0)
        {
            // No valid targets — still notify the enemy so it can take its turn
            GameManager.Instance.NotifyPlayerMove();
            yield break;
        }
        print($"Thunder Strike: Found {strikePositions.Count} targets.");
        grid.ChangeState(new ItemProcessingState(grid));

        print("Thunder Strike: Starting selection animation.");
        //Selection animation
        List<GameObject> markers = new List<GameObject>();
        Sequence selectSequence = DOTween.Sequence();

        for (int i = 0; i < strikePositions.Count; i++)
        {
            Vector2Int capturedPos = strikePositions[i];
            Vector3 worldPos = grid.VisualGrid.GridToWorldPosition(capturedPos);

            selectSequence.AppendCallback(() =>
            {
                markers.Add(SpawnThunderMarker(worldPos));
                ElementVisual visual = grid.GridData.GetVisualAt(capturedPos);
                visual?.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 5, 0.5f);
                SFXManager.Instance.PlaySFXByName("ActivateObliderateElement");
            });

            if (i < strikePositions.Count - 1)
                selectSequence.AppendInterval(thunderDelayBetweenElements);
        }

        selectSequence.AppendInterval(0.2f);

        yield return selectSequence.WaitForCompletion();

        foreach (GameObject marker in markers)
            Destroy(marker);

        NotifyBlockerEnemyOfUsedPositions(strikePositions);
        grid.InstantVisualEmpty(strikePositions);

        bool attackDone = false;
        spellPathAnimator.OnSpellCast(strikePositions.ToArray(), strikeElements, () =>
        {
            grid.DeleteElementsByPlayer(strikePositions);
            grid.ChangeState(new RefillState(grid, grid.VisualGrid));
            attackDone = true;
        });

        yield return new WaitUntil(() => attackDone);

        GameManager.Instance.NotifyPlayerMove();
    }

    private GameObject SpawnThunderMarker(Vector3 worldPos)
    {
        GameObject markerObj = new GameObject("Thunder_Marker");
        SpriteRenderer sr = markerObj.AddComponent<SpriteRenderer>();
        sr.sprite = thunderMarkerSprite;
        sr.color = new Color(1f, 0.9f, 0.1f);
        sr.sortingOrder = 101;
        markerObj.transform.position = worldPos;
        markerObj.transform.localScale = Vector3.zero;
        markerObj.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
        return markerObj;
    }

    private void NotifyBlockerEnemyOfUsedPositions(List<Vector2Int> usedPositions)
    {
        if (GameManager.Instance == null || GameManager.Instance.CombatHandler == null)
            return;

        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();
        enemy?.NotifyAdjacentElementsUsed(usedPositions);
    }
}