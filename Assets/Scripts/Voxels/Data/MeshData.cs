using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Voxels 
{
	/// <summary>
    /// The main data structure for a mesh
    /// </summary>
	[System.Serializable]
	public class MeshData 
	{
        #region Variabes
        [SerializeField]
        public List<Vector3> Verticies = new List<Vector3>();
		[SerializeField]
        public List<int> Triangles = new List<int>();
		[SerializeField]
        public List<Vector2> TextureCoordinates = new List<Vector2>();
		[SerializeField]
        public List<Color32> Colors = new List<Color32>();
        #endregion

        #region Init
        public MeshData()
        {

        }
        /// <summary>
        /// Create a mesh data from a base mesh
        /// </summary>
        public MeshData(Mesh MyMesh)
        {
            for (int i = 0; i < MyMesh.vertices.Length; i++)
            {
                Verticies.Add(MyMesh.vertices[i] + new Vector3(0.5f, 0.5f, 0.5f));
            }
            int[] MyIndicies = MyMesh.GetIndices(0);
            for (int i = 0; i < MyIndicies.Length; i++)
            {
                Triangles.Add(MyIndicies[i]);
            }
            for (int i = 0; i < MyMesh.uv.Length; i++)
            {
                TextureCoordinates.Add(MyMesh.uv[i]);
            }
            for (int i = 0; i < MyMesh.colors32.Length; i++)
            {
                Colors.Add(MyMesh.colors32[i]);
            }
        }
        public MeshData(List<MeshData> MyData)
        {
            for (int a = 0; a < MyData.Count; a++)
            {
                int TrianglesBuffer = Verticies.Count;
                for (int i = 0; i < MyData[a].Verticies.Count; i++)
                {
                    Verticies.Add(MyData[a].Verticies[i]);
                }
                for (int i = 0; i < MyData[a].Triangles.Count; i++)
                {
                    Triangles.Add(TrianglesBuffer + MyData[a].Triangles[i]);
                }
                for (int i = 0; i < MyData[a].TextureCoordinates.Count; i++)
                {
                    TextureCoordinates.Add(MyData[a].TextureCoordinates[i]);
                }
                for (int i = 0; i < MyData[a].Colors.Count; i++)
                {
                    Colors.Add(MyData[a].Colors[i]);
                }
            }
        }
        #endregion

        #region Utility
        int TrianglesBuffer;
        public MeshData AddDataMesh;

        /// <summary>
        /// Combines this mesh with another one
        /// </summary>
        public void Add()
        {
            TrianglesBuffer = Verticies.Count;
            //Debug.Log("Current: " + Verticies.Count + " -Adding mesh data: " + NewMesh.Verticies.Count);
            Verticies.AddRange(AddDataMesh.Verticies);
            TextureCoordinates.AddRange(AddDataMesh.TextureCoordinates);
            Colors.AddRange(AddDataMesh.Colors);
            for (i = 0; i < AddDataMesh.Triangles.Count; i++)
            {
                Triangles.Add(TrianglesBuffer + AddDataMesh.Triangles[i]);
            }
            AddDataMesh = null;
        }
        /// <summary>
        /// Multiply Every vertex another vector
        /// </summary>
        /// <param name="MultiplyValue"></param>
        public void MultiplyVerts(Vector3 MultiplyValue)
        {
            for (int i = 0; i < Verticies.Count; i++)
            {
                Verticies[i] = new Vector3(
                    Verticies[i].x * MultiplyValue.x,
                    Verticies[i].y * MultiplyValue.y,
                    Verticies[i].z * MultiplyValue.z);
            }
        }

        int i;
        /// <summary>
        /// Add to every vertex another vector
        /// </summary>
        public void AddToVertex(Vector3 Addition)
        {
            for (i = 0; i < Verticies.Count; i++)
            {
                Verticies[i] += (Addition);
            }
        }
        /// <summary>
        /// Multiplies all the colours by a colour
        /// </summary>
        /// <param name="MyColor"></param>
        public void Tint(Color MyColor)
        {
            for (int i = 0; i < Colors.Count; i++)
            {
                Colors[i] = MyColor * Colors[i];
            }
        }
        #endregion

        #region Getters
        /// <summary>
        /// Returns a Mesh data using this meshData as the data
        /// </summary>
        /// <returns></returns>
        public Mesh GetMesh()
        {
            Mesh MyMesh = new Mesh();
            MyMesh.vertices = Verticies.ToArray();
            MyMesh.triangles = Triangles.ToArray();
            MyMesh.uv = TextureCoordinates.ToArray();
            MyMesh.colors32 = Colors.ToArray();
            MyMesh.subMeshCount = 1;
            MyMesh.RecalculateNormals();
            return MyMesh;
        }

        public Vector3 GetSize()
        {
            if (Verticies.Count == 0)
                return new Vector3();
            Vector3 Min = Verticies[0];
            Vector3 Max = Verticies[0];
            for (int i = 1; i < Verticies.Count; i++)
            {
                if (Verticies[i].magnitude < Min.magnitude)
                {
                    Min = Verticies[i];
                }
                else if (Verticies[i].magnitude > Max.magnitude)
                {
                    Max = Verticies[i];
                }
            }
            return Max - Min;
        }
        public bool HasData()
        {
			return (Verticies.Count > 0);
		}
		public Vector3[] GetVerticies() 
		{
			return Verticies.ToArray ();
		}

		public Vector2[] GetTextureCoordinates() 
		{
			return TextureCoordinates.ToArray ();
		}

		public int[] GetTriangles()
		{
			return Triangles.ToArray ();
        }
        public List<Color32> GetColors()
        {
            return Colors;
        }
        #endregion

        #region Setters
        public void SetColor(int Index, Color32 NewColor) 
		{
            if (Index >= 0 && Index < Colors.Count)
                Colors[Index] = NewColor;
            //else
            //    Debug.LogError("Index outside of bounds: " + Index + " - max: " + Colors.Count);
		}
        #endregion

        #region Data
        /// <summary>
        /// Clear the mesh of all its data
        /// </summary>
        public void Clear() 
		{
			Verticies.Clear ();
			Triangles.Clear ();
			TextureCoordinates.Clear ();
			Colors.Clear ();
		}

		public void AddTriangle(Vector3 Vertex1, Vector3 Vertex2, Vector3 Vertex3) 
		{
			Verticies.Add (Vertex1);
			Verticies.Add (Vertex2);
			Verticies.Add (Vertex3);
			Triangles.Add(Verticies.Count - 3);
			Triangles.Add(Verticies.Count - 2);
			Triangles.Add(Verticies.Count - 1);
		}
        public void AddQuadUVs(int VoxelIndex, int TileResolution, float TextureResolution, int SideIndex)
        {
            AddQuadUVs(VoxelIndex, TileResolution, TextureResolution, SideIndex, TextureCoordinates);
        }

        public void AddQuadUVs() 
		{
			TextureCoordinates.Add (new Vector2 (0, 0));
			TextureCoordinates.Add (new Vector2 (1, 0));
			TextureCoordinates.Add (new Vector2 (1, 1));
			TextureCoordinates.Add (new Vector2 (0, 1));
		}
		public void AddQuadColours(byte Brightness) 
		{
			for (int i = 0; i < 4; i++)
            {
                AddColor(new Color32(Brightness, Brightness, Brightness, (byte)(255)));
            }
		}

		public void AddQuadTriangles()
		{
			Triangles.Add(Verticies.Count - 4);
			Triangles.Add(Verticies.Count - 3);
			Triangles.Add(Verticies.Count - 2);
			
			Triangles.Add(Verticies.Count - 4);
			Triangles.Add(Verticies.Count - 2);
			Triangles.Add(Verticies.Count - 1);
        }
        public void AddColor(Color32 NewColor)
        {
            Colors.Add(NewColor);
        }

        public void AddVertex(Vector3 NewVertex)
        {
            Verticies.Add(NewVertex);
        }

        public void AddTriangle(int tri)
        {
            Triangles.Add(tri);
        }
        public void RotateTextureCoordinates()
        {
            RotateTextureCoordinates(TextureCoordinates);
        }
        #endregion

        #region File
        /// <summary>
        /// Gets the data in a script form
        /// </summary>
        public List<string> GetScript()
        {
            List<string> MyScript = new List<string>();
            if (Verticies.Count > 0)
            {
                MyScript.Add("/BeginVerticies");
                for (int i = 0; i < Verticies.Count; i++)
                {
                    MyScript.Add(Verticies[i].x + " " + Verticies[i].y + " " + Verticies[i].z);
                }
                MyScript.Add("/EndVerticies");
            }
            if (Triangles.Count > 0)
            {
                MyScript.Add("/BeginTriangles");
                for (int i = 0; i < Triangles.Count; i++)
                {
                    MyScript.Add("" + Triangles[i]);
                }
                MyScript.Add("/EndTriangles");
            }
            if (TextureCoordinates.Count > 0)
            {
                MyScript.Add("/BeginCoordinates");
                for (int i = 0; i < TextureCoordinates.Count; i++)
                {
                    MyScript.Add(TextureCoordinates[i].x + " " + TextureCoordinates[i].y);
                }
                MyScript.Add("/EndCoordinates");
            }
            if (Colors.Count > 0)
            {
                MyScript.Add("/BeginColors");
                for (int i = 0; i < Colors.Count; i++)
                {
                    MyScript.Add(Colors[i].r + " " + Colors[i].g + " " + Colors[i].b);
                }
                MyScript.Add("/EndColors");
            }
            return MyScript;
        }

        /// <summary>
        /// Loads the data from a script
        /// </summary>
        public void RunScript(List<string> MyScript)
        {
            //Debug.LogError("Loading Mesh with: " + MyScript.Count + " lines.\n" + Zeltex.Util.FileUtil.ConvertToSingle(MyScript));
            for (int i = 0; i < MyScript.Count; i++)
            {
                string MyLine = Zeltex.Util.ScriptUtil.RemoveWhiteSpace(MyScript[i]);
                if (MyLine == "/BeginVerticies")
                {
                    //Debug.LogError("BeginVerticies: " + i);
                    i++;
                    for (int j = i; j < MyScript.Count; j++)
                    {
                        if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(MyScript[j]) == "/EndVerticies")
                        {
                            i = j;
                            //Debug.LogError("EndVerticies: " + j);
                            break;
                        }
                        string[] MyVertThings = MyScript[j].Split(' ');
                        if (MyVertThings.Length == 3)
                        {
                            try
                            {
                                Verticies.Add(new Vector3(
                                    float.Parse(MyVertThings[0]),
                                    float.Parse(MyVertThings[1]),
                                    float.Parse(MyVertThings[2])));
                            }
                            catch (System.FormatException e)
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                    }
                }

                if (MyLine == "/BeginTriangles")
                {
                    i++;
                    for (int j = i; j < MyScript.Count; j++)
                    {
                        if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(MyScript[j]) == "/EndTriangles")
                        {
                            i = j;
                            break;
                        }
                        try
                        {
                            Triangles.Add(int.Parse(MyScript[j]));
                        }
                        catch (System.FormatException e)
                        {
                            Debug.LogError(e.ToString());
                        }
                    }
                }
                if (MyLine == "/BeginCoordinates")
                {
                    i++;
                    for (int j = i; j < MyScript.Count; j++)
                    {
                        if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(MyScript[j]) == "/EndCoordinates")
                        {
                            i = j;
                            break;
                        }
                        string[] MyVertThings = MyScript[j].Split(' ');
                        if (MyVertThings.Length == 2)
                        {
                            try
                            {
                                TextureCoordinates.Add(new Vector2(
                                    float.Parse(MyVertThings[0]),
                                    float.Parse(MyVertThings[1])));
                            }
                            catch (System.FormatException e)
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                    }
                }
                if (MyLine == "/BeginColors")
                {
                    i++;
                    for (int j = i; j < MyScript.Count; j++)
                    {
                        if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(MyScript[j]) == "/EndColors")
                        {
                            i = j;
                            break;
                        }
                        string[] MyInput = MyScript[j].Split(' ');
                        if (MyInput.Length == 3)
                        {
                            try
                            {
                                Colors.Add(new Color32(
                                    (byte)int.Parse(MyInput[0]),
                                    (byte)int.Parse(MyInput[1]),
                                    (byte)int.Parse(MyInput[2]),
                                    255
                                ));
                            }
                            catch (System.FormatException e)
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Add UVs for a quad!
        ///  Tile Resolution is width - assume 8
        /// </summary>
        public static void AddQuadUVs(
            int VoxelIndex, 
            int TileResolution, 
            float TextureResolution, 
            int SideIndex, 
            List<Vector2> MyTextureCoordinates)
        {
            float TotalTiles = TileResolution * TileResolution;
            float TileSize = 1f / ((float)TileResolution);
            //float PixelSize = PolyModel.BufferLength / (TileResolution * TextureResolution);  // one pixel is the buffer
            float PixelSize = 0;  // one pixel is the buffer
            TileSize += 2 * PixelSize;  // apply our buffer to tile size
            float PosX = 0;
            float PosY = 0;
            if (TileResolution != 1)
            {
                PosX = (VoxelIndex % (TileResolution - 1));
                PosY = (VoxelIndex / (TileResolution - 1));
            }
            PosX *= TileSize;
            PosY *= TileSize;
            //Debug.LogError ("Buffer: " + Buffer);
            MyTextureCoordinates.Add(new Vector2(PosX + PixelSize,
                                                 PosY + PixelSize));
            MyTextureCoordinates.Add(new Vector2(PosX + TileSize - PixelSize,
                                                 PosY + PixelSize));
            MyTextureCoordinates.Add(new Vector2(PosX + TileSize - PixelSize,
                                                 PosY + TileSize - PixelSize));
            MyTextureCoordinates.Add(new Vector2(PosX + PixelSize,
                                                 PosY + TileSize - PixelSize));
            if (SideIndex == 2) // front
            {
                RotateTextureCoordinates(MyTextureCoordinates);
                RotateTextureCoordinates(MyTextureCoordinates);
            }
            else if (SideIndex == 4) // left
            {
                RotateTextureCoordinates(MyTextureCoordinates);
            }
            else if (SideIndex == 5)    // right
            {
                RotateTextureCoordinates(MyTextureCoordinates);
                RotateTextureCoordinates(MyTextureCoordinates);
                RotateTextureCoordinates(MyTextureCoordinates);
            }
        }
        public static Vector2 GetTilePosition(int TileIndex, int TileResolution)
        {
            //float PixelSize = 0;// 1f / (TileResolution);  // one pixel is the buffer
            float TotalTiles = TileResolution * TileResolution;
            float TileSize = 1f / ((float)TileResolution);
            //TileSize += 2 * PixelSize;  // apply our buffer to tile size
            float PosX = 0;
            float PosY = 0;
            if (TileResolution != 1)
            {
                PosX = (TileIndex % (TileResolution));  // - 1
                PosY = (TileIndex / (TileResolution)); //- 1
            }
            PosX *= TileSize;
            PosY *= TileSize;
            return new Vector2(PosX, PosY);
        }
        public static void RotateTextureCoordinates(List<Vector2> MyTextureCoordinates)
        {
            if (MyTextureCoordinates.Count != 0)
            {
                Vector2 TempVec = MyTextureCoordinates[0];
                for (int i = 1; i < MyTextureCoordinates.Count; i++)
                {
                    MyTextureCoordinates[i - 1] = MyTextureCoordinates[i];
                }
                MyTextureCoordinates[MyTextureCoordinates.Count - 1] = TempVec;
            }
        }
        #endregion
    }
}