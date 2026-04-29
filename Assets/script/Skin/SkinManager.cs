using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [SerializeField]
    private SkinDB skinDB;


    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }


        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public Skin GetSkin(SkinType skinType)
    {
        Debug.Log($"Attempting to retrieve skin of type: {skinType}");
        if (skinDB == null || !skinDB.GetSkin(skinType))
        {
            Debug.LogWarning($"Skin of type {skinType} not found in SkinDB.");
            return null;
        }
        return skinDB.GetSkin(skinType);
    }

    public List<Skin> GetAllSkins()
    {
        if (skinDB == null)
        {
            Debug.LogWarning("SkinDB is not assigned in SkinManager.");
            return null;
        }
        return skinDB.Skins;
    }
}
