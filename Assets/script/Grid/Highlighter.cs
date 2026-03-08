using UnityEngine;

public class Highlighter : MonoBehaviour
{
    [SerializeField]
    private GameObject Highligth;

    private void OnEnable() => SetHighlight(false);
    public void SetHighlight(bool high) => Highligth.SetActive(high);
}