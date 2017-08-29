using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Guis
{
    /// <summary>
    /// Updates a rect?
    /// </summary>
    public class RectUpdater : MonoBehaviour
    {
        public Vector2 MySize;
        public Vector2 MyRatio;
        public float MyDistance = 2;
        public float Buffer = 0.1f;
        private Vector2 MyResolution;
        public static RectTransform MyRect;

        void Start()
        {
            MyResolution = new Vector2(Screen.width, Screen.height);
            //UpdateRect();
        }

        void Update()
        {
            if (HasResolutionChanged())
            {
                //UpdateRect();
            }
        }

        public static Vector2 MousePositionToScaledScreenPosition(Vector2 MyMousePosition)
        {
            MyMousePosition.x /= Screen.width;
            MyMousePosition.y /= Screen.height;
            MyMousePosition.x -= 0.5f;
            MyMousePosition.y -= 0.5f;
            MyMousePosition.x *= 1920;
            MyMousePosition.y *= 1080;
            return MyMousePosition;
        }

        public static void GetRatio(Transform MyTransform)    //Transform MyTransform, 
        {
           /* MyRect = GetRect();
            float MyDistance = Vector3.Distance(Camera.main.transform.position, MyTransform.position);
            Vector2 MySize = GetRectSize(MyDistance);    // get camera ratio at distance
            MySize.x /= MyTransform.lossyScale.x;
            MySize.y /= MyTransform.lossyScale.y;
            return new Vector2(MySize.x / Screen.width, MySize.y / Screen.height);*/
        }
        public static RectTransform GetRect()
        {
            if (MyRect == null)
            {
                MyRect = GameObject.Find("MainMenuBackground").GetComponent<RectTransform>();
                return MyRect;
            }
            return MyRect;
        }
        public static Vector2 GetResolutionMultiplier(Vector2 MyResolution)
        {
            return MyResolution;
        }
        public static Vector2 GetScaledResolution()
        {
            return GetResolutionMultiplier(new Vector2(Screen.width, Screen.height));
        }

        /// <summary>
        /// Returns true if the resolution has changed.
        /// </summary>
        private bool HasResolutionChanged()
        {
            Vector2 NewResolution = new Vector2(Screen.width, Screen.height);
            if (MyResolution != NewResolution)
            {
                MyResolution = NewResolution;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Updates the rect
        /// </summary>
        /*void UpdateRect()
        {
            RectTransform MyRect = gameObject.GetComponent<RectTransform>();
            MySize = GetRectSize(MyDistance);
            MySize.x /= transform.lossyScale.x;
            MySize.y /= transform.lossyScale.y;
            MyRatio = new Vector2(MySize.x / 1920f, MySize.y / 1080f);
            MyRect.SetSize(MySize);
            RectTransform MyBabyRect = gameObject.transform.GetChild(0).GetComponent<RectTransform>();
            MyBabyRect.SetSize(MySize);
            MyBabyRect.position = MyRect.position;
            MyBabyRect.anchoredPosition = new Vector3();
        }*/
    }
}
/*void OnDrawGizmos()
{
    if (!DebugMode)
        return;
    Vector3 CubeSize = new Vector3(0.05f, 0.05f, 0.05f);
    Vector3 LowerBound = Camera.main.ViewportToWorldPoint(new Vector3(Buffer, Buffer, MyDistance));
    Vector3 LowerBound2 = Camera.main.ViewportToWorldPoint(new Vector3(Buffer, 1- Buffer, MyDistance));
    Vector3 UpperBound = Camera.main.ViewportToWorldPoint(new Vector3(1- Buffer, 1- Buffer, MyDistance));
    Vector3 UpperBound2 = Camera.main.ViewportToWorldPoint(new Vector3(1- Buffer, Buffer, MyDistance));
    //Camera.main.ViewportToScreenPoint(); ViewportToWorldPoint
    Gizmos.DrawCube(LowerBound, CubeSize);
    Gizmos.DrawCube(LowerBound2, CubeSize);
    Gizmos.DrawCube(UpperBound, CubeSize);
    Gizmos.DrawCube(UpperBound2, CubeSize);
}*/
