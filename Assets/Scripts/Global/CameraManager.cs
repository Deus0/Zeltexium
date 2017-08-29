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
		private GameObject CameraPrefab;
        [SerializeField]
		private Camera MainCamera;
        [SerializeField]
        private Camera GuiCamera;
        [Tooltip("Anything linked to the main camera have their events added to here")]
        public UnityEngine.Events.UnityEvent OnMainCameraChange;
        [SerializeField]
        private bool IsParentToAllCameras;

		private void Start()
		{
            /*if (Camera.main)
            {
                SetGuiCamera(Camera.main);
                MainCamera = Camera.main;
            }*/
			if (IsSpawnOnStart)
			{
                SpawnCamera();

            }
			else
			{
                if (Camera.main && IsParentToAllCameras)
                {
                    Camera.main.transform.SetParent(transform);
                }
			}
		}

        public void SetGuiCamera(Camera NewGuiCamera)
        {
            GuiCamera = NewGuiCamera;
        }

        public Camera GetGuiCamera()
        {
            return GuiCamera;
        }

        public Camera SpawnCamera()
        {
            if (CameraPrefab)
            {
                MainCamera = Instantiate(CameraPrefab).GetComponent<Camera>();
                if (IsParentToAllCameras)
                {
                    MainCamera.transform.SetParent(transform);
                }
                OnMainCameraChange.Invoke();
                return MainCamera;
            }
            else
            {
                Debug.LogError("Set camera prefab.");
                return null;
            }
        }

        public Camera GetMainCamera()
        {
            return MainCamera;
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
			else
			{
				//Debug.LogWarning("Could not find close enough camera for: " + name);
			}
			return null;
		}

        public void EnableGameCamera()
        {
            if (GuiCamera)
            {
                GuiCamera.gameObject.SetActive(false);
            }
            if (MainCamera)
            {
                MainCamera.gameObject.SetActive(true);
            }
        }

        public void EnableMainMenuCamera()
        {
            if (GuiCamera)
            {
                GuiCamera.gameObject.SetActive(true);
            }
            if (MainCamera)
            {
                MainCamera.gameObject.SetActive(false);
            }
        }

    }
}

