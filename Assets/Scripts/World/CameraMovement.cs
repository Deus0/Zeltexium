using UnityEngine;
using System.Collections.Generic;
using Zeltex.Characters;
using UnityEngine.EventSystems;
using Zeltex.Guis;

// Utility download
namespace Zeltex.Cameras
{
    /// <summary>
    /// Basic camera movement script
    /// </summary>
    public class CameraMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float NormalSpeed = 30;
        public float ViewerSpeed = 10;
        private float MovementSpeed = 20.0f; //regular speed
	    //private float maxShift = 1000.0f; //Maximum speed when holdin gshift
	    private float camSens = 0.2f; //How sensitive it with mouse
	    private Vector3 LastMousePosition = new Vector3(0,0,0); //kind of in the middle of the screen, rather than at the top (play)
	    private float totalRun = 1.0f;
        private bool IsMoving = false;
        private Camera MyCamera;
        private Camera MainCamera;
        private ObjectViewer MyViewer;
        private bool IsLerp = true;
        private Vector3 TargetPosition;
        private float LerpSpeed = 10;

        void OnEnable()
        {
            //Debug.Log("OnEnable [" + Time.realtimeSinceStartup + "] Setting MouseLocker in " + name + " to " + false);
	    }

        private void Awake()
        {
            DoesHaveCamera();
        }

        public bool IsCameraMoving()
        {
            return (MyCamera && (MyCamera.transform.position != TargetPosition || IsMoving));
        }

