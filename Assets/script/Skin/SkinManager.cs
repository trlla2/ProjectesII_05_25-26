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
        if (skinDB == null || !skinDB.Skins.ContainsKey(skinType))
        {
            Debug.LogWarning($"Skin of type {skinType} not found in SkinDB.");
            return null;
        }
        return skinDB.Skins[skinType];
    }
}
