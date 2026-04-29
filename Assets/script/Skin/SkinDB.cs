using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "NewSkinDB", menuName = "Game/SkinDB")]
public class SkinDB : ScriptableObject
{
    [SerializeField] private List<Skin> skins = new List<Skin>();

    public List<Skin> Skins => skins;

    public Skin GetSkin(SkinType type)
    {
        foreach (var item in skins)
        {
            if (item.SkinType == type)
            {
                return item;
            }
        }

        return null;
    }
}
