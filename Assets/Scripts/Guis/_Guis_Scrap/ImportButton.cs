using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GuiSystem
{
    /// <summary>
    /// Quick Overlay over the ui, to invoke the event on mouse down
    /// </summary>
    public class ImportButton : MonoBehaviour, IPointerDownHandler
    {
        public UnityEvent OnMouseDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            //Debug.LogError("Mouse down on button.");
            OnMouseDown.Invoke();
        }
    }
}