using UnityEngine;
public interface IInputHandler
{
    bool IsPathingInputDown();
    bool IsPathingInputHeld();
    bool IsPathingInputUp();
    ElementVisual GetElementVisualFromInput(Camera camera);
}

public class MouseInputHandler : IInputHandler
{
    public bool IsPathingInputDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    public bool IsPathingInputHeld()
    {
        return Input.GetMouseButton(0);
    }

    public bool IsPathingInputUp()
    {
        return Input.GetMouseButtonUp(0);
    }

    public ElementVisual GetElementVisualFromInput(Camera camera)
    {
        if (camera == null)
            return null;

        Vector3 worldPoint = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hit = Physics2D.OverlapPoint(point2D);

        if (hit == null)
            return null;

        ElementVisual visual = hit.GetComponent<ElementVisual>();

        return visual;
    }
}