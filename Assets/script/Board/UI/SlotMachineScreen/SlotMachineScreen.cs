using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
        _betAmountText.text = $"BET: {_currentBet} GOLDS";

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

        _tries.text = $"Tries Left: {_avaliablePulls}";
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

        GameManager.Instance.PlayerStats.UseMoney(CurrencyType.GOLD, _currentBet);

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
            // 4 of the same // NEW SPELL (4-match spells)
            _prizeText.text = $"YOU WON {_currentBet * _jackpotMult} GOLDS!!!";
            _jackPotAnimation.StartAnimation();
            GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Mathf.RoundToInt(_currentBet * _jackpotMult));
            _isJackpot = true;
            SpawnJackpotParticles();
            OnJackpot.Invoke();
        }
        else if (_rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[2].stoppedSlot ||
            _rows[1].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot ||
            _rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[3].stoppedSlot ||
            _rows[0].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot)
        {
            // 3 of the same
            _prizeText.text = $"YOU WON {_currentBet * _threeMult} GOLDS!!!";
            GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Mathf.RoundToInt(_currentBet * _threeMult));
            _isJackpot = false;
            OnWin.Invoke();
        }
        else if (_rows[0].stoppedSlot != _rows[1].stoppedSlot && 
                 _rows[1].stoppedSlot != _rows[2].stoppedSlot &&
                 _rows[2].stoppedSlot != _rows[3].stoppedSlot &&
                 _rows[3].stoppedSlot != _rows[0].stoppedSlot &&
                 _rows[0].stoppedSlot != _rows[2].stoppedSlot &&
                 _rows[1].stoppedSlot != _rows[3].stoppedSlot)
        {
            _prizeText.text = $"YOU WON {_currentBet * _fullHouse} GOLDS \n + EXTRA ROLL!!!";
            GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, Mathf.RoundToInt(_currentBet * _fullHouse));
            _avaliablePulls++;
            _isJackpot = false;
            OnWin.Invoke();
        }
        else
        {
            // 2 of the same
            _prizeText.text = "YOU LOST";
            _isJackpot = false;
        }
        _resultsChecked = true;
    }

    private void SpawnJackpotParticles()
    {
        if (_jackpotParticles == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, Camera.main.nearClipPlane + 1f);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);

        GameObject parts = Instantiate(_jackpotParticles, worldCenter, Quaternion.identity);
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
