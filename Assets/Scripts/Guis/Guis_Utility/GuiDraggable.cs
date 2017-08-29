using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Zeltex.Guis
{
    /// <summary>
    /// Makes the gui window draggable.
    /// Added onto headers in the gui.
    /// </summary>
    public class GuiDraggable : 	MonoBehaviour, 
									IPointerDownHandler, 
									IDragHandler
    {
        //[Tooltip("Uses orbitor script on the window to reposition.")]
        [HideInInspector]
        public RectTransform WindowRect;
        [HideInInspector]
        public RectTransform MyRect;
        [HideInInspector]
        public UnityEvent OnSelect;
        private Orbitor MyOrbitor;
        private Vector2 InitialPointerPosition;
		private Vector2 NewPointerPosition;
		private Vector2 OriginalMousePosition;
		private Vector2 LastScreenPosition;
        private Vector2 OriginalOffset;
        [SerializeField]
        private bool IsNode;

        void Start()
        {
            if (IsNode == false)
            {
                MyOrbitor = FindRootOrbitor(transform);
                WindowRect = MyOrbitor.GetComponent<RectTransform>();
            }
            else
            {
                WindowRect = transform.parent as RectTransform;
            }
            /*if (RootOrbitor)
            {
                WindowRect = RootOrbitor.gameObject.GetComponent<RectTransform>();
            }
            if (WindowRect == null)
            {
                if (transform.parent != null)
                {
                    WindowRect = transform.parent as RectTransform;
                }
                else
                {
                    MyRect = GetComponent<RectTransform>();
                    if (MyRect == null)
                    {
                        Debug.LogError(name + " has a GuiDraggable script but no rect transform..");
                    }
                }
            }
            if (WindowRect)
            {
                MyOrbitor = WindowRect.GetComponent<Orbitor>();
            }*/
        }

        Orbitor FindRootOrbitor(Transform MyTransform)
        {
            //for (int i = 0; i < MyTransform.childCount; i++)
            if (MyTransform != null)
            {
                //Transform Child = MyTransform.GetChild(i);
                Transform Parent = MyTransform.parent;
                if (Parent != null)
                {
                    Orbitor MyOrbitor = Parent.gameObject.GetComponent<Orbitor>();
                    if (MyOrbitor)
                    {
                        return MyOrbitor;
                    }
                    else
                    {
                        Orbitor MyOrbitor2 = FindRootOrbitor(Parent);
                        if (MyOrbitor2)
                        {
                            return MyOrbitor2;
                        }
                    }
                }
            }
            return null;
        }

        public void OnPointerDown(PointerEventData data)
        {
            // Mouse position relative to rect
            if (MyOrbitor == null && WindowRect == MyRect)
            {
                WindowRect = transform.parent.parent.gameObject.GetComponent<RectTransform>();
            }
            if (MyOrbitor)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(WindowRect, data.position, data.pressEventCamera, out OriginalOffset);
                OriginalOffset.x *= WindowRect.lossyScale.x;
                OriginalOffset.y *= WindowRect.lossyScale.y;
                OriginalOffset.x *= 1080;// 1920 / 2f;//-;
                OriginalOffset.y *= 1080;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(MyRect, data.position, data.pressEventCamera, out OriginalOffset);
            }
            OnSelect.Invoke();
            WindowRect.transform.SetAsLastSibling();
            // for all guis, change sort order
            if (WindowRect.transform.parent)
            {
                for (int i = 0; i < WindowRect.transform.parent.childCount; i++)
                {
                    Canvas MyCanvas = WindowRect.transform.parent.GetChild(i).gameObject.GetComponent<Canvas>();
                    if (MyCanvas && GuiSpawner.IsUseSortOrders)
                    {
                        MyCanvas.sortingOrder = i;
                    }
                }
            }
        }
        /// <summary>
        /// When dragging, set the new screen position of the orbitor to the difference in mouse positions plus the original position
        /// </summary>
        /// <param name="data"></param>
		public void OnDrag(PointerEventData data) 
		{
            if (MyOrbitor)
            {// RectUpdater.BaseToScaledPosition(NewPointerPosition, WindowRect.transform.lossyScale, MyOrbitor.MyDisplayDistance); //NewPointerPosition;
                Vector2 NewScreenPosition = RectUpdater.MousePositionToScaledScreenPosition(data.position) - OriginalOffset;
                MyOrbitor.SetScreenPosition(NewScreenPosition);
            }
            else if (MyRect != null)
            {
                Vector2 NewScreenPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(WindowRect, data.position, data.pressEventCamera, out NewScreenPosition);
                MyRect.localPosition = NewScreenPosition - OriginalOffset;  // minus the local position ! so it moves from the point! :D
            }
            else
            {
                Debug.LogError("Null problems in " + name);
            }
        }
    }
}
