using UnityEngine;

public class BoardCameraController : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] Vector3 camOffset;


    void Update()
    {
        this.gameObject.transform.position = new Vector3(_player.transform.position.x+ camOffset.x, transform.position.y, _player.transform.position.z+camOffset.z);
    }

}
