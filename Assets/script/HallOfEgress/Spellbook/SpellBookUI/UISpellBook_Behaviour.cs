using UnityEngine;

public class UISpellBook_Behaviour : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private GameObject spellUI;
    private void Start()
    {
        foreach (Spell s in GameManager.Instance.PlayerStats.GetSpellBook())
        {
            GameObject spellUIGO = Instantiate(spellUI, this.transform);
            UISpell_Behaviour spellUIBehaviour = spellUIGO.GetComponent<UISpell_Behaviour>();
            if (spellUIBehaviour != null)
            {
                spellUIBehaviour.SetupSpellUI(s);
            }
        }
    }

}