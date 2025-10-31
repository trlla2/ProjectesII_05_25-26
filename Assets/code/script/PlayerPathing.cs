using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(LineRenderer))]
public class PlayerPathing : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public Transform cursorVisual;

    [Header("Pathing Settings")]
    public int maxPathLength = 6;
    public float cursorLerpSpeed = 5f;
    
    private float adjacencyDistance = 1.76776695297f; //es el resultao de 1.25*sqrt(2), 1.25 es la dist entre slots i raiz d 2 para las diagonales

    private LineRenderer line;
    private List<Slot> currentPath = new List<Slot>();
    private Camera mainCam;
    private bool isPathing = false;
    private Vector3 targetCursorPos;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        mainCam = Camera.main;
        line.positionCount = 0;
        if (grid == null) Debug.LogError("No GRid reference!!");
    }

    void Update()
    {
        HandleMouseInput();
        UpdateLineRenderer();
        UpdateCursorPosition();

        if (!isPathing && line.positionCount != 0)
            line.positionCount = 0;
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginPathing();
        }
        else if (Input.GetMouseButton(0) && isPathing)
        {
            ContinuePathing();
        }
        else if (Input.GetMouseButtonUp(0) && isPathing)
        {
            EndPathing();
        }
    }

    void BeginPathing()
    {
        isPathing = true;
        currentPath.Clear();
        line.positionCount = 0;
        grid.IsPlayerPathing = true;
        if (cursorVisual != null)
            cursorVisual.gameObject.SetActive(true);

    }

    void ContinuePathing()
    {
        isPathing = true;
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        SlotHolder holder = hit.collider.GetComponent<SlotHolder>();
        if (holder == null) return;

        Slot slot = holder.LinkedSlot;

        if (!currentPath.Contains(slot))
        {
            if (currentPath.Count == 0 || IsAdjacentWorld(slot, currentPath[^1]))
            {
                if (currentPath.Count < maxPathLength)
                {
                    currentPath.Add(slot);
                    holder.Highlighter().Highlight();
                    UpdateLineRenderer();
                }
            }
        }
        else if (currentPath.Count >= 2 && slot == currentPath[^2])
        {
            Slot removed = currentPath[^1];
            removed.Holder.GetComponent<SlotHolder>().Highlighter().ResetHighlight();
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
                s.Holder.GetComponent<SlotHolder>().Highlighter().ResetHighlight();
        }

        currentPath.Clear();
        line.positionCount = 0;
        if (cursorVisual != null)
            cursorVisual.gameObject.SetActive(false);

    }


    void TryCastSpell()
    {
        if (currentPath.Count == 0) return;

        List<Element> pathElements = new List<Element>();
        foreach (Slot s in currentPath)
            pathElements.Add(s.CurrentElement);

        foreach (Spell spell in grid.availableSpells)
        {
            if (spell.Matches(pathElements))
            {
                Debug.Log($"Spell cast: {spell.Name}");
                OnSpellCast(spell);
                return;
            }
        }

        Debug.Log("No spell matched.");
    }

    public void OnSpellCast(Spell spell)
    {
        this.gameObject.GetComponent<PlayerStats>().Attack(spell);
    }

    void UpdateLineRenderer()
    {
        if (currentPath.Count == 0) return;

        line.positionCount = currentPath.Count;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 pos = currentPath[i].Holder.transform.position;
            pos.y += grid.FallSpeed * grid.currentFallSpeedMultiplier * Time.deltaTime;
            line.SetPosition(i, pos);
        }
    }

    void UpdateCursorPosition()
    {
        if (!isPathing || currentPath.Count == 0 || cursorVisual == null)
            return;

        cursorVisual.position = new Vector3(currentPath.LastOrDefault().Holder.transform.position.x, currentPath.LastOrDefault().Holder.transform.position.y, 0);
    }


    bool IsAdjacentWorld(Slot a, Slot b)
    {
        float dist = Vector2.Distance(a.Holder.transform.position, b.Holder.transform.position);
        return dist <= adjacencyDistance;
    }
}
