using UnityEngine;

public class BoardCameraController : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] Vector3 camOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = new Vector3(_player.transform.position.x+ camOffset.x, _player.transform.position.y+camOffset.y, _player.transform.position.z+camOffset.z);
    }

}
