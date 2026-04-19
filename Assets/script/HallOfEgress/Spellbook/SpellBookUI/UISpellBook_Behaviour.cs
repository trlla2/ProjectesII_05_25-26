using UnityEngine;

public class UISpellBook_Behaviour : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private GameObject spellUI;
    private void Start()
    {
        GameManager.Instance.PlayerStats.OnSpellBookChange += UpdateSpellBook;

        UpdateSpellBook();
    }

    private void UpdateSpellBook()
    {

        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

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

    private void OnDestroy()
    {
        GameManager.Instance.PlayerStats.OnSpellBookChange -= UpdateSpellBook;

    }

}