	    void Update()
	    {
		    if (Input.GetMouseButtonDown(1))
            {
                BeginMovement();
            }
            // Movement update!
            else if (IsMoving)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    IsMoving = false;
                    MyCamera = null;
                }
                else
                {
                    MoveCamera();
                }
                LerpCamera();
            }
        }

        private bool DoesHaveCamera()
        {
            if (MainCamera == null)
            {
                MainCamera = gameObject.GetComponent<Camera>();
            }
            if (MainCamera == null)
            {
                MainCamera = Camera.main;
            }
            if (MainCamera == null)
            {
                MainCamera = CameraManager.Get().GetMainCamera();
            }
            MyCamera = MainCamera;
            return (MainCamera != null);
        }

        private void BeginMainCamera()
        {
            if (DoesHaveCamera())
            {
                LastMousePosition = Input.mousePosition;
                IsMoving = true;
                MovementSpeed = NormalSpeed;
                TargetPosition = MyCamera.transform.position;
            }
        }
        private void BeginMovement()
        {
            // if ray hit object viewer
            //Create the PointerEventData with null for the EventSystem
            PointerEventData MyPointerEvent = new PointerEventData(EventSystem.current);
            //Set required parameters, in this case, mouse position
            MyPointerEvent.position = Input.mousePosition;
            //Create list to receive all results
            List<RaycastResult> MyResults = new List<RaycastResult>();
            //Raycast it
            EventSystem.current.RaycastAll(MyPointerEvent, MyResults);

            //if (!Character.IsRayHitGui())
            if (MyResults.Count == 0)
            {
                BeginMainCamera();
            }
            else
            {
                //Debug.LogError("Raycast results: " + MyResults.Count + " at position: " + MyPointerEvent.position.ToString());
                MyViewer = null;
                for (int i = 0; i < MyResults.Count; i++)
                {
                    MyViewer = MyResults[i].gameObject.GetComponent<ObjectViewer>();
                    if (MyViewer)
                    {
                        MyCamera = MyViewer.GetRenderCamera();
                        if (MyCamera)
                        {
                            LastMousePosition = Input.mousePosition;
                            IsMoving = true;
                            MovementSpeed = ViewerSpeed;
                            TargetPosition = MyCamera.transform.position;
                        }
                        else
                        {
                            Debug.LogError(MyViewer.name + " does not have a camera.");
                        }
                        break;
                    }
                }
                if (MyViewer == null)
                {
                    Debug.LogError("Hitting but no viewer");
                    BeginMainCamera();
                }
            }
        }

        void LerpCamera()
        {
            if (IsLerp && MyCamera)
            {
                if (MyCamera.transform.position != TargetPosition)
                {
                    MyCamera.transform.position = Vector3.Lerp(MyCamera.transform.position, TargetPosition, Time.deltaTime * LerpSpeed);
                }
            }
        }

        private void RotateCamera()
        {
            LastMousePosition = Input.mousePosition - LastMousePosition;
            LastMousePosition = new Vector3(-LastMousePosition.y * camSens, LastMousePosition.x * camSens, 0);
            LastMousePosition = new Vector3(
                MyCamera.transform.eulerAngles.x + LastMousePosition.x,
                MyCamera.transform.eulerAngles.y + LastMousePosition.y,
                0);
            MyCamera.transform.eulerAngles = LastMousePosition;
            LastMousePosition = Input.mousePosition;
        }

        void MoveCamera()
        {
            RotateCamera();

            var BaseInput = GetBaseInput();
            var MouseScrollInput = GetInputMouseScroll();
            BaseInput = BaseInput * MovementSpeed;
            MouseScrollInput = MouseScrollInput * 0.1f;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                BaseInput = BaseInput * 2f;
                MouseScrollInput = MouseScrollInput * 4f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                BaseInput.y += MovementSpeed;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                BaseInput.y -= MovementSpeed;
            }

            BaseInput = BaseInput * Time.deltaTime;
            //If player wants to move on X and Z axis only
            if (IsLerp)
            {
                //TargetPosition = TargetPosition + MyCamera.transform.TransformDirection(0.5f * (BaseInput + MouseScrollInput));
                Vector3 Input = 0.5f * (BaseInput + MouseScrollInput);
                Vector3 Velocity = MyCamera.transform.TransformDirection(Input);
                /*Velocity.x = Mathf.RoundToInt(Velocity.x * 10f) / 10f;
                Velocity.y = Mathf.RoundToInt(Velocity.y * 10f) / 10f;
                Velocity.z = Mathf.RoundToInt(Velocity.z * 10f) / 10f;*/
                TargetPosition = TargetPosition + Velocity;
                //Debug.Log("moving camera: " + Input.ToString() + ":" + Velocity.ToString());
            }
            else
            {
               /* Vector3 Input = 0.5f * (BaseInput + MouseScrollInput);
                Vector3 Velocity = MyCamera.transform.TransformDirection(Input);
                TargetPosition = TargetPosition + Velocity;
                Debug.Log("moving camera: " + Input.ToString() + ":" + Velocity.ToString());*/
                //MyCamera.transform.Translate(BaseInput);
                //MyCamera.transform.Translate(MouseScrollInput);
            }
            //transform.position += transform.InverseTransformPoint(BaseInput);
            //transform.position += transform.InverseTransformPoint(MouseScrollInput);
        }

	    private Vector3 GetInputMouseScroll() 
	    {
		    Vector3 MyInput = new Vector3();
		    float MouseScroll = Input.GetAxis ("Mouse ScrollWheel");
		    if (MouseScroll > 0)
            {
			    MyInput += new Vector3 (0, 0, 1);
		    }
            else if (MouseScroll < 0)
            {
			    MyInput += new Vector3 (0, 0, -1);
		    }
		    return MyInput;
	    }

	    //returns the basic values, if it's 0 than it's not active.
	    private Vector3 GetBaseInput()
	    {
		    Vector3 MyInput = new Vector3();
		    if (Input.GetKey (KeyCode.W))
            {
			    MyInput += new Vector3(0, 0 , 1);
		    }
		    if (Input.GetKey (KeyCode.S))
            {
			    MyInput += new Vector3(0, 0 , -1);
		    }
		    if (Input.GetKey (KeyCode.A))
            {
			    MyInput += new Vector3(-1, 0 , 0);
		    }
		    if (Input.GetKey (KeyCode.D))
            {
			    MyInput += new Vector3(1, 0 , 0);
		    }
		    return MyInput;
	    }
    }
}