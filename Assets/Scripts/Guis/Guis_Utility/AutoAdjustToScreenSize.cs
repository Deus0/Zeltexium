using UnityEngine;
using UnityEngine.Events;

namespace Zeltex.Guis
{
    /// <summary>
    /// Auto adjusts the connect rect transform to the screen size
    /// </summary>
    public class AutoAdjustToScreenSize : MonoBehaviour
    {
        private RectTransform MyRect;
        private UnityAction<Vector2> ScreenSizeChangeAction;

        // Use this for initialization
        void Awake()
        {
            MyRect = GetComponent<RectTransform>();
            MyRect.SetSize(ScreenSizeManager.Get().GetCurrentScreenSize());
            ScreenSizeChangeAction = OnScreenSizeChange;
            ScreenSizeManager.Get().ScreenSizeChangeEvent.AddEvent(ScreenSizeChangeAction);
        }

        private void OnScreenSizeChange(Vector2 ScreenSize)
        {
            MyRect.SetSize(ScreenSize);
        }
    }

}