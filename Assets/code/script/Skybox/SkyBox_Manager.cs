using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyBox_Manager : MonoBehaviour
{
    public static SkyBox_Manager Instance;

    [Header("Configuración")]
    [SerializeField]
    private float rotationSpeed = 1.0f;

    private Material activeSkyboxMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 

            SetupSkyboxMaterial();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void SetupSkyboxMaterial()
    {
        // SETUP A COPY OF THE SKYBOX MATERIAL TO NOT MODIFY THE ACTUAL MATERIAL
        if (RenderSettings.skybox != null)
        {
            activeSkyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = activeSkyboxMaterial;
        }
    }

    void Update()
    {
        if (activeSkyboxMaterial != null)
        {
            float currentRotation = activeSkyboxMaterial.GetFloat("_Rotation");
            float newRotation = currentRotation + (rotationSpeed * Time.deltaTime);

            activeSkyboxMaterial.SetFloat("_Rotation", newRotation % 360);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance == this && activeSkyboxMaterial != null)
        {
            RenderSettings.skybox = activeSkyboxMaterial;
        }
    }
}
