using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Tools
{
    /// <summary>
    /// Handles mouse clicks on it
    /// </summary>
    public class Gizmo : MonoBehaviour
    {
        [SerializeField]
        private GizmoHandle GizmoAxisXYZ;
        [SerializeField]
        private GizmoHandle GizmoAxisX;
        [SerializeField]
        private GizmoHandle GizmoAxisY;
        [SerializeField]
        private GizmoHandle GizmoAxisZ;
        
        [HideInInspector]
        public GameObject MyTarget;

        public LayerMask MyLayer;

        private GizmoMode MyMode = GizmoMode.Translate;
        private GizmoDragType MyDragType;
        private Transform SelectedObject;
        private bool IsDragging = false;
        private Vector3 MouseBeginPosition;
        private float OriginalDistance;
        private Vector3 SelectedObjectPosition;
        private Vector3 OriginalScale;
        private Vector3 OriginalRotation;
        private bool IsLocal = false;
        private Vector3 MoveScale = new Vector3(10, 5, 10);
        private Ray RaycastRay;
        private RaycastHit RaycastHit;
        private GizmoHandle RaycastGizmoHandle;
        private Vector2 RaycastMousePosition;

        private bool WasRayHit;

        private void Start()
        {
            GizmoAxisXYZ.MyGizmo = this;
            GizmoAxisX.MyGizmo = this;
            GizmoAxisY.MyGizmo = this;
            GizmoAxisZ.MyGizmo = this;
        }

        private void Update()
        {
            Raycast();
            UpdateMouseDrag();
        }

        private void LateUpdate()
        {
            WasRayHit = false;
        }

        /// <summary>
        /// Begins the dragging
        /// </summary>
        private void Raycast()
        {
            if (!IsDragging && WasRayHit && Input.GetMouseButtonDown(0))
            {
                //Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                //RaycastHit MyHit;
                //if (Physics.Raycast(MyRay, out MyHit, 100, MyLayer))
                {
                    Debug.Log("Hit Gizmo: " + RaycastHit.collider.name);
                    SelectedObject = RaycastHit.collider.gameObject.transform;
                    MouseBeginPosition = RaycastMousePosition;
                    SelectedObjectPosition = SelectedObject.position;
                    OriginalScale = SelectedObject.localScale;
                    OriginalRotation = SelectedObject.rotation.eulerAngles;
                    OriginalDistance = Vector3.Distance(transform.position, RaycastRay.origin);  // original position
                    IsDragging = true;
                    if (RaycastHit.collider.name == "TranslateX")
                    {
                        MyDragType = GizmoDragType.DragX;
                    }
                    else if (RaycastHit.collider.name == "TranslateY")
                    {
                        MyDragType = GizmoDragType.DragY;
                    }
                    else if (RaycastHit.collider.name == "TranslateZ")
                    {
                        MyDragType = GizmoDragType.DragZ;
                    }
                    else if (RaycastHit.collider.name == "TranslateXYZ")
                    {
                        MyDragType = GizmoDragType.DragAll;
                    }
                }
            }
        }

        public void OnRayhit(GizmoHandle MyHandle, Ray MyRay, RaycastHit MyHit)
        {
            Debug.Log("Ray hit on gizmo handle: " + MyHandle.name);
            WasRayHit = true;
            RaycastRay = MyRay;
            RaycastHit = MyHit;
            RaycastGizmoHandle = MyHandle;
            RaycastMousePosition = Input.mousePosition;
        }

        private void AddToPosition(Vector3 PositionDelta)
        {
            Vector3 NewPosition = SelectedObjectPosition + PositionDelta;
            SetNewPosition(NewPosition);
        }

        private void SetNewPosition(Vector3 NewPosition)
        {
            MyTarget.transform.position = NewPosition;
            transform.position = NewPosition;
        }

        void UpdateMouseDrag()
        {
            if (Input.GetMouseButtonUp(0))
            {
                IsDragging = false;
            }
            if (IsDragging)
            {
                Vector3 MouseDifference = Input.mousePosition - MouseBeginPosition;
                MouseDifference.x /= Screen.width;
                MouseDifference.y /= Screen.height;
                if (MyDragType == GizmoDragType.DragX)
                {
                    // Moved difference by MouseDifference.x Pixels
                    // Convert the pixel difference to world position
                    if (MyMode == GizmoMode.Translate)
                    {
                        Vector3 MoveDifference;
                        if (IsLocal == true)
                        {
                            MoveDifference = SelectedObject.right * MouseDifference.x * OriginalDistance * MoveScale.x;
                        }
                        else
                        {
                            MoveDifference = new Vector3(MouseDifference.x * OriginalDistance * MoveScale.x, 0, 0);
                        }
                        AddToPosition(-MoveDifference);
                    }
                    else if (MyMode == GizmoMode.Scale)
                    {
                        Vector3 ScaleMultiplier = new Vector3(MouseDifference.x * OriginalDistance, 0, 0);
                        SelectedObject.localScale = OriginalScale + ScaleMultiplier;
                    }
                    else if (MyMode == GizmoMode.Rotate)
                    {
                        Vector3 NewAngle = OriginalRotation + -MouseDifference.x * Vector3.up * 180;
                        if (IsLocal)
                        {
                            //NewAngle = OriginalRotation + -MouseDifference.x * SelectedObject.up * 180;
                            MyTarget.transform.eulerAngles = NewAngle;
                        }
                        SelectedObject.eulerAngles = NewAngle;
                    }
                }
                else if (MyDragType == GizmoDragType.DragY)
                {
                    // Moved difference by MouseDifference.x Pixels
                    // Convert the pixel difference to world position
                    MouseDifference.x = 0; MouseDifference.z = 0;
                    //SelectedObject.transform.position = SelectedObjectPosition + MouseDifference;
                    if (MyMode == GizmoMode.Translate)
                    {
                        //MouseDifference.y *= OriginalDistance * 1;
                        //MyGizmo.position = SelectedObjectPosition + MouseDifference;
                        Vector3 MoveDifference = Vector3.up * MouseDifference.y * OriginalDistance * MoveScale.y;
                        if (IsLocal == true)
                        {
                            MoveDifference = SelectedObject.up * MouseDifference.y * OriginalDistance * MoveScale.y;
                        }
                        AddToPosition(MoveDifference);
                    }
                    else if (MyMode == GizmoMode.Scale)
                    {
                        MouseDifference.y *= OriginalDistance * 1;
                        SelectedObject.localScale = OriginalScale + MouseDifference;
                    }
                    else if (MyMode == GizmoMode.Rotate)
                    {
                        MouseDifference.y *= 180;
                        SelectedObject.eulerAngles = OriginalRotation + new Vector3(MouseDifference.y, 0, 0);
                        if (IsLocal)
                        {
                            MyTarget.transform.eulerAngles = OriginalRotation + new Vector3(MouseDifference.y, 0, 0);
                        }
                    }
                }
                else if (MyDragType == GizmoDragType.DragZ)
                {
                    // Moved difference by MouseDifference.x Pixels
                    // Convert the pixel difference to world position
                    //SelectedObject.transform.position = SelectedObjectPosition + MouseDifference;
                    if (MyMode == GizmoMode.Translate)
                    {
                        // MouseDifference.y *= OriginalDistance * 2;
                        //MyGizmo.position = SelectedObjectPosition + new Vector3(0,0, MouseDifference.y);
                        Vector3 MoveDifference = Vector3.forward * MouseDifference.y * OriginalDistance * MoveScale.z;
                        if (IsLocal == true)
                        {
                            MoveDifference = SelectedObject.forward * MouseDifference.y * OriginalDistance * MoveScale.z;
                        }
                        AddToPosition(-MoveDifference);
                    }
                    else if (MyMode == GizmoMode.Scale)
                    {
                        MouseDifference.y *= OriginalDistance * 1;
                        SelectedObject.localScale = OriginalScale + new Vector3(0, 0, MouseDifference.y);
                    }
                    else if (MyMode == GizmoMode.Rotate)
                    {
                        MouseDifference.x *= 180;
                        //MouseDifference.y *= 90;
                        Vector3 NewAngle = OriginalRotation + new Vector3(0, 0, -MouseDifference.x);
                        SelectedObject.eulerAngles = NewAngle;

                        if (IsLocal)
                        {
                            MyTarget.transform.eulerAngles = NewAngle;
                        }
                    }
                    //Debug.Log("Distance: " + OriginalDistance + "- MouseDifference: " + MouseDifference.x);
                }
                else if (MyDragType == GizmoDragType.DragAll)
                {
                    if (MyMode == GizmoMode.Translate)
                    {
                        //Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        SetNewPosition(RaycastRay.origin + RaycastRay.direction * OriginalDistance);
                    }
                    else if (MyMode == GizmoMode.Scale)
                    {
                        MouseDifference.y *= OriginalDistance * 1;
                        SelectedObject.localScale = OriginalScale + (new Vector3(1, 1, 1)) * MouseDifference.magnitude;
                    }
                }
            }
        }
    }
}
