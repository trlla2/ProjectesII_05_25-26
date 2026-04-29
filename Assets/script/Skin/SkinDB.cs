using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "NewSkinDB", menuName = "Game/SkinDB")]
public class SkinDB : ScriptableObject
{
    [SerializeField] private List<SkinType> type = new List<SkinType>();
    [SerializeField] private List<Skin> skin = new List<Skin>();
    
    public Dictionary<SkinType, Skin> Skins;

    public void OnBeforeSerialize()
    {
        type.Clear();
        skin.Clear();

        foreach (var kvp in Skins)
        {
            type.Add(kvp.Key);
            skin.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Skins = new Dictionary<SkinType, Skin>();

        for (int i = 0; i != Math.Min(type.Count, skin.Count); i++)
        {
            Skins.Add(type[i], skin[i]);
        }
    }
}
