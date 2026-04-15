using DG.Tweening;
using UnityEngine;

public class BoardCameraController : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private Vector3 camOffset;

    [Header("Zoom Settings")]
    [SerializeField] private float _defaultOrthographicSize = 5f;
    [SerializeField] private float _branchChoiceOrthographicSize = 8f;
    [SerializeField] private float _zoomDuration = 0.5f;
    [SerializeField] private Ease _zoomEase = Ease.InOutSine;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        transform.position = new Vector3(
            _player.transform.position.x + camOffset.x,
            transform.position.y,
            _player.transform.position.z + camOffset.z
        );
    }

    public void ZoomIn()
    {
        _camera.DOOrthoSize(_branchChoiceOrthographicSize, _zoomDuration).SetEase(_zoomEase);
    }

    public void ZoomOut()
    {
        _camera.DOOrthoSize(_defaultOrthographicSize, _zoomDuration).SetEase(_zoomEase);
    }
}
