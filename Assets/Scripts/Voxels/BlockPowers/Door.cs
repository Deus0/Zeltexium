using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

// need to open the door differently depending on what side im on

// Rotation point should be a setting - Centre, Left, Right - and should get the actual value by calculating the objects boundary box

namespace Zeltex.WorldUtilities
{
    /// <summary>
    /// used to open doors!
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("Events")]
	    public UnityEvent OnOpenDoor = null;
	    public UnityEvent OnClickLockedDoor = null;
	    [Header("Options")]
	    [SerializeField] private bool IsLocked = false;
	    [SerializeField] private float TimeToAnimate = 1.5f;
        public Vector3 RotateAngle = new Vector3(-90, 0, 0);
        public float TimePassed = 0;
        private float AnimateLength = 1.5f;
        [Tooltip("The amount of euler angles to rotate")]
        public Vector3 OriginalRotateAngle = new Vector3(-90, 0, 0);
        public Vector3 ForceRotationPoint = new Vector3(0.2f, 0, 0.2f);   //new Vector3(0.4f, 0, 0.4f); // -0.45, 0, 0.45, 90,0,0
        private Vector3 RotationPoint = new Vector3(0.2f, 0, 0.2f);    // -0.45, 0, 0.45, 90,0,0
	    public bool IsForeverRotate = false;
	    [Header("Audio")]
	    public float SoundVolume = 1f;
	    public AudioSource SoundSource;	// source of where the sound is emmited from - getting hit, casting spell, dying etc
	    public AudioClip OpeningSound;
	    public AudioClip ClosingSound;
        public List<Door> LinkedDoors = new List<Door>();

	    // states
	    private bool IsOpeningDoor = false;
	    private bool IsClosingDoor = false;
	    private int LastState = 1;					// used to toggle door
	    private Vector3 BeginAngle = new Vector3(0,0,0);
	    private Vector3 EndAngle = new Vector3(0,90,0);
	    private float Direction = -1;
	    private float BeginDirection = -1;
	    private float EndDirection = 1;
	    private Vector3 Pivot;
	    private Vector3 BeginPosition;
	    private Vector3 EndPosition;
        private float TimeBegun;
        public bool IsAnimatePosition = false;
	    public Vector3 NewPosition;
        bool HasBegun;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Pivot, Pivot + new Vector3(0, 5, 0));
            Gizmos.DrawLine(Pivot, Pivot + new Vector3(1, 0, 0));
            Gizmos.DrawLine(Pivot, Pivot + new Vector3(-1, 0, 0));
        }
        void Begin()
        {
            if (HasBegun)
                return;
            HasBegun = true;
            SoundSource = GetComponent<AudioSource>();
            BeginAngle = transform.rotation.eulerAngles;
            BeginPosition = transform.position;
            RotationPoint = transform.InverseTransformDirection(RotationPoint);
            if (EndAngle.x > 0 || RotateAngle.y > 0 || RotateAngle.z > 0)
            {
                BeginDirection = -1; EndDirection = 1;
            }
            else
            {
                BeginDirection = 1; EndDirection = -1;
            }
            //transform.TransformPoint
            Pivot = BeginPosition + (RotationPoint);    // to find end positoin
            transform.RotateAround(Pivot,
                                    Vector3.up * BeginDirection,
                                    RotateAngle.x);
            EndPosition = transform.position;
            EndAngle = transform.rotation.eulerAngles;
            RestoreBeginState();

            if (IsAnimatePosition)
            {
                EndPosition = BeginPosition + NewPosition;
                EndAngle = BeginAngle;
            }
        }

	    private void RestoreBeginState() 
	    {
		    transform.position = BeginPosition;
		    transform.eulerAngles = BeginAngle;
	    }

	    private void RestoreEndState()
        {
		    transform.position = EndPosition;
		    transform.eulerAngles = EndAngle;
	    }

	    // Update is called once per frame
	    void Update ()
        {
		    if ((IsOpeningDoor || IsClosingDoor))
            {
			    Animate();
		    }
            if (ForceRotationPoint != RotationPoint)
            {
                RotationPoint = ForceRotationPoint;
                HasBegun = false;
                Begin();
            }
	    }
        /// <summary>
        /// Animates the door opening and closing
        /// </summary>
	    private void Animate() 
	    {
		    float TimeSinceBegun = Time.time - TimeBegun;
		    if (TimeSinceBegun >= TimeToAnimate)
            {
                StopAnimation();
		    }
            else
            {
                if (!IsAnimatePosition)
                {
                    TimePassed += Direction * (Time.deltaTime);
                    float AngleAdditionX = (RotateAngle.x) * (Time.deltaTime / TimeToAnimate) * Direction;
                    transform.RotateAround( Pivot,
                                            Vector3.up,
                                            AngleAdditionX);
                }
                else
                {
                    if (IsOpeningDoor)
                    {
                        transform.position = Vector3.Lerp(EndPosition, BeginPosition, TimeSinceBegun / TimeToAnimate);
                    }
                    else if (IsClosingDoor)
                    {
                        transform.position = Vector3.Lerp(BeginPosition, EndPosition, TimeSinceBegun / TimeToAnimate);

                    }
			    }
		    }
	    }
        /// <summary>
        /// Stops the animation
        /// </summary>
	    private void StopAnimation()
        {
		    if (IsOpeningDoor || IsClosingDoor)
            {
			    //set angle to perfectness
			    // which way do i have to go to rotate the angle perfectly to our target
			    if (IsOpeningDoor)
                {
				    RestoreBeginState();
                    TimePassed = 0;
                }
                else if (IsClosingDoor)
                {
				    RestoreEndState();
                    TimePassed = AnimateLength;
                }
			    IsOpeningDoor = false;
			    IsClosingDoor = false;
		    }
	    }
        /// <summary>
        /// Toggles the door animation
        /// </summary>
	    public void ToggleDoor()
        {
		    if (IsLocked == false)
            {
			    Debug.Log ("Toggling Door");
			    if (IsOpeningDoor || LastState == 1)
                {
				    //CloseDoor ();
                    for (int i = 0; i < LinkedDoors.Count; i++)
                    {
                        LinkedDoors[i].CloseDoor();
                    }
                }
                else if (IsClosingDoor || LastState == 2)
                {
				    //OpenDoor ();
                    for (int i = 0; i < LinkedDoors.Count; i++)
                    {
                        LinkedDoors[i].OpenDoor();
                    }
				    if (OnOpenDoor != null)
                    {
                        OnOpenDoor.Invoke();
                    }
			    }
		    }
            else
            {
			    //CloseDoor ();
			    //if (OnClickLockedDoor != null)
			    //	OnClickLockedDoor.Invoke();
		    }
	    }

	    public void Lock()
        {
		    IsLocked = true;
	    }

	    public void Unlock()
        {
		    IsLocked = false;
	    }

	    private bool IsAnimating()
        {
		    if (IsClosingDoor || IsOpeningDoor)
            {
                return true;
            }
		    else
            {
                return false;
            }
	    }

	    public void OpenDoor() 
	    {
            Begin();
            SetDoorState(true);
        }

	    public void CloseDoor()
        {
            Begin();
            SetDoorState(false);
        }
        private void SetDoorState(bool IsOpen)
        {
            // if toggling during animation
            if (IsAnimating())
            {
                //float TimePassedSinceLastToggle = Time.time - TimeBegun;
                if (IsOpen)
                {
                    TimeToAnimate = (TimePassed); // percentage through kinda thing!
                    RotateAngle.x = Mathf.Lerp(0, OriginalRotateAngle.x, TimeToAnimate / AnimateLength);
                }
                else
                {
                    TimeToAnimate = AnimateLength - (TimePassed); // reverse time for closing!
                    RotateAngle.x = Mathf.Lerp(0, OriginalRotateAngle.x, TimeToAnimate / AnimateLength);
                    //RotateAngle.x = Mathf.Lerp(OriginalRotateAngle.x, 0, TimePassedSinceLastToggle / TimeToAnimate);
                }
                //TimeToAnimate = Time.time - TimeBegun;
                //TimeBegun = Time.time + TimePassed; // reverse time left
                TimeBegun = Time.time;
                //TimePassed += 0;
            }
            else
            {
                //TimePassed = 0;
                RotateAngle.x = OriginalRotateAngle.x;
                TimeToAnimate = AnimateLength;
                TimeBegun = Time.time;
            }
            IsClosingDoor = !IsOpen;
            IsOpeningDoor = IsOpen;
            if (IsOpen)
            {
                Direction = EndDirection;
                LastState = 1;
            }
            else
            {
                Direction = BeginDirection;
                LastState = 2;
            }

            if (SoundSource != null && ClosingSound != null)
            {
                SoundSource.PlayOneShot(ClosingSound, SoundVolume);
            }
        }
    }
}
