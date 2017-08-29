using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum GizmoDragType
{
    None,
    DragAll,
    DragX,
    DragY,
    DragZ
}
[System.Serializable]
public enum GizmoMode
{
    None,
    Select,
    Translate,
    Rotate,
    Scale,
    Create
}

public class GizmoHandler : MonoBehaviour
{
    [Header("References")]
    private Transform SelectedObject;
    private Transform MyGizmo;
    [Header("Gizmo")]
    public GameObject GizmoPrefab;
    private GizmoMode MyMode;
    private GizmoDragType MyDragType;
    [Header("Cube")]
    public GameObject CompanionPrefab;
    //public GameObject CubePrefab;
    [Header("Raycasting")]
    public LayerMask MyLayer;
    public LayerMask MyDefaultLayer;

    private bool IsDragging = false;
    private Vector3 MouseBeginPosition;
    private float OriginalDistance;
    private Vector3 SelectedObjectPosition;
    private Vector3 OriginalScale;
    private Vector3 OriginalRotation;
    private bool IsLocal = false;

    private List<Transform> MySpawns = new List<Transform>();

    public void SpawnGizmoPosition()
    {
        if (MyGizmo != null)
        {
            Destroy(MyGizmo.gameObject);
            MyGizmo = null;
        }
        if (MyGizmo == null)
        {
            // Create 3 directions on SelectedObject
            MyGizmo = ((GameObject)Instantiate(GizmoPrefab, SelectedObject.position, SelectedObject.rotation)).transform;
            SetGizmoSize();
            //MyGizmo.SetParent(SelectedObject);
        }
        // GameObject GizmoTranslateX = (GameObject)Instantiate();
        //MyMode = GizmoMode.Translate;
    }

    void OnGUI()
    {
        Vector3 MyDirection = transform.forward;//(SelectedObject.position - transform.position).normalized*0.6f;
        /*if (GUILayout.Button("ZoomIn"))
        {
            transform.position += MyDirection;
        }
        if (GUILayout.Button("ZoomOut"))
        {
            transform.position -= MyDirection;
        }
        if (GUILayout.Button("Top"))
        {
            transform.eulerAngles = new Vector3(90, 0, 0);
        }
        if (GUILayout.Button("Front"))
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        if (GUILayout.Button("Left"))
        {
            transform.eulerAngles = new Vector3(0, 90, 0);
        }*/
        GUILayout.Label("Mode: " + MyMode.ToString());
        if (SelectedObject != null && MyGizmo != null)
        {
            GUILayout.Label("Position: " + MyGizmo.position.ToString());
            GUILayout.Label("Rotation: " + MyGizmo.rotation.eulerAngles.ToString());
            GUILayout.Label("Scale: " + MyGizmo.localScale.ToString());
            if (IsLocal)
            {
                if (GUILayout.Button("Local"))
                {
                    IsLocal = false;
                }
            }
            else
            {
                if (GUILayout.Button("World"))
                {
                    IsLocal = true;
                }
            }
            if (GUILayout.Button("Bam"))
            {
                for (int i = 0; i < MySpawns.Count; i++)
                {
                    Rigidbody MyRigid = MySpawns[i].gameObject.GetComponent<Rigidbody>();
                    if (MyRigid == null)
                    {
                        MyRigid = MySpawns[i].gameObject.AddComponent<Rigidbody>();
                    }
                    MyRigid.useGravity = !MyRigid.useGravity;
                    if (MyRigid.useGravity)
                    {
                        MyRigid.AddForce(new Vector3(Random.Range(-20, 20), 1000, Random.Range(-20, 20)));
                    }
                }
            }
        }
    }
    public float LerpSpeed = 15;


