using TMPro;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class SpellSlotBehaviour : MonoBehaviour
{

    private bool selected = false;

    [Header("Content")]
    [SerializeField]
    private TMP_Text spellNameText;
    [SerializeField]
    private GameObject recipieParent;
    [SerializeField]
    private GameObject elementSpellSlot;

    [Header("animation")]
    [SerializeField]
    private float animationDuration = 0.5f;
    private float animationTime = 0f;  

    [SerializeField]
    private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    private Image selectedBG;

    private Spell currentSpell;

    private int slotIndex;

    private SpellSlotBehaviour nextSlot;

    public SpellSlotBehaviour NextSlot => nextSlot;

    private void Start()
    {
        selectedBG = GetComponent<Image>();
    }

    public void Initialize(int slot, SpellSlotBehaviour nextSlot)
    {
        slotIndex = slot;
        SetSpell(GameManager.Instance.PlayerStats.SpellBook[slot]);
        this.nextSlot = nextSlot;
    }

    private void Update()
    {
        if(selected && animationTime < animationDuration)
        {
            animationTime += Time.deltaTime;
            float a = curve.Evaluate(animationTime / animationDuration);
            selectedBG.color = new Color(selectedBG.color.r, selectedBG.color.g, selectedBG.color.b, a);
        }
        else if (!selected && animationTime > 0)
        {
            animationTime -= Time.deltaTime;
            float a = curve.Evaluate(animationTime / animationDuration);
            selectedBG.color = new Color(selectedBG.color.r, selectedBG.color.g, selectedBG.color.b, a);
        }
    }

    public void Select() { selected = true; }

    public void UnSelect() { selected = false; }

    public void SetNewSpell(Spell spell)
    {
        if (spell == null || GameManager.Instance.PlayerStats.ExistSpellOnSpellBook(spell)) return;
        
        GameManager.Instance.PlayerStats.AddSpellToSpellBook(spell, slotIndex);

        SetSpell(spell);
    }

    private void SetSpell(Spell spell)
    {
        if (spell == null) return;

        currentSpell = spell;

        spellNameText.text = spell.Name;

        foreach (Transform child in recipieParent.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < spell.Recipe.Count; i++)
        {
            GameObject newElementSlot = Instantiate(elementSpellSlot, recipieParent.transform);
            newElementSlot.GetComponent<Image>().sprite = spell.Recipe[i].Sprite;
        }
    }
}
