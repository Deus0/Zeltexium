using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.AnimationUtilities
{
    /// <summary>
    /// A line, used primarily for grid overlays.
    /// </summary>
    public class GridOverlayLine
    {
        public LineRenderer MyLine;
        public Vector3 Position1;
        public Vector3 Position2;
        public GridOverlayLine(LineRenderer NewLine, Vector3 NewPosition1, Vector3 NewPosition2)
        {
            MyLine = NewLine;
            Position1 = NewPosition1;
            Position2 = NewPosition2;
        }
    }

    /// <summary>
    /// Used to be drawn around worlds, but also used in skeleton editor.
    /// </summary>
    public class GridOverlay : MonoBehaviour
    {
        #region Variables
        public bool IsGridShown;
        List<GridOverlayLine> MyLines = new List<GridOverlayLine>();
        [Header("Debug")]
        bool DebugGrid = false;
        [SerializeField] private bool DoGenerateLines = false;
        public bool IsClear = false;
        [Header("Material")]
        public Material MainMaterial;
        public Material SubMaterial;
        public Color32 MainColor = new Color(0f, 1f, 0f, 1f);
        public Color32 SubColor = new Color(0f, 0.5f, 0f, 1f);
        [Header("Size")]
        public bool IsCentred = true;
        public Int3 GridSizeMin = new Int3();
        public Int3 GridSizeMax = new Int3(64,16,64);
        public float Thickness = 0.03f;
        [Header("Steps")]
        public float LargeStep = 16;
        public float SmallStep = 4;
        public bool IsScaleSize = false;
        public bool IsJustFloor = false;
        #endregion

        void Update()
        {
            if (DoGenerateLines)
            {
                DoGenerateLines = false;
                Clear();
                Generate();
            }
            if (IsClear)
            {
                IsClear = false;
                Clear();
            }
            AnimateGrid();
        }
        #region GettersAndSetters
        public bool IsForce;

        private void SetSize(Int3 NewSize)
        {
            GridSizeMin = Int3.Zero();
            GridSizeMax = NewSize;
        }
        public void SetState(bool NewState, Int3 NewSize)
        {
            SetSize(NewSize);
            SetState(NewState);
        }
        public void SetState(bool NewState)
        {
            if (IsGridShown != NewState || IsForce)
            {
                IsForce = false;
                Clear();
                IsGridShown = NewState;
                if (IsGridShown)
                {
                    Generate();
                }
            }
        }
        public bool GetState()
        {
            return IsGridShown;
        }
        #endregion
        public void GenerateLines(Int3 NewSize)
        {
            SetSize(NewSize);
            Clear();
            Generate();
        }
        public void SpawnForWorld(Int3 WorldSize)
        {
            WorldSize.x *= Voxels.Chunk.ChunkSize;
            WorldSize.y *= Voxels.Chunk.ChunkSize;
            WorldSize.z *= Voxels.Chunk.ChunkSize;
            if (IsCentred)
            {
                GridSizeMin = 0.5f*WorldSize;
                GridSizeMax = 1.5f*WorldSize;
            }
            else
            {
                GridSizeMin = 0 * WorldSize;
                GridSizeMax = 1 * WorldSize;
            }
            Clear();
            Generate();
        }

        private void AnimateGrid()
        {
            for (int i = 0; i < MyLines.Count; i++)
            {
                if (MyLines[i] == null || MyLines[i].MyLine == null)
                {
                    Clear();
                    Generate();
                    return;
                }
                // update line positions
                Vector3 Position1 = transform.TransformPoint(MyLines[i].Position1);
                Vector3 Position2 = transform.TransformPoint(MyLines[i].Position2);
                MyLines[i].MyLine.SetPosition(0, Position1);
                MyLines[i].MyLine.SetPosition(1, Position2);
            }
        }

        void Generate()
        {
			if (MainMaterial == null || SubMaterial == null) 
			{
				MainMaterial = new Material(Shader.Find("Standard"));
				SubMaterial = new Material(Shader.Find("Standard"));
			}
            DoTheLines();
        }
        void Clear()
        {
            for (int i = MyLines.Count-1; i >= 0; i--)
            {
                if (MyLines[i] != null && MyLines[i].MyLine != null)
                    DestroyImmediate(MyLines[i].MyLine.gameObject);
            }
            MyLines.Clear();
            for (int i = transform.childCount-1; i >= 0; i--)
            {
                if (transform.GetChild(i).name.Contains("Line "))
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        void CreateLine(Vector3 Position1, Vector3 Position2, Color32 LineColor, float Thickness)
        {
            GameObject NewLine = new GameObject();
            NewLine.layer = gameObject.layer;
            NewLine.transform.position = Position2 - Position1;
            NewLine.name = "Line " + transform.childCount;
            NewLine.transform.SetParent(transform, false);
            LineRenderer MyLineRender = NewLine.AddComponent<LineRenderer>();
            MyLineRender.receiveShadows = false;
            MyLineRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (LineColor.r == MainColor.r && LineColor.g == MainColor.g && MainColor.b == LineColor.b)
                MyLineRender.sharedMaterial = MainMaterial;
            else
                MyLineRender.sharedMaterial = SubMaterial;
            if (MyLineRender.sharedMaterial)
                MyLineRender.sharedMaterial.color = LineColor;
            MyLineRender.SetPosition(0, Position1);
            MyLineRender.SetPosition(1, Position2);
            if (IsScaleSize)
            {
                MyLineRender.startWidth = Thickness;
                MyLineRender.endWidth = Thickness;
            }
            else
            {
                MyLineRender.startWidth = Thickness * transform.lossyScale.x;
                MyLineRender.endWidth = MyLineRender.startWidth;
            }
            MyLines.Add(new GridOverlayLine(MyLineRender, Position1, Position2));
        }
        void DoTheLines()
        {
            if (SmallStep < 1)
                SmallStep = 1;
            if (LargeStep < SmallStep)
                LargeStep = SmallStep;
            DrawNormalGrid(GridSizeMin.y);
            if (!IsJustFloor)
            {
                DrawNormalGrid(GridSizeMax.y);
                DrawFrontGrid(GridSizeMin.z);
                DrawFrontGrid(GridSizeMax.z);
                DrawLeftGrid(GridSizeMin.x);
                DrawLeftGrid(GridSizeMax.x);
            }
        }
        public bool IsCentredXZ;
        void DoTheLine(Vector3 Position1, Vector3 Position2)
        {
            if (DebugGrid)
            {
                Gizmos.DrawLine((Position1), (Position2));
            }
            else
            {
                if (IsCentred)
                {
                    Position1 -= (GridSizeMax - GridSizeMin).GetVector() / 2f;
                    Position2 -= (GridSizeMax - GridSizeMin).GetVector() / 2f;
                }
                else if (IsCentredXZ)
                {
                    float PositionY1 = Position1.y;
                    float PositionY2 = Position2.y;
                    Position1 -= (GridSizeMax - GridSizeMin).GetVector() / 2f;
                    Position2 -= (GridSizeMax - GridSizeMin).GetVector() / 2f;
                    Position1.y = PositionY1;
                    Position2.y = PositionY2;
                }
                if (IsScaleSize)
                {
                    Position1 = new Vector3(
                        Position1.x / transform.lossyScale.x,
                        Position1.y / transform.lossyScale.y,
                        Position1.z / transform.lossyScale.z);
                    Position2 = new Vector3(
                        Position2.x / transform.lossyScale.x,
                        Position2.y / transform.lossyScale.y,
                        Position2.z / transform.lossyScale.z);

                }
                if (Gizmos.color == MainColor)
                    CreateLine(Position1, Position2, Gizmos.color, Thickness*2f);
                else
                    CreateLine(Position1, Position2, Gizmos.color, Thickness);
            }
        }
        // draw the grid :) 
        void OnDrawGizmos()
        {
            if (DebugGrid)
            {
                // orient to the gameobject, so you can rotate the grid independently if desired
                Gizmos.matrix = transform.localToWorldMatrix;
                DoTheLines();
            }
        }
        void DrawNormalGrid(float PositionY)
        {
            // draw the horizontal lines
            for (float x = GridSizeMin.x; x <= GridSizeMax.x; x += SmallStep)
            {
                Gizmos.color = (x % LargeStep == 0 ? MainColor : SubColor);
                if (x == 0)
                    Gizmos.color = MainColor;

                Vector3 pos1 = new Vector3(x, PositionY, GridSizeMin.z);
                Vector3 pos2 = new Vector3(x, PositionY, GridSizeMax.z);
                DoTheLine((pos1), (pos2));
            }

            // draw the vertical lines
            for (float z = GridSizeMin.z; z <= GridSizeMax.z; z += SmallStep)
            {
                Gizmos.color = (z % LargeStep == 0 ? MainColor : SubColor);
                if (z == 0)
                    Gizmos.color = MainColor;
                Vector3 pos1 = new Vector3(GridSizeMin.x, PositionY, z);
                Vector3 pos2 = new Vector3(GridSizeMax.x, PositionY, z);
                DoTheLine((pos1), (pos2));
            }
        }
        void DrawLeftGrid(float PositionX)
        {
            // draw the horizontal lines
            for (float y = GridSizeMin.y; y <= GridSizeMax.y; y += SmallStep)
            {
                Gizmos.color = (y % LargeStep == 0 ? MainColor : SubColor);
                if (y == 0)
                    Gizmos.color = MainColor;

                Vector3 pos1 = new Vector3(PositionX, y, GridSizeMin.z);
                Vector3 pos2 = new Vector3(PositionX, y, GridSizeMax.z);
                DoTheLine((pos1), (pos2));
            }

            // draw the vertical lines
            for (float z = GridSizeMin.z; z <= GridSizeMax.z; z += SmallStep)
            {
                Gizmos.color = (z % LargeStep == 0 ? MainColor : SubColor);
                if (z == 0)
                    Gizmos.color = MainColor;
                Vector3 pos1 = new Vector3(PositionX, GridSizeMin.y, z);
                Vector3 pos2 = new Vector3(PositionX, GridSizeMax.y, z);
                DoTheLine((pos1), (pos2));
            }
        }
        void DrawFrontGrid(float PositionZ)
        {
            // draw the horizontal lines
            for (float x = GridSizeMin.x; x <= GridSizeMax.x; x += SmallStep)
            {
                Gizmos.color = (x % LargeStep == 0 ? MainColor : SubColor);
                if (x == 0)
                    Gizmos.color = MainColor;

                Vector3 pos1 = new Vector3(x, GridSizeMin.y, PositionZ);
                Vector3 pos2 = new Vector3(x, GridSizeMax.y, PositionZ);
                DoTheLine((pos1), (pos2));
            }

            // draw the vertical lines
            for (float y = GridSizeMin.z; y <= GridSizeMax.y; y += SmallStep)
            {
                Gizmos.color = (y % LargeStep == 0 ? MainColor : SubColor);
                if (y == 0)
                    Gizmos.color = MainColor;
                Vector3 pos1 = new Vector3(GridSizeMin.x, y, PositionZ);
                Vector3 pos2 = new Vector3(GridSizeMax.x, y, PositionZ);
                DoTheLine((pos1), (pos2));
            }
        }
    }
}