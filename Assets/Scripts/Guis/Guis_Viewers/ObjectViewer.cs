using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zeltex.AnimationUtilities;
using Zeltex.Util;

/// <summary>
/// The backend classes used behind some of the more advanced UI elements
/// </summary>
namespace Zeltex.Guis
{
    /// <summary>
    /// This class contains
    ///      - the Spawning of GameObject
    ///      - Spawning of rendertexture attached to camera
    ///      - the Spawning of Camera pointing at it
    /// Seperate Object Functions:
    ///      - Selection of object
    ///      - Moving of Object
    ///      - Rotating of object
    /// Seperate Camera functions
    ///     - The camera script - seperate from viewer! Use Prefabbed camera!
    ///      - Zooming of camera on object
    ///      - Panning of camera
    /// </summary>
    public class ObjectViewer : MonoBehaviour
    //,IDragHandler
    //,IPointerDownHandler
    //,IPointerUpHandler
    {
        #region Variables
        // static
        public static int TotalViewerIndexes = 0;
        private static Transform ParentsTransform;
        private static Transform CameraParentsTransform;
        [Header("References")]
        public GameObject SpawnPrefab;
        private RenderTexture MyRenderTexture;     // this should be spawned and temporary!
        private GameObject CameraPrefab;
        [Header("Options")]
        public Color32 BackgroundClearColor = Color.black;
        [Header("Lighting")]
        public Vector3 LightPosition = new Vector3();
        public float LightIntensity = 1;
        public Color32 LightColor = Color.white;
        public LightType MyLightType = LightType.Spot;
        // Spawning
        private GameObject MySpawn;
        [SerializeField]
        protected Camera MyCamera;
        protected GameObject MyLight;
        private int ViewerIndex = -1;
        // Raycasting
        protected Vector2 PreviousMousePosition;
        protected Vector2 DragPositionBegin;
        protected RaycastHit MyHit;

        //protected bool CanClick;
        //protected bool CanRotateViewer;
        //protected bool CanSpray;           // can the user spray click on selecting
        //private bool IsAltRotate;
        //private bool IsInitialClickObject = false;
        //protected bool IsRightClick;
       // protected bool IsDragging = false;
       // private bool IsBeginSpray;
       // private bool IsSpraying;
        //protected bool IsUseWorldAxis = false;
        // Interaction
        //private bool IsMovingObject;    // if true, the object follows the mouse in update
       // protected GameObject SelectedObject;    // gets set to null when clicking on nothing
        //private bool IsGridSnap; // uses setter
        // Auto Rotation
        //private float GridSnapLength = 10;
        private bool IsMoveObjects ;

        // Camera
        private float ZoomPosition = 1.2f;
        private float RotationSpeed = 1f;
        //private float CameraLerpSpeed = 3f;
        //private float ZoomSpeed = 0.25f;
        private float CameraClipValue = 0.01f;
        //private Quaternion TargetRotation = Quaternion.identity;
        protected bool IsAutoRotate = false;
        protected Vector2 AutoRotateInput = new Vector2(1, 0);

        private float CameraPanSpeed = 4;
        protected Vector2 PanPosition = new Vector2(0, 0);
        private Material MyEdgeMaterial;
        private Shader MyEdgeShader;
        private Vector2 RenderTextureSize = new Vector2(1024, 1024);
        private float FieldOfView = 90;
        #endregion

        #region Mono

        public virtual void Update()
        {
            UpdateCamera();
        }
        #endregion

        #region Input

        /// <summary>
        /// Handles pointer down event
        /// </summary>
        //public void OnPointerDown(PointerEventData MyEventData)
        //{
            /*if (CanClick)
            {
                IsRightClick = (MyEventData.button != PointerEventData.InputButton.Left);
                OnTheClick(MyEventData.position, true);
            }*/
        //}
        /// <summary>
        /// Handles pointer up event
        /// </summary>
        //public void OnPointerUp(PointerEventData MyEventData)
        //{
            //Debug.LogError("OnPointerUp: " + MyEventData.position.ToString() + " at " + Time.time);
            //IsDragging = false;
        //}
        /// <summary>
        /// Handles drag event
        /// </summary>
        //public void OnDrag(PointerEventData MyEventData)
       // {
            //Debug.LogError("OnDrag: " + MyEventData.position.ToString() + " at " + Time.time);
            //OnTheDrag(MyEventData.position, MyEventData.button);
       // }
        /// <summary>
        /// A virtual function for handling drag
        /// </summary>
        //protected virtual void OnDragged(Vector3 MousePosition, Vector3 MousePositionDifference)
       // {
            //ObjectFollowMouse(MousePosition);
       // }
        #endregion

        #region ZelGuiEvents
        /// <summary>
        /// Called when viewer is opened
        /// </summary>
        public virtual void OnBegin()
        {
            Spawn();
            AttachCamera();
            if (MySpawn == null)
            {
                Debug.LogError("VoxelMesh is null in OnBegin.");
            }
        }
        /// <summary>
        /// On End - Called when ZelGui switched off
        /// </summary>
        public virtual void OnEnd()
        {
            Clear();
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Clear the spawned stuff!
        /// </summary>
        public void Clear()
        {
            if (MySpawn != null)
            {
                MySpawn.Die();
            }
            ClearOthers();
        }
        /// <summary>
        /// Clears the camera and light
        /// </summary>
        protected void ClearOthers()
        {
            if (MyCamera != null)
            {
                MyCamera.gameObject.Die();
            }
            if (MyLight != null)
            {
                MyLight.Die();
            }
        }

        /// <summary>
        /// uses indexing to generate a uniqe position for the viewer object
        /// </summary>
        protected Vector3 GetSpawnPosition()
        {
            if (ViewerIndex == -1)
            {
                ViewerIndex = TotalViewerIndexes;
                TotalViewerIndexes++;
            }
            return new Vector3(ViewerIndex * 100, -100, 0);
        }

        /// <summary>
        /// Spawn an object to be viewed!
        /// </summary>
        protected void Spawn()
        {
            // if has not spawned
            if (MySpawn == null)
            {
                MySpawn = (GameObject)Instantiate(
                    SpawnPrefab,
                    GetSpawnPosition(),
                    Quaternion.identity);
                //MySpawn.layer = 1 << LayerManager.Get().ViewerLayer;
                if (ParentsTransform == null)
                {
                    GameObject ViewerParents = GameObject.Find("Viewers");
                    if (ViewerParents == null)
                    {
                        ViewerParents = new GameObject();
                        ViewerParents.name = "Viewers";
                    }
                    if (ViewerParents)
                    {
                        ParentsTransform = ViewerParents.transform;
                    }
                }
                if (MySpawn)
                {
                    MySpawn.transform.SetParent(ParentsTransform);
                    MySpawn.name = gameObject.name + " Viewer";
                    //TargetRotation = Quaternion.identity;
                }
                else
                {
                    Debug.LogError("Instantiation Failed. Inside " + name + ".");
                }
            }
        }

        /// <summary>
        /// Resizes the render teture
        /// </summary>
        public void ResizeRenderTexture(Vector2 NewSize)
        {
            if (RenderTextureSize != NewSize || MyRenderTexture == null)
            {
                RenderTextureSize = NewSize;
                if (MyRenderTexture != null)
                {
                    MyRenderTexture.Die();
                }
                MyRenderTexture = new RenderTexture(
                    Mathf.RoundToInt(RenderTextureSize.x),
                    Mathf.RoundToInt(RenderTextureSize.y),
                    24,
                    RenderTextureFormat.ARGB32);
                MyRenderTexture.name = name + "_RenderTexture";
                if (MyCamera)
                {
                    MyCamera.targetTexture = MyRenderTexture;
                }
                RawImage MyRawImage = gameObject.GetComponent<RawImage>();
                if (MyRawImage)
                {
                    MyRawImage.texture = MyRenderTexture;
                }
            }
        }

        /// <summary>
        /// attach a camera with a render texture for our viewer gui!
        /// </summary>
        protected void AttachCamera()
        {
            if (MySpawn != null)
            {
                if (CameraParentsTransform == null && CameraManager.Get())
                {
                    CameraParentsTransform = CameraManager.Get().transform;
                }
                // spawn camera
                if (MyCamera == null)
                {
                    GameObject CameraObject = new GameObject();
                    CameraObject.name = name + "_ViewerCamera";
                    CameraObject.transform.SetParent(CameraParentsTransform);
                    PanPosition = new Vector2(CameraObject.transform.position.x, CameraObject.transform.position.y);
                    CameraObject.transform.position = MySpawn.transform.TransformPoint(new Vector3(0, 0, ZoomPosition * MySpawn.transform.localScale.z));
                    CameraObject.transform.LookAt(MySpawn.transform.position);
                    MyCamera = CameraObject.AddComponent<Camera>();
                    MyCamera.fieldOfView = FieldOfView;
                    MyCamera.nearClipPlane = CameraClipValue;
                    MyCamera.clearFlags = CameraClearFlags.SolidColor;
                    MyCamera.backgroundColor = BackgroundClearColor;
                    MyCamera.cullingMask = LayerManager.Get().ViewerLayer;
                    MyCamera.nearClipPlane = 0.01f;
                    MyCamera.targetTexture = MyRenderTexture;
                    // Render Texture
                    ResizeRenderTexture(RenderTextureSize);
                }
                // set camera to only view - MyLayer
                // set render texture of camera to this's raw image texture
                SpawnLight();

            }
        }
        /// <summary>
        /// Attach a light to our viewer camera
        /// </summary>
        private void SpawnLight()
        {
            if (MyLight == null)
            {
                MyLight = new GameObject();
                MyLight.name = name + "_ViewerLight";
                MyLight.transform.SetParent(MyCamera.transform);
                MyLight.transform.localPosition = LightPosition;
                MyLight.transform.localRotation = Quaternion.identity;
                Light LightComponent = MyLight.GetComponent<Light>();
                if (LightComponent == null)
                {
                    LightComponent = MyLight.AddComponent<Light>();
                }
                LightComponent.type = MyLightType;
                LightComponent.spotAngle = 100;
                LightComponent.intensity = LightIntensity;
                LightComponent.color = LightColor;
                LightComponent.cullingMask = LayerManager.Get().ViewerLayer;
            }
        }

        IEnumerator AddEffectsToCamera()
        {
            /*UnityStandardAssets.Effects.EdgeDetection MyEdgeDetection =
                MyCamera.GetComponent<UnityStandardAssets.ImageEffects.EdgeDetection>();
            if (MyEdgeDetection == null)
            {
                MyEdgeDetection = MyCamera.gameObject.AddComponent<UnityStandardAssets.ImageEffects.EdgeDetection>();
            }
            MyEdgeDetection.mode = UnityStandardAssets.ImageEffects.EdgeDetection.EdgeDetectMode.RobertsCrossDepthNormals;
            MyEdgeDetection.edgesOnlyBgColor = Color.black;
            //yield return new WaitForSeconds(0.1f);
            if (MyEdgeShader == null)
                MyEdgeShader = Shader.Find("EdgeDetectNormals");
            if (MyEdgeMaterial == null)
            {
                MyEdgeMaterial = new Material(MyEdgeShader);
            }
            MyEdgeDetection.edgeDetectShader = MyEdgeShader;
            MyEdgeDetection.edgeDetectMaterial = MyEdgeMaterial;
            //MyEdgeDetection.CheckResources();
            MyEdgeDetection.BeSupported();
            //yield return new WaitForSeconds(0.1f);
            MyEdgeDetection.enabled = false;
            //yield return new WaitForSeconds(0.1f);
            MyEdgeDetection.enabled = true;
            yield return new WaitForSeconds(0.1f);*/
            yield return null;
        }
		#endregion

		#region GettersAndSetters
		public Camera GetRenderCamera()
        {
            return MyCamera;
        }
        /// <summary>
        /// Spin the model
        /// </summary>
        public void SetAutoRotate(bool IsAutoRotate_)
        {
            IsAutoRotate = IsAutoRotate_;
        }

        public void SetGridSnap(bool NewState)
        {
        }
        public void SetGrid(bool NewGridState)
        {
            GetSpawn().GetComponent<GridOverlay>().SetState(NewGridState);
        }

        public GameObject GetSpawn()
        {
            return MySpawn;
        }
        public void SetSpawn(GameObject MySpawn_)
        {
            MySpawn = MySpawn_;
        }
        #endregion

        #region Raycasting
        
        protected bool IsMouseInViewer(Vector2 MousePosition)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(gameObject.GetComponent<RectTransform>(), MousePosition, Camera.main);
        }

        public bool GetRayHitInViewer(Vector2 MousePosition, out Ray MyRay, out RaycastHit MyHit_)
        {

            bool DidHit = GetRayInViewer(MousePosition, out MyRay);
            if (DidHit)
            {
                if (UnityEngine.Physics.Raycast(MyRay.origin, MyRay.direction, out MyHit_, 20, LayerManager.Get().ViewerLayer))  // MyHit stored and used later in block updates etc
                {
                    return true;
                }
            }
            MyHit_ = new RaycastHit();
            return false;
        }
        /// <summary>
        /// Converts normal mouse position to mouse position inside viewer
        /// </summary>
        public Vector2 GetLocalMousePosition(Vector2 MousePosition)
        {
            Vector2 MyRectPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gameObject.GetComponent<RectTransform>(),
                                                                          MousePosition,
                                                                          Camera.main,
                                                                          out MyRectPosition))
            {
                // Convert rect size to texture size
                MyRectPosition += GetComponent<RectTransform>().GetSize() / 2f;    // so no negatives - otherwise begins in centre
                MyRectPosition.x /= GetComponent<RectTransform>().GetSize().x;
                MyRectPosition.y /= GetComponent<RectTransform>().GetSize().y;
                // now 0 to 1 times by width and height
                Texture MyTexture = GetComponent<RawImage>().texture;
                MyRectPosition.x *= MyTexture.width;
                MyRectPosition.y *= MyTexture.height;
                return MyRectPosition;
            }
            return new Vector2(0, 0);
        }

        /// <summary>
        /// Use mouse position to get a raycast in the viewer
        /// </summary>
        public bool GetRayInViewer(Vector2 MousePosition, out Ray MyRay)
        {
            //Ray MyRay;
            Vector2 MyRectPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gameObject.GetComponent<RectTransform>(),
                                                                          MousePosition,
                                                                          Camera.main,
                                                                          out MyRectPosition))
            {
                // Convert rect size to texture size
                MyRectPosition += GetComponent<RectTransform>().GetSize() / 2f;    // so no negatives - otherwise begins in centre
                MyRectPosition.x /= GetComponent<RectTransform>().GetSize().x;
                MyRectPosition.y /= GetComponent<RectTransform>().GetSize().y;
                // now 0 to 1 times by width and height
                Texture MyTexture = GetComponent<RawImage>().texture;
                MyRectPosition.x *= MyTexture.width;
                MyRectPosition.y *= MyTexture.height;
                MyRay = MyCamera.ScreenPointToRay(new Vector3(MyRectPosition.x, MyRectPosition.y, 0));
                return true;
            }
            MyRay = new Ray();
            return false;
        }

        protected bool GetViewportPointInViewer(Vector2 MousePosition, out Vector3 MyViewportPoint)
        {
            //Ray MyRay;
            Vector2 MyRectPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gameObject.GetComponent<RectTransform>(),
                                                                          MousePosition,
                                                                          Camera.main,
                                                                          out MyRectPosition))
            {
                // Convert rect size to texture size
                MyRectPosition += GetComponent<RectTransform>().GetSize() / 2f;    // so no negatives - otherwise begins in centre
                MyRectPosition.x /= GetComponent<RectTransform>().GetSize().x;
                MyRectPosition.y /= GetComponent<RectTransform>().GetSize().y;
                // now 0 to 1 times by width and height
                Texture MyTexture = GetComponent<RawImage>().texture;
                MyRectPosition.x *= MyTexture.width;
                MyRectPosition.y *= MyTexture.height;
                MyViewportPoint = MyCamera.ScreenToViewportPoint(new Vector3(MyRectPosition.x, MyRectPosition.y, 0));
                return true;
            }
            MyViewportPoint = new Vector3();
            return false;
        }
        #endregion

        #region Moving
        #endregion

        #region Camera

        protected void RotateObject(Vector2 RotateVector)
        {
            RotateVector *= RotationSpeed;
            Vector3 RotatePivot = MySpawn.transform.position;
            Vector3 RightDirection = MyCamera.transform.right;
            Vector3 UpDirection = MyCamera.transform.up;
            Vector3 ForwardDirection = MyCamera.transform.forward;
            //Vector3 RotateVector3 = MyCamera.transform.InverseTransformDirection(RotateVector);// // 
            Quaternion BeforeRotation = MySpawn.transform.rotation;
            MySpawn.transform.Rotate(MySpawn.transform.InverseTransformDirection(RightDirection), -RotateVector.y);
            MySpawn.transform.Rotate(MySpawn.transform.InverseTransformDirection(UpDirection), RotateVector.x);
            MySpawn.transform.rotation = BeforeRotation;
        }
        private void PanCamera(Vector2 MousePositionDifference)
        {
            PanPosition += (new Vector2(-MousePositionDifference.x, MousePositionDifference.y) / 250f) * CameraPanSpeed;
        }
        /// <summary>
        /// Updates the camera position and rotation
        /// </summary>
        private void UpdateCamera()
        {
            /*if (MyCamera)
            {
                Vector3 NewPosition = new Vector3(PanPosition.x, PanPosition.y, MySpawn.transform.lossyScale.z * ZoomPosition);
                if (NewPosition != MyCamera.transform.position)
                {
                    MyCamera.transform.position = Vector3.Lerp(
                        MyCamera.transform.position,
                        NewPosition,
                        Time.deltaTime * CameraLerpSpeed);
                }
                if (MySpawn)
                {
                    MySpawn.transform.rotation = Quaternion.Lerp(
                        MySpawn.transform.rotation,
                        TargetRotation,
                        Time.deltaTime * CameraLerpSpeed);
                }
                //MySpawn.transform.Rotate(TargetRotation);
            }*/
        }

        /// <summary>
        /// Restores position and rotation of spawned object!
        /// </summary>
        public void RestoreTransform()
        {
            if (MySpawn != null)
            {
                MySpawn.transform.position = GetSpawnPosition();
            }
        }

        /// <summary>
        /// Changes the rotation to a certain view
        /// </summary>
        public void ChangeView(string ViewType)
        {
            if (ViewType == "Top")
            {

            }
            else if (ViewType == "Bottom")
            {

            }
            else if (ViewType == "Left")
            {

            }
            else if (ViewType == "Right")
            {

            }
            else if (ViewType == "Front")
            {

            }
            else if (ViewType == "Back")
            {

            }
        }
        #endregion
    }

}
/*/// <summary>
/// Called whem mouse is dragged in the viewer
/// </summary>
protected void OnTheDrag(Vector2 NewMousePosition, PointerEventData.InputButton PointerID)
{
    if (!IsDragging && (!IsAltRotate|| (IsAltRotate && Input.GetKey(KeyCode.LeftAlt))))
    {
        IsDragging = true;
        DragPositionBegin = NewMousePosition;
        PreviousMousePosition = NewMousePosition;
    }
    if (IsDragging)
    {
        Vector2 MousePositionDifference = DragPositionBegin - NewMousePosition;
        if (CanRotate() && SelectedObject == null)
        {
            if (PointerID == PointerEventData.InputButton.Middle)   // Middle click to pan
            {
                PanCamera(MousePositionDifference);
            }
            else
            {
                RotateObject(MousePositionDifference);
            }
        }
        OnDragged(NewMousePosition, MousePositionDifference);
        PreviousMousePosition = NewMousePosition;
        DragPositionBegin = NewMousePosition;
    }
}*/

