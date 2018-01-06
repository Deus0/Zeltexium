using UnityEngine;
using UnityEngine.Events;
using Zeltex.Skeletons;
using System.Collections;

namespace Zeltex.Guis
{
    /// <summary>
    /// Orbits the gui in space around a character or object
    /// ToDo: Slow down rotation a bit
    /// </summary>
	public class Orbitor : MonoBehaviour 
	{
        #region Variables
        private static int InitiateStickTime = 25;
        [Header("Target")]
        [Tooltip("Sets the target to the main camera")]
        [SerializeField]
        private bool IsTargetMainCamera = true;
        [Tooltip("The target transform o f the orbital paths")]
        public Transform TargetObject;
        public SkeletonHandler TargetSkeleton;
        private Quaternion TargetRotation;  // the rotation to lerp to
        private Vector3 TargetPosition;		// the position to lerp to
        [Header("Positioning")]
        [SerializeField]
        private Vector2 ScreenPosition = new Vector2(0, 0);
        [Header("Movement")]
        [Tooltip("The linear speed of movement to the orbit position")]
		[SerializeField]
        private float MovementSpeed = 3f;
        [Tooltip("The linear speed of movement to the orbit position")]
        private float RotationSpeed = 0.5f;
        [SerializeField]
        private bool IsInstantOnStart;
        [Tooltip("Used to make speed instant, attaches itself as a child to the target")]
		[SerializeField]
        private bool IsInstant = false;
		[Header("Positioning")]
		[Tooltip("The angle used in positioning away from the target")]
		[SerializeField]
        private Vector3 MyDirection;
		[Tooltip("The distance away from the target")]
		public float MyDisplayDistance = 1;
		[SerializeField]
        private bool IsFollowUserAngle = true;			// rotates to position according to users transform.forward angle
		public bool IsFollowUserAngleAddition = true;	// will follow camera rotation on an angle addition

        // Spinning
        private bool IsSpinning = false;
        private float TimeCount;
        private float LastSpunTime = 0f;
        private float TimeDifference = 0f;
		public float SpinSpeed = 0.5f;
        private ZelGui MyZelGui;
        public bool IsFreezePositionZ = false;

        private UnityAction ScreenSizeChangeEvent;
        #endregion

        #region Mono

        private void Awake()
        {
            ScreenSizeChangeEvent = OnScreenSizeChange;
            if (IsTargetMainCamera && CameraManager.Get())
            {
                CameraManager.Get().OnMainCameraChange.AddEvent(OnMainCameraChange);
            }
            //DataManager.Get().StartCoroutine(AwakeRoutine());
            Initiate();
        }

        private void Start()
        {
            Initiate();
        }

        void OnEnable()
        {
            Initiate();
        }

        void Initiate()
        {
            RoutineManager.Get().StartCoroutine(InitiateRoutine());
        }

        private IEnumerator InitiateRoutine()
        {
            if (IsTargetMainCamera && CameraManager.Get() && CameraManager.Get().GetMainCamera())
            {
                TargetObject = CameraManager.Get().GetMainCamera().transform;
            }
            if (TargetObject != null)
            {
                for (int i = 0; i < InitiateStickTime; i++)
                {
                    CheckOrbitPosition();
                    transform.position = GetTargetWorldPosition();
                    transform.rotation = TargetRotation;
                    yield return null;
                }
            }
        }

        private void OnMainCameraChange()
        {
            TargetObject = CameraManager.Get().GetMainCamera().transform;
        }

        public void OnScreenSizeChange()
        {

        }

        void Update()
        {
            if (TargetSkeleton != null)
            {
                TargetObject = TargetSkeleton.GetSkeleton().MyCameraBone;
            }
            if (TargetObject != null)
            {
                CheckOrbitPosition();
                UpdateOrbit(Time.deltaTime);
            }
        }
        #endregion

        #region GettersAndSetters

        /// <summary>
        /// Sets the speed
        /// </summary>
        public void SetSpeed(float NewSpeed)
        {
            MovementSpeed = NewSpeed;
        }

        /// <summary>
        /// SetScreenPosition
        /// </summary>
		public void SetScreenPosition(Vector2 ScreenPosition_)
        {
            ScreenPosition = ScreenPosition_;
        }