    private void SetGizmoSize()
    {
        float GizmoDistance = Vector3.Distance(transform.position, MyGizmo.position);
        MyGizmo.localScale = (new Vector3(1, 1, 1)) * GizmoDistance / 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            MyMode = GizmoMode.None;
            if (MyGizmo != null)
            {
                Destroy(MyGizmo.gameObject);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MyMode = GizmoMode.Select;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MyMode = GizmoMode.Translate;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            MyMode = GizmoMode.Scale;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            MyMode = GizmoMode.Rotate;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            MyMode = GizmoMode.Create;
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (SelectedObject != null)
            {
                Destroy(SelectedObject.gameObject);
                Destroy(MyGizmo.gameObject);
            }
        }

        if (SelectedObject != null && MyGizmo != null)
        {
            SelectedObject.position = Vector3.Lerp(SelectedObject.position, MyGizmo.position, Time.deltaTime * LerpSpeed * 2);
            if (IsLocal == false)   // if world position / rotation
            {
                MyGizmo.rotation = Quaternion.Lerp(MyGizmo.rotation, Quaternion.identity, Time.deltaTime * LerpSpeed);
            }
            else
            {
                MyGizmo.rotation = Quaternion.Lerp(MyGizmo.rotation, SelectedObject.rotation, Time.deltaTime * LerpSpeed);
            }
            SetGizmoSize();
        }

        HandleMouseClick();
        if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;
        }
        UpdateMouseDrag();
	}

    private void SpawnCube(Vector3 MySpawnPosition)
    {
        GameObject NewCube = (GameObject)Instantiate(CompanionPrefab, Vector3.zero, Quaternion.identity);
        SelectedObject = NewCube.transform;
        SelectedObject.position = MySpawnPosition + Vector3.up;
        SpawnGizmoPosition();   // make sure gizmo
        MyGizmo.position = SelectedObject.position;
        MyMode = GizmoMode.Translate;
        MySpawns.Add(SelectedObject);
    }
    /// <summary>
    /// When user clicks the mouse
    /// </summary>
    void HandleMouseClick()
    {
        if (Camera.main != null)
        {
            Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit MyHit;
            if (Input.GetMouseButtonDown(0))
            {
                if (MyMode == GizmoMode.Select)
                {
                    if (Physics.Raycast(MyRay, out MyHit, 100, MyDefaultLayer))
                    {
                        Debug.Log("Selecting Object: " + MyHit.collider.name);
                        SelectedObject = MyHit.collider.transform;
                        SpawnGizmoPosition();
                    }
                }
                else if (MyMode == GizmoMode.Create)
                {
                    if (Physics.Raycast(MyRay, out MyHit, 100, MyDefaultLayer))
                    {
                        Debug.Log("Selecting Object: " + MyHit.collider.name);
                        //SelectedObject = MyHit.collider.transform;
                        //SpawnGizmoPosition();
                        if (MyHit.collider.gameObject.tag == "Ground")
                        {
                            SpawnCube(MyHit.point);
                        }
                    }
                }
            }
            if (Input.GetMouseButton(0) && !IsDragging)
            {
                if (Physics.Raycast(MyRay, out MyHit, 100, MyLayer))
                {
                    Debug.Log("Hit Gizmo: " + MyHit.collider.name);
                    MouseBeginPosition = Input.mousePosition;
                    SelectedObjectPosition = SelectedObject.position;
                    OriginalScale = SelectedObject.localScale;
                    OriginalRotation = SelectedObject.rotation.eulerAngles;
                    OriginalDistance = Vector3.Distance(transform.position, SelectedObjectPosition);  // original position
                    IsDragging = true;
                    if (MyHit.collider.name == "TranslateX")
                    {
                        MyDragType = GizmoDragType.DragX;
                    }
                    else if (MyHit.collider.name == "TranslateY")
                    {
                        MyDragType = GizmoDragType.DragY;
                    }
                    else if (MyHit.collider.name == "TranslateZ")
                    {
                        MyDragType = GizmoDragType.DragZ;
                    }
                    else if (MyHit.collider.name == "TranslateXYZ")
                    {
                        MyDragType = GizmoDragType.DragAll;
                    }
                }
            }

        }
    }

    void UpdateMouseDrag()
    {
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
                    Vector3 MoveDifference = new Vector3(MouseDifference.x * OriginalDistance * 2, 0, 0);
                    if (IsLocal == true)
                    {
                        MoveDifference = SelectedObject.right * MouseDifference.x * OriginalDistance * 2;
                    }
                    MyGizmo.position = SelectedObjectPosition + MoveDifference;
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
                        MyGizmo.eulerAngles = NewAngle;
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
                    Vector3 MoveDifference = Vector3.up * MouseDifference.y * OriginalDistance * 1;
                    if (IsLocal == true)
                    {
                        MoveDifference = SelectedObject.up * MouseDifference.y * OriginalDistance * 2;
                    }
                    MyGizmo.position = SelectedObjectPosition + MoveDifference;
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
                        MyGizmo.eulerAngles = OriginalRotation + new Vector3(MouseDifference.y, 0, 0);
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
                    Vector3 MoveDifference = Vector3.forward * MouseDifference.y * OriginalDistance * 2;
                    if (IsLocal == true)
                    {
                        MoveDifference = SelectedObject.forward * MouseDifference.y * OriginalDistance * 2;
                    }
                    MyGizmo.position = SelectedObjectPosition + MoveDifference;
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
                        MyGizmo.eulerAngles = NewAngle;
                    }
                }
                //Debug.Log("Distance: " + OriginalDistance + "- MouseDifference: " + MouseDifference.x);
            }
            else if (MyDragType == GizmoDragType.DragAll)
            {
                if (MyMode == GizmoMode.Translate)
                {
                    Ray MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    MyGizmo.position = MyRay.origin + MyRay.direction * OriginalDistance;
                }
                else if (MyMode == GizmoMode.Scale)
                {
                    MouseDifference.y *= OriginalDistance * 1;
                    SelectedObject.localScale = OriginalScale + (new Vector3(1,1,1))* MouseDifference.magnitude;
                }
            }
        }
    }
}