/// <summary>
/// Sets all the objects to view in a layer, so it can be seperated from the rest of the scene
/// </summary>
/*private void SetLayerRecursive(GameObject Object, LayerMask Layer)
{
    Object.layer = Mathf.RoundToInt(Mathf.Log(Layer.value, 2));
    for (int i = 0; i < Object.transform.childCount; i++)
    {
        SetLayerRecursive(Object.transform.GetChild(i).gameObject, Layer);
    }
}*/
/// <summary>
/// The object will follow the mouse position
/// </summary>
/*void UpdateObjectMouseFollow()
{
    if (IsMovingObject)
    {
        ObjectFollowMouse(Input.mousePosition);
        if (Input.GetMouseButtonUp(0))
        {
            IsMovingObject = false;
        }
    }
}
/// <summary>
/// Moves the selected object towards the mouse position
/// </summary>
protected void ObjectFollowMouse(Vector2 MousePosition)
{
    if (SelectedObject != null)
    {
        Vector3 OriginalLocalPosition = SelectedObject.transform.localPosition;
        Vector3 OriginalPosition = SelectedObject.transform.position;
        Vector3 NewPosition = MouseToBonePosition(MousePosition, SelectedObject.transform.position);
        // Convert this to local position for Selected Object!
        SelectedObject.transform.position = NewPosition;
        NewPosition = SelectedObject.transform.localPosition;
        //NewPosition -= SelectedObject.transform.parent.position;
        SelectedObject.transform.position = OriginalPosition;
        Vector3 DifferencePosition = NewPosition - OriginalLocalPosition;
        if (IsGridSnap)
        {
            NewPosition.y = (Mathf.RoundToInt(NewPosition.y * GridSnapLength)) / GridSnapLength;
            NewPosition.x = (Mathf.RoundToInt(NewPosition.x * GridSnapLength)) / GridSnapLength;
            NewPosition.z = (Mathf.RoundToInt(NewPosition.z * GridSnapLength)) / GridSnapLength;
            DifferencePosition.y = (Mathf.RoundToInt(DifferencePosition.y * GridSnapLength)) / GridSnapLength;
            DifferencePosition.x = (Mathf.RoundToInt(DifferencePosition.x * GridSnapLength)) / GridSnapLength;
            DifferencePosition.z = (Mathf.RoundToInt(DifferencePosition.z * GridSnapLength)) / GridSnapLength;
        }
        if (IsMoveObjects == true)
        {
            SelectedObject.transform.localPosition = NewPosition;
        }
        //Debug.Log("Original-" + OriginalLocalPosition.ToString() + "-NewPosition-" + NewPosition.ToString());
        OnMoveGameObject(
            SelectedObject,
            NewPosition,
            DifferencePosition);
        //OnUpdatePosition();
    }
}
protected virtual void OnMoveGameObject(GameObject MovedObject, Vector3 NewPosition, Vector3 DifferencePosition)
{

}
protected Vector3 MouseToBonePosition(Vector2 MousePosition)
{
    return MouseToBonePosition(MousePosition, new Vector3());
}
protected Vector3 MouseToBonePosition(Vector2 MousePosition, Vector3 NewPoint)
{
    NewPoint = MyCamera.transform.InverseTransformPoint(NewPoint);
    float Distance = NewPoint.z;
    return MouseToBonePosition(MousePosition, Distance);
}
/// <summary>
/// Using raycasting to find a position inside a viewport
/// </summary>
protected Vector3 MouseToBonePosition(Vector2 MousePosition, float Distance)
{
    Vector2 ViewportPoint = new Vector2();
    RectTransformUtility.ScreenPointToLocalPointInRectangle(gameObject.GetComponent<RectTransform>(),
                                                                  MousePosition,
                                                                  Camera.main,
                                                                  out ViewportPoint);
    ViewportPoint += GetComponent<RectTransform>().GetSize() / 2f;
    ViewportPoint.x /= GetComponent<RectTransform>().GetSize().x;
    ViewportPoint.y /= GetComponent<RectTransform>().GetSize().y;
    Texture MyTexture = GetComponent<RawImage>().texture;
    ViewportPoint.x *= MyTexture.width;
    ViewportPoint.y *= MyTexture.height;
    ViewportPoint = MyCamera.transform.InverseTransformPoint(MyCamera.ScreenToWorldPoint(new Vector3(ViewportPoint.x, ViewportPoint.y, Distance)));
    return MyCamera.transform.position + new Vector3(-ViewportPoint.x, ViewportPoint.y, 0) + MyCamera.transform.forward * Distance;
}*/
