using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class JackPotAnimation : MonoBehaviour
{
    public TMP_Text jackpotText;
    public int replays;
    public Color DeActivatedColor;
    public Color ActivatedColor;
    public float time = 0.2f;

    public void OnEnable()
    {
        jackpotText.color = DeActivatedColor;
    }

    public void StartAnimation()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < replays; i++)
        {
            jackpotText.color = DeActivatedColor;
            yield return new WaitForSeconds(time);
            jackpotText.color = ActivatedColor;
            yield return new WaitForSeconds(time);
        }
    }

    public void UnShowText()
    {
        jackpotText.color = DeActivatedColor;
    }
}
