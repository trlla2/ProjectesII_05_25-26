using System.Collections.Generic;
using UnityEngine;

public class BeamHandler : MonoBehaviour
{
    [SerializeField]
    private List<Translator> beamTranslator = new List<Translator>();

    private Translator parentTranslator = null;

    private void Awake() { parentTranslator = GetComponent<Translator>(); }

    private void OnEnable()
    {
        if (parentTranslator == null)
            parentTranslator = GetComponent<Translator>();

        parentTranslator.ToTarget();

    }

    private void OnDisable()
    {
        if (parentTranslator != null)
            parentTranslator.ResetTranslator();

        foreach (Translator translator in beamTranslator)
        {
            translator.ResetTranslator();
        }
    }
}
