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
    
    private float accelerationDuration = 0.5f;
    private float maxSpeedDuration = 0.5f;
    private float scaleDownDuration = 1.8f;
    private float moveToOrbDuration = 0.5f;
    private float finalScale = 0.1f;
    private Coroutine animationCoroutine;

    public TrailSuccessAnimationPhase(QTEVisualController visual, Vector3 targetOrbPosition, MonoBehaviour runner)
    {   
        visualController = visual;
        orbPosition = targetOrbPosition;
        coroutineRunner = runner;
    }

    public void Enter()
    {
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
        if (animationCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator PlaySuccessAnimation()
    {
        yield return coroutineRunner.StartCoroutine(RotateAndPulse());
        yield return coroutineRunner.StartCoroutine(ScaleDownAtMaxSpeed());
        yield return coroutineRunner.StartCoroutine(MoveToOrb());
        //yield return coroutineRunner.StartCoroutine(trailRenderer.FadeOut(fadeOutDuration));

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

    private IEnumerator ScaleDownAtMaxSpeed()
    {
        Vector3 startScale = trailRenderer.transform.localScale;
        Vector3 targetScale = startScale * finalScale;
        float maxRotationSpeed = 360f * 3f;

        float elapsed = 0f;

        while (elapsed < scaleDownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scaleDownDuration;

            float easedT = Mathf.SmoothStep(0f, 1f, t);
            trailRenderer.transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);

            trailRenderer.transform.Rotate(0f, 0f, maxRotationSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        //Ensure final scale is set
        trailRenderer.transform.localScale = targetScale;
    }

    private IEnumerator MoveToOrb()
    {
        Vector3 startPosition = trailRenderer.transform.position;
        float elapsed = 0f;
        float maxRotationSpeed = 360f * 3f;

        while (elapsed < moveToOrbDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / moveToOrbDuration;

            //Esta funcion es lo mismo q lerp pero flow curva inout, calculando el val t así podemos smoothear los lerps siguientes
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            trailRenderer.transform.position = Vector3.Lerp(startPosition, orbPosition, easedT);

            float rotationT = 1f - easedT;
            trailRenderer.transform.Rotate(0f, 0f, maxRotationSpeed * rotationT * Time.unscaledDeltaTime);

            yield return null;
        }

        trailRenderer.transform.position = orbPosition;
    }
}
