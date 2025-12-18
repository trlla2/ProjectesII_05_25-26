using UnityEngine;

public class TrailController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Grid Grid;

    Vector2 clickPosition;

    bool following = false;
    bool lastFollowing = false;
    bool needsClear = false;

    void Update()
    {
        following = Input.GetKey(KeyCode.Mouse0);

        if (following && !lastFollowing)
        {
            needsClear = true; 
        }

        trail.forceRenderingOff = Grid.CurrentState != GridStateType.Idle;
    }

    void LateUpdate()
    {
        if (needsClear)
        {
            clickPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = clickPosition;
            trail.Clear();
            needsClear = false;
        }

        if (following)
        {
            clickPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = clickPosition;
        }

        lastFollowing = following;
    }
}
