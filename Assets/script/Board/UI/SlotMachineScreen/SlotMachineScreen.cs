using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class SlotMachineScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text _prizeText;
    [SerializeField] private TMP_Text _betAmountText;
    [SerializeField] private List<SlotMachineRow> _rows;
    [SerializeField] JackPotAnimation _jackPotAnimation;
    [SerializeField] private MoneyUIHandler moneyUI;
    [SerializeField] private TMP_Text _tries;
    [SerializeField] private GameObject _jackpotParticles;

    private float _closeWaitTime = 1f;
    private float _closeWaitTimeJackpot = 5f;
    private bool _isJackpot = false;

    private int _currentBet = 100;
    private int _basePulls = 3;
    private int _avaliablePulls = 1;

    private int _baseBet = 50;
    private int _minBet = 10;
    private int _bet = 10;

    private float _jackpotMult = 5f;
    private float _threeMult = 3f;
    private float _fullHouse = 2f;

    

    private bool _resultsChecked = false;

    private SlotMachineTile _openedTile;

    [SerializeField]private TranslatorUI _translator;

    public UnityEvent OnWin;
    public UnityEvent OnJackpot;


    void Update()
    {
        _betAmountText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "BetText")} " +
            $"{_currentBet} ";
            //$"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "GoldsText")}";

        if (!_rows[0].rowStopped || !_rows[1].rowStopped || !_rows[2].rowStopped || !_rows[3].rowStopped)
        {
            _prizeText.enabled = false;
            _resultsChecked = false;
        }

        if (_rows[0].rowStopped && _rows[1].rowStopped && _rows[2].rowStopped && _rows[3].rowStopped && !_resultsChecked)
        {
            CheckResults();
            if (_avaliablePulls < 1)
                StartCoroutine(ClosePanelCoroutine());
        }

        _tries.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "TriesText")} {_avaliablePulls}";
    }

    public void TryPull()
    {
        if (_avaliablePulls < 1) return;
        foreach (SlotMachineRow row in _rows)
        {
            if (!row.rowStopped)
            {
                return;
            }
        }

        if(!GameManager.Instance.PlayerStats.CanUseCurrency(CurrencyType.GOLD, _currentBet))
            return;

        GameManager.Instance.PlayerStats.UseCurrency(CurrencyType.GOLD, _currentBet);

        _jackPotAnimation.UnShowText();
        _prizeText.text = "";
        _avaliablePulls--;
        foreach (SlotMachineRow row in _rows)
        {
            row.StartRotatig();
        }
        
    }

    private void CheckResults()
    {
        if (_resultsChecked) return;
        if (_rows[0].stoppedSlot == "" || _rows[1].stoppedSlot == "" || _rows[2].stoppedSlot == "" || _rows[3].stoppedSlot == "") return;

        _prizeText.enabled = true;
        if (_rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot)
        {
            // 4 of the same
            _prizeText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "WinText")}" +
                $" {_currentBet * _jackpotMult} " +
                $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "GoldsText")}!!!";

            _jackPotAnimation.StartAnimation();
            GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Mathf.RoundToInt(_currentBet * _jackpotMult));
            _isJackpot = true;
            SpawnJackpotParticles();
            AnimateJackpot();
            OnJackpot.Invoke();
        }
        else if (_rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[2].stoppedSlot ||
            _rows[1].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot ||
            _rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[3].stoppedSlot ||
            _rows[0].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot)
        {
            // 3 of the same
            _prizeText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "WinText")}" +
                $"{_currentBet * _threeMult} " +
                $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "GoldsText")}!!!";

            GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Mathf.RoundToInt(_currentBet * _threeMult));
            _isJackpot = false;
            AnimateThreeOfAKind();
            OnWin.Invoke();
        }
        else if (_rows[0].stoppedSlot != _rows[1].stoppedSlot && 
                 _rows[1].stoppedSlot != _rows[2].stoppedSlot &&
                 _rows[2].stoppedSlot != _rows[3].stoppedSlot &&
                 _rows[3].stoppedSlot != _rows[0].stoppedSlot &&
                 _rows[0].stoppedSlot != _rows[2].stoppedSlot &&
                 _rows[1].stoppedSlot != _rows[3].stoppedSlot)
        {
            // Full house
            _prizeText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "WinText")}" +
                $"{_currentBet * _fullHouse} " +
                $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "GoldsText")}" +
                $"\n {LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "ExtraRollText")}!!!";
            GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Mathf.RoundToInt(_currentBet * _fullHouse));
            _avaliablePulls++;
            _isJackpot = false;
            AnimateFullHouse();
            OnWin.Invoke();
        }
        else
        {
            // 2 of the same
            _prizeText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("SlotMachine-PrizesTexts", "LoseText")}";
            _isJackpot = false;
        }
        _resultsChecked = true;
    }

    private Image GetVisualElementForSlot(SlotMachineRow row, string slotName)
    {
        foreach (Transform child in row.transform)
        {
            //System.StringComparison.OrdinalIgnoreCase is to pass from lowercase to uppercase
            if (child.name.Equals(slotName, System.StringComparison.OrdinalIgnoreCase))
                return child.GetComponent<Image>();
        }
        
        return null;
    }

    private void AnimateThreeOfAKind()
    {
        List<SlotMachineRow> matchingRows = GetMatchingRows(3);
        string matchingSlot = matchingRows[0].stoppedSlot;
        
        foreach (var row in matchingRows)
        {
            Image visualElement = GetVisualElementForSlot(row, matchingSlot);
            if (visualElement != null)
            {
                visualElement.rectTransform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.5f);
            }
        }
    }

    private void AnimateJackpot()
    {
        string matchingSlot = _rows[0].stoppedSlot;
        
        foreach (var row in _rows)
        {
            Image visualElement = GetVisualElementForSlot(row, matchingSlot);
            if (visualElement != null)
            {
                visualElement.rectTransform.DOPunchScale(Vector3.one * 0.5f, 0.8f, 8, 0.7f);
                StartCoroutine(BlinkElement(visualElement, 0.8f));
            }
        }
    }

    private IEnumerator BlinkElement(Image image, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            image.enabled = !image.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        image.enabled = true;
    }

    private void AnimateFullHouse()
    {
        Sequence sequence = DOTween.Sequence();
        
        for (int i = 0; i < _rows.Count; i++)
        {
            string slotName = _rows[i].stoppedSlot;
            Image visualElement = GetVisualElementForSlot(_rows[i], slotName);
            
            if (visualElement != null)
            {
                RectTransform rectTransform = visualElement.rectTransform;
                Vector2 originalPos = rectTransform.anchoredPosition;
                
                sequence.Append(rectTransform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 4, 0.5f));
                sequence.Join(rectTransform.DOAnchorPosY(originalPos.y + 20f, 0.15f).SetLoops(2, LoopType.Yoyo));
            }
        }
    }

    private List<SlotMachineRow> GetMatchingRows(int count)
    {
        List<SlotMachineRow> matching = new List<SlotMachineRow>();
        
        // Check all possible 3-match combinations
        if (_rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[2].stoppedSlot)
        {
            matching.AddRange(new[] { _rows[0], _rows[1], _rows[2] });
        }
        else if (_rows[1].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot)
        {
            matching.AddRange(new[] { _rows[1], _rows[2], _rows[3] });
        }
        else if (_rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[3].stoppedSlot)
        {
            matching.AddRange(new[] { _rows[0], _rows[1], _rows[3] });
        }
        else if (_rows[0].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot)
        {
            matching.AddRange(new[] { _rows[0], _rows[2], _rows[3] });
        }
        
        return matching;
    }

    private void SpawnJackpotParticles()
    {
        if (_jackpotParticles == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, Camera.main.nearClipPlane + 1f);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);

        GameObject parts = Instantiate(_jackpotParticles, worldCenter, Quaternion.identity);
        parts.transform.localScale *= 0.5f;
        ParticleSystem particles = parts.GetComponent<ParticleSystem>();
        particles.Stop();
        particles.Play();
    }

    public void IncreaseBet()
    {
        if (!GameManager.Instance.PlayerStats.CanUseCurrency(CurrencyType.GOLD, _currentBet + _bet))
            return;

        _currentBet += _bet;
    }
    public void DecreaseBet()
    {


        if (_currentBet > _minBet)
            _currentBet -= _bet;
    }

    public void Open(SlotMachineTile current)
    {
        if (current == null || _openedTile != null) return;
        _openedTile = current;

        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        _avaliablePulls = _basePulls;
        _currentBet = _baseBet;
        _jackPotAnimation.UnShowText();
        _prizeText.text = "";
        _translator.ToTarget();

        moneyUI.StayVisible();
    }

    private IEnumerator ClosePanelCoroutine()
    {
        if(_isJackpot)
            yield return new WaitForSeconds(_closeWaitTimeJackpot);
        else
            yield return new WaitForSeconds(_closeWaitTime);

        Close();
    }

    public void Close()
    {
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _translator.ToOrigin();
        _jackPotAnimation.UnShowText();
        moneyUI.UnShow();

        _openedTile.ReplaceWithPostInteraction();
        _openedTile = null;
    }
}
