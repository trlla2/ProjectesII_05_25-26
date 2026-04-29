using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingPanel : MonoBehaviour
{
    public OnBoardingKey _key { get; private set; }
    [SerializeField] private Button dismissButton;
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private List<string> messages;
    [SerializeField] private float charDisplayDelay = 0.05f;
    private bool _activeTextisFullyDisplayed = false;
    private bool _fullExplanationCompleted = false;
    private bool _isWaitingForDismiss = false;

    private Action _onDismiss;

    private void Awake()
    {
        gameObject.SetActive(false);
        //dismissButton.onClick.AddListener(Dismiss);

        if (OnBoardingManager.Instance != null)
            OnBoardingManager.Instance.RegisterPanel(this);
    }

    private void Update()
    {
        if (textBox == null)
        {
            this.gameObject.GetComponent<Image>().raycastTarget = false;
            if (Input.GetMouseButtonDown(0))
            {
                _fullExplanationCompleted = true;
                Dismiss();
            }
        }
            
    }

    public void Initialize(OnBoardingKey key)
    {
        _key = key;
    }

    public void Show(Action onDismiss)
    {
        _onDismiss = onDismiss;
        gameObject.SetActive(true);
        if (textBox != null)
            StartCoroutine(ShowTextCharByChar());
    }

    public void Dismiss()
    {
        if (_fullExplanationCompleted)
        {
            gameObject.SetActive(false);
            _onDismiss?.Invoke();
            _onDismiss = null;
            _fullExplanationCompleted = false;
        }
        else if (_isWaitingForDismiss)
        {
            _isWaitingForDismiss = false;
            Dismiss();
        }
        else
            ForceCompleteText();
        //{
        //    if (textBox != null) ForceCompleteText();
        //    else {
        //        _fullExplanationCompleted = true;
        //        Dismiss();
        //    }
        //}
    }

    private IEnumerator ShowTextCharByChar()
    {
        
        foreach (string message in messages)
        {
            int textMaxVisibleChars = 0;
            textBox.text = message + "\n";
            _activeTextisFullyDisplayed = false;
            while (textMaxVisibleChars <= textBox.text.Length)
            {
                if (_activeTextisFullyDisplayed) break;
                textBox.maxVisibleCharacters = textMaxVisibleChars;
                textMaxVisibleChars++;
                yield return new WaitForSecondsRealtime(charDisplayDelay);
            }
            _activeTextisFullyDisplayed = true;
            _isWaitingForDismiss = true;
            while (_isWaitingForDismiss)
                yield return null;
        }
        _fullExplanationCompleted = true;
    }

    //private void OnMouseDown()
    //{
    //    if (textBox == null)
    //    {
    //        _fullExplanationCompleted = true;
    //        Dismiss();
    //    }
    //}

    private void ForceCompleteText()
    {
        //StopCoroutine(ShowTextCharByChar());
        //StopAllCoroutines();
        textBox.maxVisibleCharacters = textBox.text.Length;
        _activeTextisFullyDisplayed = true;
        if (_fullExplanationCompleted == false && textBox.text == messages[messages.Count - 1])
        {
            _fullExplanationCompleted = true;
            _isWaitingForDismiss = false;
        }
        //if (textBox.text == messages[messages.Count - 1])
        //{
        //    _fullExplanationCompleted = true;
        //    _isWaitingForDismiss = false;
        //}
    }
}