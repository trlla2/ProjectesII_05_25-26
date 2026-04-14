using System.Collections.Generic;
using UnityEngine;

public class StatLevelBoxManager : MonoBehaviour
{
    [SerializeField] private StatLevelBox _defenseBoxPrefab;
    [SerializeField] private StatLevelBox _damageBoxPrefab;
    [SerializeField] private StatLevelBox _evasionBoxPrefab;
    [SerializeField] private StatLevelBox _maxHealthBoxPrefab;
    [SerializeField] private StatLevelBox _critChanceBoxPrefab;
    [SerializeField] private Transform _boxContainer;

    private Dictionary<ModificableStat, StatLevelBox> _activeBoxes
        = new Dictionary<ModificableStat, StatLevelBox>();

    private void OnEnable()
    {
        GameManager.Instance.PlayerStats.OnStatUpgradeLevelChanged += HandleStatChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerStats.OnStatUpgradeLevelChanged -= HandleStatChanged;
    }

    private void HandleStatChanged(ModificableStat stat, int newLevel)
    {
        if (newLevel == 0)
        {
            // Si el nivel vuelve a 0, ocultamos y eliminamos la caja
            if (_activeBoxes.TryGetValue(stat, out StatLevelBox existingBox))
            {
                existingBox.UpdateBox(0);
                _activeBoxes.Remove(stat);
            }
            return;
        }

        if (_activeBoxes.TryGetValue(stat, out StatLevelBox box))
        {
            // Ya existe: sobreescribir
            box.UpdateBox(newLevel);
        }
        else
        {
            StatLevelBox _boxPrefab = null;
            // Nueva caja para este stat
            switch (stat)
            {
                case ModificableStat.DEFENSE:
                    _boxPrefab = _defenseBoxPrefab;
                    break;
                case ModificableStat.DAMAGE:
                    _boxPrefab = _damageBoxPrefab;
                    break;
                case ModificableStat.EVASION:
                    _boxPrefab = _evasionBoxPrefab;
                    break;
                case ModificableStat.HEALTH:
                    _boxPrefab = _maxHealthBoxPrefab;
                    break;
                case ModificableStat.CRIT_CHANCE:
                    _boxPrefab = _critChanceBoxPrefab;
                    break;
                default: break;
            }
            StatLevelBox newBox = Instantiate(_boxPrefab, _boxContainer);
            newBox.SetUp(stat, newLevel);
            _activeBoxes[stat] = newBox;
        }
    }
}