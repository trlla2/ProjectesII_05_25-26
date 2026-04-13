using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SlotMachineRow : MonoBehaviour
{
    public bool rowStopped;
    public string stoppedSlot;

    [SerializeField] private int rotateIterations;

    private static float[] SlotPositions = { 700f, 250f, -250f, -700f };
    private static string[] SlotNames    = { "STAR", "PLANT", "FIRE", "WATER" };

    private const float StepSize = 150f;
    private const float WrapMin = -700f;
    private const float WrapJump = 1850f;   // de -700 salta a 1150 (simula spin infinito)
    [SerializeField] private float StepDuration = 0.025f;

    void Start()
    {
        rowStopped = true;
    }

    public void StartRotatig()
    {
        rowStopped = false;
        stoppedSlot = "";
        StartCoroutine(SpinCoroutine());
    }

    private IEnumerator SpinCoroutine()
    {
        yield return StartCoroutine(StepPhase(rotateIterations, StepDuration));

        int extraSteps = Random.Range(60, 100);

        extraSteps = AlignStepsToSlot(extraSteps);

        float interval = StepDuration;
        for (int i = 0; i < extraSteps; i++)
        {
            // ajuste de velocidad progresivo (""curva"")
            float progress = (float)i / extraSteps;
            if (progress > 0.95f) interval = StepDuration / 0.2f;
            else if (progress > 0.75f) interval = StepDuration / 0.4f;
            else if (progress > 0.50f) interval = StepDuration / 0.6f;
            else if (progress > 0.25f) interval = StepDuration / 0.8f;
            else interval = StepDuration;

            yield return StartCoroutine(AnimateStep(interval));
        }

        float targetY = GetNearestSlotY(transform.localPosition.y);
        yield return transform.DOLocalMoveY(targetY, 0.25f)
                              .SetEase(Ease.OutCubic)
                              .WaitForCompletion();

        SetStoppedSlot(targetY);
        rowStopped = true;
    }

    private IEnumerator AnimateStep(float duration)
    {
        float startY = transform.localPosition.y;
        float endY   = startY - StepSize;

        // simular spin infinito
        if (endY <= WrapMin)
        {
            yield return transform.DOLocalMoveY(endY, duration)
                                  .SetEase(Ease.Linear)
                                  .WaitForCompletion();
            SetLocalY(endY + WrapJump);
        }
        else
        {
            yield return transform.DOLocalMoveY(endY, duration)
                                  .SetEase(Ease.Linear)
                                  .WaitForCompletion();
        }
    }

    private IEnumerator StepPhase(int steps, float interval)
    {
        for (int i = 0; i < steps; i++)
            yield return StartCoroutine(AnimateStep(interval));
    }

    private int AlignStepsToSlot(int requestedSteps)
    {
        float simulatedY = transform.localPosition.y;

        for (int i = 0; i < requestedSteps; i++)
        {
            simulatedY -= StepSize;
            if (simulatedY <= WrapMin)
                simulatedY += WrapJump;
        }

        float nearest = GetNearestSlotY(simulatedY);
        float distUntilNextSlotPos = simulatedY - nearest;
        int snapSteps = Mathf.RoundToInt(Mathf.Abs(distUntilNextSlotPos) / StepSize);

        return requestedSteps + snapSteps;
    }

    private float GetNearestSlotY(float currentY)
    {
        float bestY = SlotPositions[0];
        float bestDist = Mathf.Abs(currentY - bestY);

        for (int i = 1; i < SlotPositions.Length; i++)
        {
            float dist = Mathf.Abs(currentY - SlotPositions[i]);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestY = SlotPositions[i];
            }
        }
        return bestY;
    }

    private void SetStoppedSlot(float y)
    {
        for (int i = 0; i < SlotPositions.Length; i++)
        {
            if (Mathf.Approximately(y, SlotPositions[i]))
            {
                stoppedSlot = SlotNames[i];
                return;
            }
        }
        stoppedSlot = "";
    }

    private void SetLocalY(float y)
    {
        Vector3 p = transform.localPosition;
        p.y = y;
        transform.localPosition = p;
    }
}
