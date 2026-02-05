using UnityEngine;
using UnityEngine.UI;

public class SpellSlotBehaviour : MonoBehaviour
{

    private bool selected = false;
    [SerializeField]
    private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    private Image selectedBG;

    private Spell currentSpell;

    private void Start()
    {
        selectedBG = GetComponent<Image>();
    }

    public void Initialize(Spell s)
    {
        if (s == null) return;
        SetSpell(s);
    }

    private void Update()
    {
       // Animation Stuff
    }

    public void Select() { selected = true; }

    public void UnSelect() { selected = false; }

    public void AddNewSpell(Spell spell)
    {
        // remove spell stuff

        SetSpell(spell);
    }

    private void SetSpell(Spell spell)
    {
        currentSpell = spell;
        // Set text and stuff
        
    }
}
