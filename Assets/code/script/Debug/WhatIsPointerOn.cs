using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WhatIsPointerOn : MonoBehaviour
{
    void Update()
    {
        // 1. Comprobar si el nuevo Input System está detectando el ratón
        if (Mouse.current == null)
        {
            Debug.LogWarning("El ratón no se detecta. Revisa la configuración del Input System.");
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 2. Comprobar si existe un Event System en la escena
            if (EventSystem.current == null)
            {
                Debug.LogError("¡FALTA UN EVENT SYSTEM! Ve a GameObject > UI > Event System para crear uno.");
                return;
            }

            List<RaycastResult> results = RaycastMouse();

            if (results.Count == 0)
            {
                Debug.Log("Has hecho clic, pero el rayo no ha tocado ninguna UI. (¿Falta el Graphic Raycaster en el Canvas o el Raycast Target en las imágenes?)");
            }

            foreach (var hit in results)
            {
                Debug.Log("Detectado: " + hit.gameObject.name + " en capa: " + hit.gameObject.layer);
            }
        }
    }

    private List<RaycastResult> RaycastMouse()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results;
    }
}