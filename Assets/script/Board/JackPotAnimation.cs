using System.Collections;
using TMPro;
using UnityEngine;

public class JackPotAnimation : MonoBehaviour
{
    public TMP_Text jackpotText;
    public int replays;
    public Color DeActivatedColor;
    public Color ActivatedColor;

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
            yield return new WaitForSeconds(0.5f);
            jackpotText.color = ActivatedColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UnShowText()
    {
        jackpotText.color = DeActivatedColor;
    }
}
