using System;
using System.Collections;
using UnityEngine;

public class TrailSuccessAnimationPhase : IQTEPhase
{
    public event Action<bool> OnPhaseComplete;

    private QTEVisualController visualController;
    private QTETrailRenderer trailRenderer;
    private Vector3 orbPosition;
    private MonoBehaviour coroutineRunner;
    
    private float accelerationDuration = 1.5f;
    private float maxSpeedDuration = 1f;
    private float moveToOrbDuration = 0.5f;
    private float finalScale = 0.1f;
    private float fadeOutDuration = 0.2f;

    private bool isAnimating;
    private Coroutine animationCoroutine;

    public TrailSuccessAnimationPhase(QTEVisualController visual, Vector3 targetOrbPosition, MonoBehaviour runner)
    {
        visualController = visual;
        orbPosition = targetOrbPosition;
        coroutineRunner = runner;
    }

    public void Enter()
    {
        isAnimating = true;
        
        if (visualController != null)
        {
            trailRenderer = visualController.GetTrailRenderer();
        }

        if (coroutineRunner != null && trailRenderer != null)
        {
            animationCoroutine = coroutineRunner.StartCoroutine(PlaySuccessAnimation());
        }
        else
        {
            Debug.LogError("TrailSuccessAnimationPhase: Missing references!");
            OnPhaseComplete?.Invoke(true);
        }
    }

    public void update()
    {
        //Animation is handled by coroutines
    }

    public void Exit()
    {
        isAnimating = false;
        
        if (animationCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator PlaySuccessAnimation()
    {
        yield return coroutineRunner.StartCoroutine(RotateAndPulse());
        yield return coroutineRunner.StartCoroutine(MoveToOrbAndShrink());
        yield return coroutineRunner.StartCoroutine(trailRenderer.FadeOut(fadeOutDuration));

        isAnimating = false;
        OnPhaseComplete?.Invoke(true);
    }

    private IEnumerator RotateAndPulse()
    {
        float elapsed = 0f;
        float totalDuration = accelerationDuration + maxSpeedDuration;
        
        Vector3 startPosition = trailRenderer.transform.position;
        float currentRotationSpeed = 0f;
        float maxRotationSpeed = 360f * 3f;
        float currentRotation = 0f;

        Color baseColor = trailRenderer.GetTrailColor();
        Color emissiveColor = baseColor * 3f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            //Accelerate first, then maintain speed for 1sec
            if (elapsed < accelerationDuration)
            {
                float aT = elapsed / accelerationDuration;
                currentRotationSpeed = Mathf.Lerp(0f, maxRotationSpeed, aT);
            }
            else
            {
                currentRotationSpeed = maxRotationSpeed;
            }

            currentRotation += currentRotationSpeed * Time.unscaledDeltaTime;
            trailRenderer.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

            float pulseSpeed = 8f;
            float pulse = Mathf.Abs(Mathf.Sin(elapsed * pulseSpeed));
            Color currentColor = Color.Lerp(baseColor, emissiveColor, pulse);
            trailRenderer.SetTrailColor(currentColor);

            yield return null;
        }
    }

    private IEnumerator MoveToOrbAndShrink()
    {
        Vector3 startPosition = trailRenderer.transform.position;
        Vector3 startScale = trailRenderer.transform.localScale;
        Vector3 targetScale = startScale * finalScale;

        float elapsed = 0f;

        while (elapsed < moveToOrbDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / moveToOrbDuration;

            //Esta funcion es lo mismo q lerp pero flow curva inout, calculando el val t así podemos smoothear los lerps siguientes
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            trailRenderer.transform.position = Vector3.Lerp(startPosition, orbPosition, easedT);
            trailRenderer.transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);

            float rotationT = 1f - easedT;
            trailRenderer.transform.Rotate(0f, 0f, 180f * rotationT * Time.unscaledDeltaTime);

            yield return null;
        }

        //Ensure final values
        trailRenderer.transform.position = orbPosition;
        trailRenderer.transform.localScale = targetScale;
    }
}
