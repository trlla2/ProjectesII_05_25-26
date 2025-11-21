using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class PlayerPathing : MonoBehaviour
{
    public Grid grid;
    public Transform cursorVisual;

    [Header("Pathing Settings")]
    public int maxPathLength = 6;
    public float cursorLerpSpeed = 5f;
    public float SelectScaleAmount = 0.2f;
    public float SelectScaleTime = 0.2f;


    private float adjacencyDistance;

    private LineRenderer line;
    private List<Slot> currentPath = new List<Slot>();

    public SpellPathAnimator spellAnimator;

    private bool isPathing = false;

    public bool canPath = true;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;

        adjacencyDistance = grid.XDistanceBetween * 1.47f; //es el resultao de 1.25*sqrt(2), 1.25 es la dist entre slots i raiz d 2 para las diagonales

        if (grid == null)
        {
            Debug.LogError("No Grid reference!");
        }


        GameManager.Instance.OnEndBattle += StopPathing;
    }
    private void FixedUpdate()
    {
        UpdateCursorPosition();
    }

    void Update()
    {
        HandleMouseInput();
        UpdateLineRenderer();

        if (!isPathing && line.positionCount != 0)
        {
            line.positionCount = 0;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndBattle -= StopPathing;
    }

    void HandleMouseInput()
    {
        if (!canPath) return;

        if (Input.GetMouseButtonDown(0))
        {
            BeginPathing();
        }
        if (Input.GetMouseButton(0) && isPathing)
        {
            ContinuePathing();
        }
        if (Input.GetMouseButtonUp(0) && isPathing)
        {
            EndPathing();
        }
    }

    void BeginPathing()
    {
        if (grid.isScrolling) return;

        isPathing = true;
        currentPath.Clear();
        line.positionCount = 0;
        grid.IsPlayerPathing = true;
    }

    void ContinuePathing()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector3.forward);

        if (hit.collider == null) return;

        if (cursorVisual != null)
        {
            cursorVisual.gameObject.SetActive(true);
        }

        SlotHolder holder = hit.collider.GetComponent<SlotHolder>();
        if (holder == null) return;

        Slot slot = holder.LinkedSlot;
        if (slot == null || slot.Animated) return;

        if (!currentPath.Contains(slot))
        {
            //Can add if first slot or adjacent to last slot
            if (currentPath.Count == 0 || IsAdjacentWorld(slot, currentPath[^1]))
            {
                if (currentPath.Count < maxPathLength)
                {
                    currentPath.Add(slot);
                    holder.Highlighter().Highlight();

                    if (slot.scaleAnimator != null)
                    {
                        slot.scaleAnimator.PlayScale(SelectScaleAmount, SelectScaleTime);
                    }

                    UpdateLineRenderer();
                }
            }
        }
        else if (currentPath.Count >= 2 && slot == currentPath[^2])
        {
            //Allow backtracking
            Slot removed = currentPath[^1];

            if (removed.Holder != null)
            {
                removed.Holder.GetComponent<SlotHolder>().Highlighter().ResetHighlight();
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            UpdateLineRenderer();
        }
    }

    void EndPathing()
    {
        isPathing = false;
        grid.IsPlayerPathing = false;

        TryCastSpell();

        foreach (Slot s in currentPath)
        {
            if (s != null && s.Holder != null)
            {
                s.Holder.GetComponent<SlotHolder>().Highlighter().ResetHighlight();
            }
        }

        currentPath.Clear();
        line.positionCount = 0;

        if (cursorVisual != null)
        {
            cursorVisual.gameObject.SetActive(false);
        }
    }

    void TryCastSpell()
    {
        if (currentPath.Count == 0) return;

        List<Element> pathElements = new List<Element>();
        foreach (Slot s in currentPath)
        {
            pathElements.Add(s.CurrentElement);
        }

        foreach (Spell spell in PlayerStats.Instance.GetSpellBook())
        {
            if (spell.Matches(pathElements))
            {
                List<Slot> animationPath = new List<Slot>(currentPath);

                if (spellAnimator != null)
                {
                    spellAnimator.OnSpellCast(animationPath, spell);
                }

                OnSpellCast(spell);
                if (spell.OnCast != null) spell.OnCast.Invoke();

                grid.DeleteElements(currentPath);
                grid.ResetWaitTimer();

                Debug.Log($"Spell cast: " + spell.Name);

                return;
            }
        }

        Debug.Log("No spell matched.");
    }

    public void OnSpellCast(Spell spell)
    {
        PlayerStats.Instance.Attack(spell);
        if(spell.Name == "HEAL")
        {
            PlayerStats.Instance.Heal(30);
        }
    }

    public void ForceEndPathing()
    {
        isPathing = false;
        grid.IsPlayerPathing = false;
        line.positionCount = 0;
        currentPath.Clear();
        cursorVisual.gameObject.SetActive(false);
    }

    private void StopPathing(bool isWin, int exp, int money)
    {
        canPath = false;
    }

    void UpdateLineRenderer()
    {
        if (currentPath.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = currentPath.Count;

        for (int i = 0; i < currentPath.Count; i++)
        {
            if (currentPath[i]?.Holder != null)
            {
                Vector3 pos = currentPath[i].Holder.transform.position;
                line.SetPosition(i, pos);
            }
        }
    }

    void UpdateCursorPosition()
    {
        if (!isPathing)
        {
            cursorVisual.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorVisual.transform.position = new Vector3(cursorVisual.transform.position.x, cursorVisual.transform.position.y, 0);
        }
        if (!isPathing || currentPath.Count == 0 || cursorVisual == null)
            return;

        Slot lastSlot = currentPath.Last();
        if (lastSlot.Holder != null)
        {
            Vector3 targetPos = lastSlot.Holder.transform.position;
            cursorVisual.position = Vector3.Lerp(
                cursorVisual.position,
                targetPos,
                cursorLerpSpeed * Time.fixedDeltaTime
            );
        }
    }

    bool IsAdjacentWorld(Slot a, Slot b)
    {
        if (a?.Holder == null || b?.Holder == null) return false;

        float dist = Vector2.Distance(
            a.Holder.transform.position,
            b.Holder.transform.position
        );

        return dist <= adjacencyDistance;
    }
}