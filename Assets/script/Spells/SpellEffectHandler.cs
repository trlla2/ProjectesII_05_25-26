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
    public ParticleSystem Particles;
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

    private TutorialManager tutorialManager;

    private const float FreezeDuration = 3f;
    private const int HealAmount = 5;

    public void Initialize(TutorialManager tutorial)
    {
        tutorialManager = tutorial;
    }

    public void HandleSpellEffects(Spell spell)
    {
        GameManager.Instance.NotifyPlayerMove();

        PlayEffectsForSpell(spell.Name);

        switch (spell.Name)
        {
            case "HEAL":
                GameManager.Instance.CombatHandler.HealPlayer(HealAmount);
                break;

            case "FREEZE":
                TryFreezeEnemy();
                break;

            case "THUNDER":
                StartCoroutine(TryThunderStrikeCoroutine());
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

        if (data.Particles != null)
            data.Particles.Play();
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
            Debug.LogWarning("THUNDER: Grid reference is missing on SpellEffectHandler.");
            yield break;
        }

        if (spellPathAnimator == null)
        {
            Debug.LogWarning("THUNDER: SpellPathAnimator reference is missing on SpellEffectHandler.");
            yield break;
        }

        // Wait for the spell cast's own refill to complete
        while (grid.CurrentState != GridStateType.Idle)
            yield return null;

        // Collect all non-blocked Yellow / Orange cells on the refreshed board
        List<Vector2Int> strikePositions = new List<Vector2Int>();
        List<Element> strikeElements = new List<Element>();

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
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
            Debug.Log("THUNDER: No Yellow or Orange elements found – no bonus hits.");
            yield break;
        }

        //Not an item but same concept
        grid.ChangeState(new ItemProcessingState(grid));

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
            });

            if (i < strikePositions.Count - 1)
                selectSequence.AppendInterval(thunderDelayBetweenElements);
        }

        selectSequence.AppendInterval(0.2f);

        yield return selectSequence.WaitForCompletion();

        foreach (GameObject marker in markers)
            Destroy(marker);

        // Visually clear struck cells and let SpellPathAnimator fire one hit per clone
        //each clone calls CombatHandler.AttackEnemy on landing
        NotifyBlockerEnemyOfUsedPositions(strikePositions);
        grid.InstantVisualEmpty(strikePositions);

        bool attackDone = false;
        spellPathAnimator.OnSpellCast(strikePositions.ToArray(), strikeElements, () =>
        {
            grid.DeleteElementsByPlayer(strikePositions);

            //we only count as player move if bomb enemy
            if (GameManager.Instance.CombatHandler.GetEnemyRef().ActiveSpell is BombSpell)
                GameManager.Instance.NotifyPlayerMove();

            grid.ChangeState(new ValidateSpellState(grid, GameManager.Instance.PlayerStats.GetSpellBook()));
            attackDone = true;
        });

        yield return new WaitUntil(() => attackDone);
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