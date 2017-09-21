using UnityEngine;
using UnityEngine.Events;

namespace Zeltex.Guis
{
    /// <summary>
    /// Toggles an object on and off.
    /// Can use distance to a target as the trigger.
    /// Another option to target camera.
    /// Used alot in guis
    /// Toggles by player input, works accross networks.
    /// This works by turning on and off child gameObjects.
    /// </summary>
    public class ZelGui : MonoBehaviour
	{
        #region Variables
        [Header("Events")]
        public UnityEvent OnToggledOn;
        public UnityEvent OnToggledOff;
        private Orbitor MyOrbitor;
        private CanvasGroup MyCanvasGroup;
        private Canvas MyCanvas;
        [SerializeField]
        private bool IsKeepCamera;
        private Animator MyAnimator;
        #endregion

        public void OnBegin()
        {
            OnToggledOn.Invoke();
        }

        void Start()
        {
            MyAnimator = GetComponent<Animator>();
            MyOrbitor = gameObject.GetComponent<Orbitor>();
            MyCanvasGroup = GetComponent<CanvasGroup>();
            MyCanvas = GetComponent<Canvas>();
            if (!IsKeepCamera && CameraManager.Get() && MyCanvas)
            {
                MyCanvas.worldCamera = CameraManager.Get().GetMainCamera();
                CameraManager.Get().OnMainCameraChange.AddEvent(OnMainCameraChangeAction);
            }
            else
            {
                Debug.LogError("Camera Manager or Canvas not in scene.");
            }
        }

        private void OnMainCameraChangeAction()
        {
            MyCanvas.worldCamera = CameraManager.Get().GetMainCamera();
        }

        #region GettersAndSetters
        // other stuff
        public bool GetState()
        {
            return gameObject.activeSelf;
        }

        public bool GetActive()
        {
            return gameObject.activeSelf;
        }
        #endregion

        #region Toggle
        public void Toggle()
        {
            SetState(!gameObject.activeSelf);
        }
        public void TurnOn()
        {
            SetState(true);
        }
        public void TurnOff()
        {
            SetState(false);
        }

        public void SetState(bool IsEnabled)
        {
            if (gameObject.activeSelf != IsEnabled)
            {
                //Debug.Log("Changing state of " + gameObject.name + " From " + gameObject.activeSelf + " To " + IsEnabled);
                gameObject.SetActive(IsEnabled);
                if (IsEnabled)
                {
                    OnToggledOn.Invoke();
                    if (MyOrbitor != null)
                    {
                        MyOrbitor.OnBegin();
                    }
                }
                else
                {
                    OnToggledOff.Invoke();
                }
            }
        }
        #endregion

        public void SetChildStates(bool NewState)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(NewState);
            }
        }

        public void Disable()
        {
            CanvasGroup MyCanvasGroup = GetComponent<CanvasGroup>();
            if (MyCanvasGroup)
            {
                MyCanvasGroup.interactable = false;
                MyCanvasGroup.blocksRaycasts = false;
                //MyCanvasGroup.alpha = 0.3f;
                if (transform.Find("DisabledOverlay"))
                {
                    transform.Find("DisabledOverlay").gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError(name + " has no CanvasGroup");
            }
        }

        public void Enable()
        {
            if (MyCanvasGroup)
            {
                MyCanvasGroup.interactable = true;
                MyCanvasGroup.blocksRaycasts = true;
                //MyCanvasGroup.alpha = 1f;
                transform.Find("DisabledOverlay").gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError(name + " has no CanvasGroup");
            }
        }
    }
}


// Auto toggle

//IsAutoToggle = NewToggle;
//IsAutoToggle = !IsAutoToggle;
//if (IsTargetMainCamera)
//{
//    TargetObject = Camera.main.transform;
//    transform.rotation = Camera.main.transform.rotation;
//}
//if (TargetObject != null)
//{
//    transform.position = TargetObject.position;
//    transform.rotation = TargetObject.rotation;
//}
/// <summary>
/// if we change our main camera in our scene, we will need to update it.. lol
/// </summary>
/*public void UpdateMainCamera() 
{
    if (IsTargetMainCamera && Camera.main) {
        if (TargetObject == null || TargetObject != Camera.main.transform) 
        {
            TargetObject = Camera.main.transform;
        }
    }
}*/
/*public void SetAuto(bool NewAuto) 
{
    IsAutoToggle = NewAuto;
}*/

/*public void SetTarget(Transform NewTarget)
{
    TargetObject = NewTarget;
}*/
/// <summary>
/// saves the distance, and turns it on or off depending on distance
/// </summary>
/*private void UpdateDistance()
{
    float DisableDistance = 10;
    if (IsAutoToggle && OverrideActive && TargetObject)
    {
        // Find the current distance to the toggle gameobject
        float DistanceToCam = Vector3.Distance(transform.position, TargetObject.transform.position);

        if (DistanceToCam > DisableDistance)
        {
            SetActive(false);
        }
        else
        {
            SetActive(true);
        }
    }
}*/
