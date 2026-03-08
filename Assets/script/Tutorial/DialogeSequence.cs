using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogeSequence : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField]
    private TranslatorUI parentTranslator;

    [Header("Text")]
    [SerializeField]
    private TMP_Text textMesh;

    [Header("Message Sequence")]
    [SerializeField]
    private List<string> messeges = new List<string>();
    private int indexMessage = 0;

    public UnityEvent OnMessageChange;

    public void StartConversation()
    {
        this.gameObject.SetActive(true);
        parentTranslator.ToTarget();

        NextMessage();
    }


    public void NextMessage()
    {
        if (messeges.Count <= indexMessage)
        {
            CloseConversation();
            return;
        }

        textMesh.text = messeges[indexMessage];

        indexMessage++;

        OnMessageChange.Invoke();   

    }

    private void CloseConversation()
    {
        parentTranslator.ToOrigin();
    }
}