        /// <summary>
        /// Get screen position
        /// </summary>
        public Vector2 GetScreenPosition()
        {
            return ScreenPosition;
        }

        public void SetTarget(Transform TargetObject_)
        {
            SetTarget(TargetObject_, TargetSkeleton);
        }
        /// <summary>
        ///  Sets target of the orbitor
        /// </summary>
        public void SetTarget(Transform TargetObject_, SkeletonHandler TargetSkeleton_)
        {
            TargetSkeleton = TargetSkeleton_;
            if (TargetObject_ != null)
            {
                TargetObject = TargetObject_;
                if (IsInstant)
                {
                    transform.SetParent(TargetObject_);
                    transform.localPosition = Vector3.zero;
                }
            }
        }
        #endregion

        #region Orbit

        /// <summary>
        ///  rotates around player using x and z angles
        /// </summary>
        private void SpinDirection()
        {
            if (IsSpinning)
            {
                TimeCount += Time.deltaTime;
                float SpinAngle = (TimeCount - TimeDifference) * SpinSpeed;
                if (!IsSpinning)
                {
                    SpinAngle = LastSpunTime;
                }
                else {
                    LastSpunTime = SpinAngle;
                }
                MyDirection.x += SpinAngle;
                MyDirection.z += SpinAngle;
            }
        }

        /// <summary>
        /// Teleports the gui to its target
        /// </summary>
        public void TeleportToTarget()
        {
            CheckOrbitPosition();
            UpdateOrbit(10000);
        }

        /// <summary>
        /// Orbit position is in direction of the camera/target gameobject - multiplied by distance
        /// </summary>
        private void CheckOrbitPosition()
        {
            SpinDirection();
            Vector3 TemporaryDirection = new Vector3((MyDirection.x), 0, (MyDirection.z)).normalized;  // first calculate direction
            if (TargetObject)
            {
                if (IsFollowUserAngle)
                {
                    TemporaryDirection = TargetObject.transform.forward;
                }
                else if (IsFollowUserAngleAddition)
                {
                    TemporaryDirection = TargetObject.transform.forward * MyDirection.z + TargetObject.transform.right * MyDirection.x;
                }
            }
            // then change the direction into a position value
            TargetPosition = TemporaryDirection * MyDisplayDistance;
            if (TargetObject)
            {
                TemporaryDirection = TargetObject.forward;
                TargetRotation = TargetObject.rotation;
            }
            else
            {
                TargetRotation = Quaternion.LookRotation(TemporaryDirection, Vector3.up);
            }
        }

        /// <summary>
        /// Update the position of the gui
        /// </summary>
        private void UpdateOrbit(float DeltaTime)
        {
            //TransformedGuiOffset = Vector3.zero;
            float TimeValue = DeltaTime * MovementSpeed / (MyDisplayDistance + 1);
            //float RotationLerpValue = DeltaTime * RotationSpeed;
            if (IsInstant)
            {
                transform.position = GetTargetWorldPosition();
                transform.rotation = TargetRotation;
            }
            else
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    GetTargetWorldPosition(),
                    TimeValue);
                transform.rotation = TargetRotation;
                //transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, RotationLerpValue);
            }
            if (IsFreezePositionZ)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
        }

        /// <summary>
        /// Gets the target position in the world from RectUpdater
        /// </summary>
        Vector3 GetTargetWorldPosition()
        {
            Vector2 ScaledPosition = ScreenSizeManager.BaseToScaledPosition(ScreenPosition, transform.lossyScale, MyDisplayDistance);
            Vector3 TransformedGuiOffset = transform.TransformVector(ScaledPosition);
            Vector3 NewPosition = TargetPosition + TransformedGuiOffset;
            if (TargetObject)
            {
                NewPosition += TargetObject.transform.position;
            }
            /*if (transform.name == "SkillBar")
            {
                Debug.LogError("  0 ScreenPosition: " + ScreenPosition.ToString() + " ::: " + MyDisplayDistance);
                Debug.LogError("  1 ScaledPosition: " + ScaledPosition.ToString());
                Debug.LogError("  2 TransformedGuiOffset: " + TransformedGuiOffset.ToString());
                Debug.LogError("  3 NewPosition: " + NewPosition.ToString());
            }*/
            return NewPosition;
        }
        #endregion
    }
}