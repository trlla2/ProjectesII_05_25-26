using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        
        Color c = fadePanel.color;
        c.a = 0f;
        fadePanel.color = c;
    }

    public void LoadScene(string sceneName, bool isAdditive)
    {
        StartCoroutine(TransitionToScene(sceneName, isAdditive));
    }

    public void UnloadScene(string sceneName)
    {
        StartCoroutine(TransitionUnloadScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName, bool isAdditive)
    {
        yield return StartCoroutine(Fade(1f));

        if(isAdditive)
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        else
            SceneManager.LoadScene(sceneName);
        
        yield return StartCoroutine(Fade(0f));
    }

    
    private IEnumerator TransitionUnloadScene(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));
        SceneManager.UnloadSceneAsync(sceneName);
        
        yield return StartCoroutine(Fade(0f));
    }


    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanel.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);

            Color c = fadePanel.color;
            c.a = newAlpha;
            fadePanel.color = c;

            yield return null;
        }

        Color finalColor = fadePanel.color;
        finalColor.a = targetAlpha;
        fadePanel.color = finalColor;
    }
}
