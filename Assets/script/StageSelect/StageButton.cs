using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private GameObject _selectedIndicator;

    private int _stageIndex;
    private Action<StageButton, int> _onSelected;

    public void Setup(int stageIndex, string displayName, bool isLocked, bool isDefault, Action<StageButton, int> onSelected)
    {
        _stageIndex = stageIndex;
        _onSelected = onSelected;

        _label.text = displayName;
        _lockedOverlay.SetActive(isLocked);
        _button.interactable = !isLocked;
        _button.onClick.AddListener(OnClick);

        SetSelected(false);

        if (isDefault)
            OnClick();
    }

    public void SetSelected(bool selected)
    {
        if (_selectedIndicator != null)
            _selectedIndicator.SetActive(selected);
    }

    private void OnClick()
    {
        _onSelected?.Invoke(this, _stageIndex);
    }
}
