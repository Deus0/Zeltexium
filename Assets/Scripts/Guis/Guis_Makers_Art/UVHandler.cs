using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using Zeltex.Util;

namespace Zeltex.Guis
{
    /// <summary>
    /// Attached to the UV handlers
    /// Bug: when i select the UV handler, it goes to 0,0 and alters the UVs on the mesh.
    /// </summary>
    public class UVHandler : MonoBehaviour,
                                    IDragHandler,
                                    IPointerDownHandler,
                                    IPointerUpHandler
    {
        public UVViewer MyViewer;
        private Color32 NormalColor = Color.white;
        private Color32 SelectedColor = Color.red;
        private Vector2 InitialMousePosition;
        private Vector2 OriginalAnchoredPosition;
        private bool IsDragging = false;

        void Start()
        {
            Deselect();
        }

        public void Deselect()
        {
            GetComponent<RawImage>().color = NormalColor;
        }
        /// <summary>
        /// When initially clicked the uv position
        /// </summary>
        public void OnPointerDown(PointerEventData MyEventData)
        {
            MyViewer.Select(gameObject);
            GetComponent<RawImage>().color = SelectedColor;
            OriginalAnchoredPosition = GetComponent<RectTransform>().anchoredPosition;
            bool IsInsideRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                MyViewer.GetComponent<RectTransform>(),
                MyEventData.position,
                Camera.main,
                out InitialMousePosition);
            IsDragging = true;
        }

        /// <summary>
        /// Move the UV on drag
        /// </summary>
        public void OnDrag(PointerEventData MyEventData)
        {
            if (IsDragging)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    MyViewer.GetComponent<RectTransform>(),
                    MyEventData.position,
                    Camera.main))
                {
                    Vector2 NewPosition;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        MyViewer.GetComponent<RectTransform>(),
                        MyEventData.position,
                        Camera.main,
                        out NewPosition))
                    {
                        Vector2 DifferenceMousePosition = NewPosition - InitialMousePosition;
                        //DifferenceMousePosition = ApplyGridPosition(DifferenceMousePosition);
                        if (DifferenceMousePosition != Vector2.zero)
                        {
                            Vector3 NewPosition2 = new Vector3(ApplyGridPosition(OriginalAnchoredPosition + DifferenceMousePosition).x,
                                ApplyGridPosition(OriginalAnchoredPosition + DifferenceMousePosition).y, 
                                0);
                            GetComponent<RectTransform>().anchoredPosition3D = NewPosition2;
                        }
                        MyViewer.OnDrag(gameObject);
                        // Apply grid to position
                    }
                }
                else
                {
                    //IsDragging = false;
                }
            }
        }

        public Vector2 ApplyGridPosition(Vector2 MyPosition)
        {
            if (MyViewer.IsGridPosition)
            {
                //Vector2 MyPosition = GetComponent<RectTransform>().anchoredPosition;
                //MyPosition.x = (Mathf.RoundToInt(MyPosition.x * MyViewer.GridLength)) / MyViewer.GridLength; 
                //MyPosition.x = Mathf.Clamp(MyPosition.x, 0, MyViewer.GetComponent<RectTransform>().GetWidth());
                //MyPosition.y = (Mathf.RoundToInt(MyPosition.y * MyViewer.GridLength)) / MyViewer.GridLength;
                //MyPosition.y = Mathf.Clamp(MyPosition.y, 0, MyViewer.GetComponent<RectTransform>().GetHeight());
                //GetComponent<RectTransform>().anchoredPosition = MyPosition;
            }
            return MyPosition;
        }
        /// <summary>
        /// When dragging finishes, update the models UVs
        /// </summary>
        public void OnPointerUp(PointerEventData MyEventData)
        {
            IsDragging = false;
            MyViewer.Release(gameObject);
        }

        public void SetColors(Color32 NormalColor_, Color32 SelectedColor_)
        {
            NormalColor = NormalColor_;
            SelectedColor = SelectedColor_;
            GetComponent<RawImage>().color = NormalColor;
        }
    }
}