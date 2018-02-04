using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Util;
using MakerGuiSystem;

namespace Zeltex.Guis
{
    /// <summary>
    /// Generates Handlers to view the UV coordinates of a mesh
    /// To do:
    /// - generate the uv handlers
    /// - Show only one tile or show all tiles connected to the voxel model
    /// </summary>
    public class UVViewer : MonoBehaviour
    {
        #region Variables
        public VoxelPolygonViewer MyVoxelViewer;
        public Material MyMaterial;
        public InputField InputU;
        public InputField InputV;
        public Vector2 UVHandlerSize = new Vector2(16, 16);
        public bool IsGridPosition = true;
        public float GridLength = 16;
        List<Vector2> MyUVs = new List<Vector2>();
        List<GameObject> MyHandlers = new List<GameObject>();
        List<GameObject> SelectedHandlers = new List<GameObject>();
        //int SelectedUV = 0;
        List<int> VertIndexes = new List<int>();
        [Header("Colours")]
        public Color32 NormalColor = Color.white;
        public Color32 SelectedColor = Color.red;
        #endregion

        #region Selection
        /// <summary>
        /// Called by the UVHandler when mouse is clicked initially!!
        /// </summary>
        public void Select(GameObject MyUVHandler)
        {
            for (int i = 0; i < SelectedHandlers.Count; i++)
            {
                SelectedHandlers[i].GetComponent<UVHandler>().Deselect();
            }
            SelectedHandlers.Clear();
            SelectedHandlers.Add(MyUVHandler);
            OnUpdateUV(MyUVHandler);
        }
        public void OnDrag(GameObject MyUVHandler)
        {
            OnUpdateUV(MyUVHandler);
        }
        public void Release(GameObject MyUVHandler)
        {
            // Just Apply Mesh update to uvs now
            for (int i = 0; i < MyHandlers.Count; i++)
            {
                if (MyUVHandler == MyHandlers[i])
                {
                    Vector2 MyUVPosition = RectPositionToUV(MyUVHandler.GetComponent<RectTransform>().anchoredPosition);    // - UVHandlerSize/2f
                    MyVoxelViewer.GetSpawn().GetComponent<Zeltex.Voxels.PolyModelHandle>().UpdateTextureCoordinate(
                        VertIndexes[i],
                        MyUVPosition);
                    break;
                }
            }
            MyVoxelViewer.GetSpawn().GetComponent<Zeltex.Voxels.PolyModelHandle>().OnMassUpdatedModel();
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Clears all the UV handlers
        /// </summary>
        public void ClearHandlers()
        {
            for (int i = 0; i < MyHandlers.Count; i++)
            {
                MyHandlers[i].Die();
            }
            MyHandlers.Clear();
            SelectedHandlers.Clear();
        }
        public void GenerateHandlers(List<Vector2> TextureCoordinates, List<int> VertIndexes_, List<string> TextureNames)
        {
            VertIndexes = VertIndexes_;
            //Debug.Log("Generating: " + TextureCoordinates.Count + " uv handlers");
            ClearHandlers();
            MyUVs.Clear();
            MyUVs.AddRange(TextureCoordinates);
            for (int i = 0; i < TextureCoordinates.Count; i++)
            {
                GenerateHandler(TextureCoordinates[i]);
            }
            /*for (int i = 0; i < TextureNames.Count; i++)
            {
                if 
            }*/
            //if (TextureNames.Count > 0)
            //{
            //    GetComponent<RawImage>().texture = TextureMaker.Get().GetVoxelTexture(TextureNames[0]);
            //}
        }

        public void GenerateHandler(Vector2 Position)
        {
            //Debug.Log("Generating uv at new position: " + Position.ToString());
            GameObject NewHandler = new GameObject();
            UVHandler MyHandler = NewHandler.AddComponent<UVHandler>();
            MyHandler.MyViewer = this;
            NewHandler.transform.SetParent(transform);
            NewHandler.name = "UVHandler_" + MyHandlers.Count;
            MyHandlers.Add(NewHandler);
            RectTransform MyRect = NewHandler.AddComponent<RectTransform>();
            MyRect.SetAnchors(new Vector2(0, 0));
            MyRect.localScale = new Vector3(1, 1, 1);
            NewHandler.transform.localRotation = Quaternion.identity;
            MyRect.anchoredPosition3D = LimitHandlerPosition(UVToRectPosition(Position));
            MyRect.SetSize(UVHandlerSize);
            RawImage MyImage = NewHandler.AddComponent<RawImage>();
            MyImage.material = MyMaterial;
            MyHandler.SetColors(NormalColor, SelectedColor);
        }
        #endregion
        #region Positioning

        public Vector2 UVToRectPosition(Vector2 MyUVPosition)
        {
            MyUVPosition = new Vector2(
                   (MyUVPosition.x) * GetComponent<RectTransform>().sizeDelta.x,
                   (MyUVPosition.y) * GetComponent<RectTransform>().sizeDelta.y);
            return MyUVPosition;
        }
        /// <summary>
        /// Converts from UV position to anchored position
        /// </summary>
        public Vector2 LimitHandlerPosition(Vector2 MyPosition)
        {
            if (IsGridPosition)
            {
                MyPosition.x = GridLength * (float)Mathf.RoundToInt(MyPosition.x / GridLength); // GridLength / 2f + 
                MyPosition.y = GridLength * (float)Mathf.RoundToInt(MyPosition.y / GridLength);
                MyPosition.x = Mathf.Clamp(MyPosition.x, 0, GetComponent<RectTransform>().sizeDelta.x);
                MyPosition.y = Mathf.Clamp(MyPosition.y, 0, GetComponent<RectTransform>().sizeDelta.y);
            }
            return MyPosition;
        }
        Vector2 RectPositionToUV(Vector2 MyRectPosition)
        {
            MyRectPosition.x /= GetComponent<RectTransform>().GetWidth();
            MyRectPosition.y /= GetComponent<RectTransform>().GetHeight();
            return MyRectPosition;
        }
        #endregion
        #region UI
        public void UseInput(InputField MyInput)
        {
            if (SelectedHandlers.Count > 0)
            {
                if (MyInput.name == "InputU")
                {
                    SelectedHandlers[SelectedHandlers.Count - 1].GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(
                            float.Parse(MyInput.text) * GetComponent<RectTransform>().GetWidth(),
                            SelectedHandlers[SelectedHandlers.Count - 1].GetComponent<RectTransform>().anchoredPosition.y
                            );
                    Release(SelectedHandlers[SelectedHandlers.Count - 1]);
                }
                else if (MyInput.name == "InputV")
                {
                    SelectedHandlers[SelectedHandlers.Count - 1].GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(
                            SelectedHandlers[SelectedHandlers.Count - 1].GetComponent<RectTransform>().anchoredPosition.x,
                            float.Parse(MyInput.text) * GetComponent<RectTransform>().GetHeight()
                            );
                    Release(SelectedHandlers[SelectedHandlers.Count - 1]);
                }
            }
        }
        public void OnUpdateUV(GameObject MyUV)
        {
            Vector2 MyUVPosition = MyUV.GetComponent<RectTransform>().anchoredPosition;
            MyUVPosition = LimitHandlerPosition(MyUVPosition);
            MyUV.GetComponent<RectTransform>().anchoredPosition3D = MyUVPosition;
            MyUVPosition = RectPositionToUV(MyUVPosition);
            OnUpdateUV(MyUVPosition);
        }
        public void OnUpdateUV(Vector2 MyUV)
        {
            InputU.text = "" + MyUV.x;
            InputV.text = "" + MyUV.y;
        }
        #endregion
    }
}