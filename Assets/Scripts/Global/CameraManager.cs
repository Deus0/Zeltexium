using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{
	/// <summary>
	/// Manages cameras
	/// connects with billboard scripts
	/// </summary>
	public class CameraManager : ManagerBase<CameraManager>
	{
		[SerializeField]
		private bool IsSpawnOnStart;
		[SerializeField]
		private GameObject MainMenuCameraPrefab;
        [SerializeField]
        private GameObject FpsCameraPrefab;
        //[SerializeField]
		//private Camera MainCamera;
        [SerializeField]
        private Camera MainMenuCamera;
        [SerializeField]
        private Camera FpsCamera;
        //[Tooltip("Anything linked to the main camera have their events added to here")]
        [HideInInspector]
        public UnityEngine.Events.UnityEvent OnMainCameraChange;
        private bool IsParentToAllCameras = true;

        private Camera MainCamera;

        public new static CameraManager Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.Find(ManagerNames.CameraManager).GetComponent<CameraManager>();
            }
            return MyManager as CameraManager;
        }

        private void Start()
		{
			if (IsSpawnOnStart)
			{
                SpawnMainMenuCamera();
            }
			else
			{
                if (Camera.main && IsParentToAllCameras)
                {
                    Camera.main.transform.SetParent(transform);
                }
			}
		}

        public Camera SpawnGameCamera()
        {
            if (FpsCamera)
            {
                return FpsCamera;
            }
            GameObject SpawnedCamera = Instantiate(FpsCameraPrefab);
            FpsCamera = SpawnedCamera.GetComponent<Camera>();
            return FpsCamera;
        }

        public void SetGuiCamera(Camera NewGuiCamera)
        {
            MainMenuCamera = NewGuiCamera;
        }

        public Camera GetGuiCamera()
        {
            return MainMenuCamera;
        }

        public Camera SpawnMainMenuCamera()
        {
            if (MainMenuCameraPrefab)
            {
                MainMenuCamera = Instantiate(MainMenuCameraPrefab).GetComponent<Camera>();
                if (IsParentToAllCameras)
                {
                    MainMenuCamera.transform.SetParent(transform);
                }
                MainCamera = MainMenuCamera;
                OnMainCameraChange.Invoke();
                return MainMenuCamera;
            }
            else
            {
                Debug.LogError("Set camera prefab.");
                return null;
            }
        }

		public Camera GetClosestCamera(Transform MyTransform)
		{
			float ClosestDistance = 20f;
			int ClosestIndex = -1;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform Child = transform.GetChild(i);
				float ThisDistance = Vector3.Distance(Child.position, transform.position);
				if (ThisDistance < ClosestDistance)
				{
					ClosestDistance = ThisDistance;
					ClosestIndex = i;
				}
			}
			if (ClosestIndex != -1)
			{
				return transform.GetChild(ClosestIndex).GetComponent<Camera>();
				//ViewerCamera = Camera.main.transform.parent.GetChild(ClosestIndex).gameObject.GetComponent<Camera>();
				/*if (ViewerCamera)
				{
					TargetPosition = ViewerCamera.transform;
				}*/
			}
			return null;
		}

        public Camera GetMainCamera()
        {
            return MainCamera;
        }

        public void EnableGameCamera()
        {
            if (MainMenuCamera)
            {
                MainMenuCamera.gameObject.SetActive(false);
            }
            if (FpsCamera)
            {
                FpsCamera.gameObject.SetActive(true);
                MainCamera = FpsCamera;
                OnMainCameraChange.Invoke();
            }
        }

        public void EnableMainMenuCamera()
        {
            if (FpsCamera)
            {
                FpsCamera.gameObject.SetActive(false);
            }
            if (MainMenuCamera)
            {
                MainMenuCamera.gameObject.SetActive(true);
                MainCamera = MainMenuCamera;
                OnMainCameraChange.Invoke();
            }
        }

    }
}

