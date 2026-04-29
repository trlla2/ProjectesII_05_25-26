using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "NewSkinDB", menuName = "Game/SkinDB")]
public class SkinDB : ScriptableObject
{
    [SerializeField] private List<Skin> skin = new List<Skin>();

    public List<Skin> Skins = new List<Skin>();

    public Skin GetSkin(SkinType type)
    {
        foreach (var item in skin)
        {
            if (item.SkinType == type)
            {
                return item;
            }
        }

        return null;
    }
}
