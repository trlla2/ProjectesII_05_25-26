using UnityEngine;

public class Highlighter : MonoBehaviour
{
    [SerializeField]
    private GameObject Highligth;

    private void OnEnable() => ResetHighlight();
    public void Highlight() => Highligth.SetActive(true);
    public void ResetHighlight() => Highligth.SetActive(false);
}