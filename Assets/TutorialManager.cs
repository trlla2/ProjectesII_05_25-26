using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    private const string TUTORIAL_STAGE_KEY = "TutorialStage";
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    [Header("Enemy Assignment")]
    [SerializeField] private EnemyType[] tutorialEnemies;
    [SerializeField] private EnemyBehaviour enemyBehaviour;

    [Header("Tutorial Stages")]
    [SerializeField] private TutorialStage[] stages;

    [Header("Gameplay Controllers")]
    [SerializeField] private MonoBehaviour[] gameplayControllers;

    private int currentStage;
    private int currentStepIndex;
    private bool isTutorialActive;
    private bool isStepActive;

    private void Start()
    {
        //debug
        ResetTutorial();
        if (IsTutorialCompleted())
        {
            enabled = false;
            return;
        }
        currentStage = PlayerPrefs.GetInt(TUTORIAL_STAGE_KEY, 0);
        InitializeTutorial();
    }

    private void InitializeTutorial()
    {
        if (currentStage >= stages.Length)
        {
            CompleteTutorial();
            return;
        }

        AssignEnemyForStage();
        isTutorialActive = true;
        StartStage();
    }

    private void AssignEnemyForStage()
    {
        if (enemyBehaviour == null || tutorialEnemies == null || currentStage >= tutorialEnemies.Length)
            return;

        enemyBehaviour.SetEnemyType(tutorialEnemies[currentStage]);
    }

    private void StartStage()
    {
        if (currentStage >= stages.Length)
            return;

        currentStepIndex = 0;
        ExecuteCurrentStep();
    }

    private void ExecuteCurrentStep()
    {
        TutorialStage stage = stages[currentStage];

        if (currentStepIndex >= stage.steps.Length)
        {
            CompleteStage();
            return;
        }

        TutorialStep step = stage.steps[currentStepIndex];
        isStepActive = true;

        if (step.pauseGameplay)
        {
            PauseGameplay();
        }

        SetObjectsActive(step.highlightObjects, true);

        step.onStepStart?.Invoke();

        if (step.completionType == StepCompletionType.Time)
        {
            StartCoroutine(TimeBasedCompletion(step.timeToComplete));
        }
    }

    private IEnumerator TimeBasedCompletion(float duration)
    {
        yield return new WaitForSeconds(duration);
        CompleteCurrentStep();
    }

    public void CompleteCurrentStep()
    {
        if (!isStepActive)
            return;

        TutorialStage stage = stages[currentStage];
        TutorialStep step = stage.steps[currentStepIndex];

        isStepActive = false;
        SetObjectsActive(step.highlightObjects, false);
        step.onStepComplete?.Invoke();

        if (step.pauseGameplay)
        {
            ResumeGameplay();
        }

        currentStepIndex++;
        ExecuteCurrentStep();
    }

    private void CompleteStage()
    {
        ResumeGameplay();
        
        currentStage++;
        PlayerPrefs.SetInt(TUTORIAL_STAGE_KEY, currentStage);
        PlayerPrefs.Save();

        if (currentStage >= stages.Length)
        {
            CompleteTutorial();
        }
        else
        {
            InitializeTutorial();
        }
    }

    private void CompleteTutorial()
    {
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();
        isTutorialActive = false;
        ResumeGameplay();
        enabled = false;
    }

    private void PauseGameplay()
    {
        if (enemyBehaviour != null)
        {
            enemyBehaviour.StopBattle();
        }

        SetGameplayControllersEnabled(false);
    }

    private void ResumeGameplay()
    {
        if (enemyBehaviour != null && GameManager.Instance != null)
        {
            enemyBehaviour.StartBattle();
        }

        SetGameplayControllersEnabled(true);
    }

    private void SetGameplayControllersEnabled(bool enabled)
    {
        foreach (MonoBehaviour controller in gameplayControllers)
        {
            if (controller != null)
            {
                controller.enabled = enabled;
            }
        }
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    private bool IsTutorialCompleted()
    {
        return PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_STAGE_KEY);
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        PlayerPrefs.Save();
    }

    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}

[System.Serializable]
public class TutorialStage
{
    public string stageName;
    public TutorialStep[] steps;
}

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string stepDescription;
    public GameObject[] highlightObjects;
    public StepCompletionType completionType;
    public float timeToComplete = 3f;
    public bool pauseGameplay = true;
    public UnityEvent onStepStart;
    public UnityEvent onStepComplete;
}

public enum StepCompletionType
{
    Event,
    Time
}