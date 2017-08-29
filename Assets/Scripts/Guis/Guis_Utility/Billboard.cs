using UnityEngine;

/*
	Main script for handling a 3d canvas
		- dialog speech bubble
		- characters name/title etc

	Functions:
		Orbits character - at specific position or orbit aroud
		Toggles canvas
		Lerps the movement to follow the position
		Auto Toggle if distance too great
*/
namespace Zeltex.Guis
{
    /// <summary>
    /// Makes the gui always face the main camera or a character
	/// ToDO: Make camera manager and link to billboards
    /// </summary>
	public class Billboard : MonoBehaviour 
	{
        public static bool IsLookAtMainCamera;
        private bool LastIsLookAtMainCamera;
		[Tooltip("Sets the MainCamera as the Target")]
		[SerializeField]
        private bool IsFaceMainCamera = true;
		[Tooltip("Constantly looks towards the object")]
		[SerializeField]
        private Transform TargetPosition;
        private ZelGui MyZelGui;
        private Vector3 MyLastPosition;
        private Vector3 LastPosition;
        private Quaternion LastRotation;
        [SerializeField]
		private Camera ViewerCamera;
        private float LastCheckedMainCamera;
        private float CheckMainCameraCooldown = 30f;
        private UnityEngine.Events.UnityAction OnMainCameraChangeAction;

        void Start()
        {
            MyZelGui = gameObject.GetComponent<ZelGui>();
            UpdateMainCamera();
            OnMainCameraChangeAction = UpdateMainCamera;
            CameraManager.Get().OnMainCameraChange.AddEvent(OnMainCameraChangeAction);
		}

        public void UpdateMainCamera()
        {
            ViewerCamera = CameraManager.Get().GetMainCamera();
            if (ViewerCamera)
            {
                TargetPosition = ViewerCamera.transform;
            }
        }

        public void OnDestroy()
        {
            CameraManager.Get().OnMainCameraChange.RemoveEvent(OnMainCameraChangeAction);
        }
        /*Camera[] MyCameras = GameObject.FindObjectsOfType<Camera>();
        for (int i = 0; i < MyCameras.Length; i++)
        {
            if (MyCameras[i].tag != "MainCamera")
            {
                ViewerCamera = MyCameras[i];
                break;
            }
        }*/

        // Update is called once per frame
        void Update () 
		{
            //UpdateCamera();
            if (MyZelGui && MyZelGui.GetActive())
            {
                UpdateBillboard();
            }
        }

        private void UpdateCamera()
        {
            if (IsFaceMainCamera && Camera.main)
            {
                if (ViewerCamera == null || Time.time - LastCheckedMainCamera >= CheckMainCameraCooldown)
                {
                    LastCheckedMainCamera = Time.time;
                    if (ViewerCamera == null)
                    {
                        LastCheckedMainCamera -= Random.Range(-CheckMainCameraCooldown / 2f, CheckMainCameraCooldown / 2f);
                    }
					ViewerCamera = CameraManager.Get().GetClosestCamera(transform);
				}
                if (ViewerCamera)
                {
                    TargetPosition = ViewerCamera.transform;
                }
            }
        }
        /// <summary>
        /// Only updates if the target has moved or rotated.
        /// </summary>
		private void UpdateBillboard()
        {
            if (TargetPosition != null &&
                (LastPosition != TargetPosition.position
                || LastRotation != TargetPosition.rotation
                || transform.position != MyLastPosition))
            {
                MyLastPosition = transform.position;
                LastPosition = TargetPosition.position;
                LastRotation = TargetPosition.rotation;
                TargetAngle = Quaternion.Euler(TargetPosition.eulerAngles);
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, TargetAngle, 8f * Time.deltaTime);
        }
        // idea: take a list of previous position/rotation changes, and times, and lag behind, looking at a certain one

        private Quaternion TargetAngle = Quaternion.identity;

		public void SetTarget(Transform NewTarget)
		{
			if (NewTarget != null)
				TargetPosition = NewTarget;
		}

		/*public void FaceMainCamera()
		{
			IsFaceMainCamera = true;
			TargetPosition = Camera.main.transform;
		}*/
	}
}	