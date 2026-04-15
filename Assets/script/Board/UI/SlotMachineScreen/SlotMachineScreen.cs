using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlotMachineScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text _prizeText;
    [SerializeField] private TMP_Text _betAmountText;
    [SerializeField] private List<SlotMachineRow> _rows;
    [SerializeField] JackPotAnimation _jackPotAnimation;
    [SerializeField] private MoneyUIHandler moneyUI;

    private int _currentBet = 100;
    private int prize;
    private int avaliablePulls = 1;

    private bool _resultsChecked = false;

    [SerializeField]private TranslatorUI _translator;

    private void Start()
    {
        Open();
    }

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
            if (avaliablePulls < 1)
                StartCoroutine(ClosePanelCoroutine());
        }
    }

    public void TryPull()
    {
        if (avaliablePulls < 1) return;
        foreach (SlotMachineRow row in _rows)
        {
            if (!row.rowStopped)
            {
                return;
            }
        }

        _jackPotAnimation.UnShowText();
        _prizeText.text = "";
        avaliablePulls--;
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
            _prizeText.text = $"YOU WON {_currentBet * 5} GOLDS!!!";
            _jackPotAnimation.StartAnimation();
        }
        else if (_rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[2].stoppedSlot ||
            _rows[1].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot ||
            _rows[0].stoppedSlot == _rows[1].stoppedSlot && _rows[1].stoppedSlot == _rows[3].stoppedSlot ||
            _rows[0].stoppedSlot == _rows[2].stoppedSlot && _rows[2].stoppedSlot == _rows[3].stoppedSlot)
        {
            // 3 of the same
            _prizeText.text = $"3 MATCHES!!!\nYOU WON {_currentBet * 3} GOLDS!!!";
        }
        else if (_rows[0].stoppedSlot != _rows[1].stoppedSlot && 
                 _rows[1].stoppedSlot != _rows[2].stoppedSlot &&
                 _rows[2].stoppedSlot != _rows[3].stoppedSlot &&
                 _rows[3].stoppedSlot != _rows[0].stoppedSlot &&
                 _rows[0].stoppedSlot != _rows[2].stoppedSlot &&
                 _rows[1].stoppedSlot != _rows[3].stoppedSlot)
        {
            _prizeText.text = $"1 ELEMENT OF EACH!!!\nYOU WON {_currentBet * 1.5f} GOLDS + EXTRA ROLL!!!";
            GameManager.Instance.PlayerStats.AddMoney(Mathf.RoundToInt(_currentBet * 1.5f));
            avaliablePulls++;
        }
        else
        {
            // 2 of the same
            _prizeText.text = "YOU LOST";
        }
        _resultsChecked = true;
    }

    public void IncreaseBet()
    {
        if (_currentBet < 500)
            _currentBet += 100;
    }
    public void DecreaseBet()
    {
        if (_currentBet > 100)
            _currentBet -= 100;
    }

    public void Open()
    {
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        avaliablePulls = 1;
        _currentBet = 100;
        _jackPotAnimation.UnShowText();
        _prizeText.text = "";
        _translator.ToTarget();

        moneyUI.StayVisible();
    }

    private IEnumerator ClosePanelCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Close();
    }

    public void Close()
    {
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _translator.ToOrigin();
        _jackPotAnimation.UnShowText();
        moneyUI.UnShow();
    }
}
