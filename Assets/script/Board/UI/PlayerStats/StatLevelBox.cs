using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ModificableStat
{
    HEALTH,
    DAMAGE,
    DEFENSE,
    CRIT_CHANCE,
    EVASION
}

public class StatLevelBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text[] _levelText;
    //[SerializeField] private Image _background;

    //[Header("Colors")]
    //[SerializeField] private Color _positiveColor = new Color(0.2f, 0.8f, 0.3f);
    //[SerializeField] private Color _negativeColor = new Color(0.9f, 0.2f, 0.2f);

    private ModificableStat _stat;

    public ModificableStat Stat => _stat;

    public void SetUp(ModificableStat stat, int level)
    {
        _stat = stat;
        UpdateBox(level);
    }

    public void UpdateBox(int level)
    {
        if (level == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (level > 0)
        {
            _levelText[1].gameObject.transform.parent.gameObject.SetActive(false);
            _levelText[0].gameObject.transform.parent.gameObject.SetActive(true);
            _levelText[0].text = $"{Mathf.Abs(level)}";
        }
        else
        {
            _levelText[0].gameObject.transform.parent.gameObject.SetActive(false);
            _levelText[1].gameObject.transform.parent.gameObject.SetActive(true);
            _levelText[1].text = $"{Mathf.Abs(level)}";
        }
            
    }
}
