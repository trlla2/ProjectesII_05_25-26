using UnityEngine;

[CreateAssetMenu(fileName = "NewItemType", menuName = "Game/Item Type")]
public class ItemsType : ScriptableObject
{
    [Header("SETUP")]
    public string nameItem;
    public string description;


    [Header("Art Stuff")]
    public Sprite enemySprite;

    [Header("FORCED MUST CHANGE !!!!!!")]
    public float evasionBonus;


}
