using UnityEngine;
using UnityEngine.Events;

namespace Zeltex.Guis
{

    [System.Serializable]
    public class EventVector2 : UnityEvent<Vector2>
    {

    }

    /// <summary>
    /// Managers the screen size change
    /// </summary>
    public class ScreenSizeManager : MonoBehaviour
    {
        private static ScreenSizeManager instance;
        [HideInInspector]
        public EventVector2 ScreenSizeChangeEvent;
        private Vector2 ScreenSize;
        private Vector2 NewScreenSize;
        
        // Use this for initialization
        void Awake()
        {
            instance = this;
        }

        public static ScreenSizeManager Get()
        {
            return instance;
        }

        // Update is called once per frame
        void Update()
        {
            NewScreenSize = new Vector2(Screen.width, Screen.height);
            if (ScreenSize != NewScreenSize)
            {
                ScreenSize = NewScreenSize;
                ScreenSizeChangeEvent.Invoke(ScreenSize);
            }
        }

        public Vector2 GetCurrentScreenSize()
        {
            return new Vector2(Screen.width, Screen.height);
        }



        #region OrbitorStatics

        /// <summary>
        /// Converts a 1920x1080 position to a scaled anchor position
        /// </summary>
        public static Vector2 BaseToScaledPosition(Vector2 GuiPosition, Vector2 MyScale, float MyDistance)
        {
            //Debug.LogError("    x1 Converting gui position: " + GuiPosition.ToString());
            //Vector2 ScreenSize = ScreenSizeManager.Get().ScreenSize;//new Vector2(Screen.width, Screen.height);  // orinally 1920 x 1080
            Vector2 MySize = GetRectSize(MyDistance);   //
            MySize = new Vector2(MySize.x / MyScale.x, MySize.y / MyScale.y);   // this is scaled resolution
            //Debug.LogError("    x2 MySize: " + MySize.ToString());
            GuiPosition = new Vector2(GuiPosition.x / 1920f, GuiPosition.y / 1080f);    // Make THE position between 0 and 1
            GuiPosition = new Vector2(GuiPosition.x * MySize.x, GuiPosition.y * MySize.y);
            //Debug.LogError("    x3 Converting gui position: " + GuiPosition.ToString());
            return GuiPosition;
        }

        /// <summary>
        /// Gets the size of the rect at a distance from the camera
        /// </summary>
        public static Vector2 GetRectSize(float MyDistance)    // at scale 1
        {
            Vector2 RectSize = new Vector2();
            if (CameraManager.Get())
            {
                Camera RectCamera = CameraManager.Get().GetMainCamera();
                if (RectCamera)
                {
                    //return RectCamera.pixelRect.size;
                    Quaternion TempQuat = RectCamera.transform.rotation;
                    RectCamera.transform.rotation = Quaternion.identity;
                    Vector3 LowerBounds = RectCamera.ViewportToWorldPoint(new Vector3(0, 0, MyDistance));
                    Vector3 UpperBounds = RectCamera.ViewportToWorldPoint(new Vector3(1, 1, MyDistance));
                    RectSize = UpperBounds - LowerBounds;
                    RectCamera.transform.rotation = TempQuat;
                }
            }
            //Debug.LogError("Getting new RectSize in screen: " + RectSize.ToString());
            return RectSize;
        }

        /// <summary>
        /// Converts a 1920x1080 position to a scaled anchor position
        /// </summary>
        /*public static Vector2 ScaledToBasePosition(Vector2 GuiPosition, Vector2 MyScale, float MyDistance)
        {
            Vector2 MySize = GetRectSize(MyDistance);
            MySize = new Vector2(MySize.x / MyScale.x, MySize.y / MyScale.y);   // this is scaled resolution
            GuiPosition = new Vector2(GuiPosition.x / MySize.x, GuiPosition.y / MySize.y);
            GuiPosition = new Vector2(GuiPosition.x * 1920f, GuiPosition.y * 1080f);    // Make THE position between 0 and 1
            return GuiPosition;
        }*/
        #endregion
    }

}