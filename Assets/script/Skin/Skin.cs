using UnityEngine;

public enum SkinType { Default, Student, Necromancer }

[CreateAssetMenu(fileName = "NewSkin ", menuName = "Game/Skin")]
public class Skin : ScriptableObject
{
    [Header("Data")]
    [SerializeField]
    private int cost;
    [SerializeField]
    private string skinName;
    [SerializeField]
    private SkinType skinType;


    [Header("Animations")]
    [SerializeField]
    private Sprite skinSprite;
    [SerializeField]
    private RuntimeAnimatorController animatorController;
   
    public string SkinName => skinName;
    public int Cost => cost;
    public SkinType SkinType => skinType; 
    public Sprite SkinSprite => skinSprite;
    public RuntimeAnimatorController AnimatorController => animatorController;
    
}
