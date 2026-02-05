using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject[] orbPrefab;
    [HideInInspector] public Orb[] orbs;
    public Orb currentOrb;
    public ElementType WeakElement;

    private OrbVisual[] orbVisuals;
    [SerializeField] private OrbChargeAnimator chargeAnimator;

    private int currentOrbIndex = 0;

    private void Start()
    {
        orbVisuals = new OrbVisual[orbPrefab.Length];
        orbs = new Orb[orbPrefab.Length];
        for (int i = 0; i < orbPrefab.Length; i++)
        {
            orbs[i] = orbPrefab[i].GetComponent<Orb>();
            orbVisuals[i] = orbPrefab[i].GetComponent<OrbVisual>();
        }

        orbs[0].IsActive = true;
        orbs[1].IsActive = false;
        currentOrb = orbs[0];
        currentOrbIndex = 0;

        for (int i = 0; i < orbs.Length; i++)
        {
            //como que estamos pasando lambdas, tenemos que guardar el index en una variable local para evitar problemas de variables
            int index = i;
            orbs[i].OnChargeChanged += (prev, curr) => OnOrbChargeChanged(index, prev, curr);
            orbs[i].OnStateChanged += (state) => OnOrbStateChanged(index, state);
        }
    }

    public void TryChargeOrb(List<Vector3> elementStartPositions, List<Element> matchingElements)
    {
        if (currentOrb == null || matchingElements == null || matchingElements.Count == 0)
            return;
        if (currentOrb.State == OrbState.Activated || currentOrb.State == OrbState.Charged)
            return;
        OrbVisual targetVisual = GetCurrentOrbVisual();
        if (chargeAnimator == null)
        {
            Debug.LogError("Charge Animator not assigned in BossManager!");
            currentOrb.AddCharge(matchingElements.Count);
            return;
        }

        chargeAnimator.AnimateChargeToOrb(elementStartPositions, matchingElements, targetVisual, 
            onEachElementArrival: (elementIndex) =>
            {
                currentOrb.AddCharge(1);
            },
            onComplete: null
        );
    }

    public OrbVisual GetCurrentOrbVisual()
    {
        if (orbVisuals == null || currentOrbIndex >= orbVisuals.Length)
            return null;

        return orbVisuals[currentOrbIndex];
    }

    private void OnOrbChargeChanged(int orbIndex, int previousCharge, int currentCharge)
    {
        if (orbVisuals != null && orbIndex < orbVisuals.Length && orbVisuals[orbIndex] != null)
        {
            orbVisuals[orbIndex].UpdateCharge(currentCharge, Orb.MaxCharge);
        }
    }

    private void OnOrbStateChanged(int orbIndex, OrbState state)
    {
        if (orbVisuals != null && orbIndex < orbVisuals.Length && orbVisuals[orbIndex] != null)
        {
            orbVisuals[orbIndex].SetState(state);
        }
    }
}
