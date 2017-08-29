using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Lighting part of chunk
    /// 
    /// </summary>
    public partial class Chunk : MonoBehaviour
    {

        // Propogates lighting accross Voxels
        #region Lighting

        /// <summary>
        /// Get the lights around the voxel
        /// </summary>
        private void GetLights(Voxel ThisVoxel, int i, int j, int k, int MaterialType, ref int[] MyLights)
        {
            if (MyWorld.IsLighting)
            {
                //MyLights = ThisVoxel.GetSurroundingLights(this, i, j, k, MaterialType);
            }
            else
            {
                //MyLights = ThisVoxel.GetBasicLights(this, i, j, k, MaterialType);
            }
            for (int a = 0; a < MyLights.Length; a++)
            {
                MyLights[a] = 255;
            }
        }

        /// <summary>
        /// Chunk lighting!
        /// </summary>
        public static MeshData AddBasicLights(MeshData MyVoxelMesh, int[] MyLights, int SideIndex)
        {
            byte ThisLight = (byte)MyLights[SideIndex];
            MyVoxelMesh.AddQuadColours(ThisLight);
            return MyVoxelMesh;
        }
        /// <summary>
        /// A smoothed version of lighting
        /// </summary>
        public static MeshData AddSmoothedLights(MeshData MyVoxelMesh, int[] MyLights)
        {
            Vector3[] MyVerticies = MyVoxelMesh.GetVerticies();
            int[] MyTriangles = MyVoxelMesh.GetTriangles();
            //Color32[] MyColors = MyVoxelMesh.GetColors();
            for (int z = 0; z < MyTriangles.Length; z += 3)
            {
                Vector3 Vertex1 = MyVerticies[MyTriangles[z]];
                Vector3 Vertex2 = MyVerticies[MyTriangles[z + 1]];
                Vector3 Vertex3 = MyVerticies[MyTriangles[z + 2]];
                // average the light value out by this
                int LightsAdded1 = 0;
                int LightsAdded2 = 0;
                int LightsAdded3 = 0;
                // if light source hits triangle, add light to it
                int LightStrength1 = 0;
                int LightStrength2 = 0;
                int LightStrength3 = 0;
                int LightIndex = 0;

                List<int> LightAddedIndex = new List<int>();
                List<Vector3> LightPositions = new List<Vector3>();
                for (int j = -1; j <= 1; j++)
                    for (int i = -1; i <= 1; i++)
                        for (int k = -1; k <= 1; k++)
                        {
                            int LightBrightness = MyLights[LightIndex];
                            LightIndex++;
                            Vector3 LightPosition = new Vector3(i, j, k) + new Vector3(0.5f, 0.5f, 0.5f);   // to make the lights in the middle of cubes
                            Vector3 TriangleMidPoint = (Vertex1 + Vertex2 + Vertex3) / 3f;
                            // Get Direction of Light Souce to Triangle
                            Vector3 LightDirection = (TriangleMidPoint - LightPosition).normalized;

                            // if light hits triangle, add the light
                            //if (DoesIntersect (Vertex1, Vertex2, Vertex3, LightPosition, LightDirection)) 
                            if (DoesIntersectPlane(Vertex1, Vertex2, Vertex3, LightPosition))
                            {
                                float LightDistance1 = Vector3.Distance(LightPosition, Vertex1);
                                if (LightDistance1 < 1)
                                {
                                    LightsAdded1++;
                                    LightStrength1 += Mathf.RoundToInt(LightDistance1 * ((float)LightBrightness));
                                    //LightAddedIndex.Add (LightIndex);
                                    //LightPositions.Add (LightPosition);
                                }
                                float LightDistance2 = Vector3.Distance(LightPosition, Vertex2);
                                if (LightDistance2 < 1)
                                {
                                    LightsAdded2++;
                                    LightStrength2 += Mathf.RoundToInt(LightDistance2 * ((float)LightBrightness));
                                }
                                float LightDistance3 = Vector3.Distance(LightPosition, Vertex3);
                                if (LightDistance3 < 1)
                                {
                                    LightsAdded3++;
                                    LightStrength3 += Mathf.RoundToInt(LightDistance3 * ((float)LightBrightness));
                                }
                                //Debug.LogError ("Light intersection at: " + LightPosition.ToString ());
                            }
                        }

                if (LightsAdded1 != 0)
                {
                    LightStrength1 /= LightsAdded1;
                    //if (SideIndex == 0)
                    {
                        //Debug.LogError ("Lights1 added: " + LightStrength1);
                        //for (int i = 0; i < LightAddedIndex.Count; i++)
                        //	Debug.LogError ("Side [" + SideIndex + "] Light Index [" + LightAddedIndex[i] + "] And light position [" + LightPositions[i] + "]");
                        //Debug.Break ();
                    }
                }
                if (LightsAdded2 != 0)
                {
                    LightStrength2 /= LightsAdded2;
                    //Debug.LogError ("Lights2 added: " + LightStrength2);
                }
                if (LightsAdded3 != 0)
                {
                    LightStrength3 /= LightsAdded3;
                    //Debug.LogError ("Lights3 added: " + LightStrength3);
                }
                MyVoxelMesh.SetColor(MyTriangles[z], new Color32((byte)LightStrength1, (byte)LightStrength1, (byte)LightStrength1, 255));
                MyVoxelMesh.SetColor(MyTriangles[z + 1], new Color32((byte)LightStrength2, (byte)LightStrength2, (byte)LightStrength2, 255));
                MyVoxelMesh.SetColor(MyTriangles[z + 2], new Color32((byte)LightStrength3, (byte)LightStrength3, (byte)LightStrength3, 255));

            }
            return MyVoxelMesh;
        }

        public static bool DoesIntersectPlane(Vector3 Vertex1, Vector3 Vertex2, Vector3 Vertex3, Vector3 RayOrigin)
        {
            Plane MyPlane = new Plane(Vertex1, Vertex2, Vertex3);
            return MyPlane.GetSide(RayOrigin);
        }
        /// <summary>
        /// Set the brightness at a position
        /// </summary>
        public void SetBrightness(int i, int j, int k, float Brightness)
        {
            SetBrightness(i, j, k, Mathf.RoundToInt(Brightness));
        }
        /// <summary>
        /// Sets the brightness at a position using integers
        /// </summary>
		public void SetBrightness(int i, int j, int k, int Brightness)
        {
            /*if (GetVoxel (i, j, k).SetBrightness (this, i, j, k, Brightness)) 
			{
				//if (IsPrimaryChange)
				HasChangedAt (i, j, k);
			}*/
        }
        /// <summary>
        /// Sets the brightness at a position with mass
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="Brightness"></param>
		public void SetBrightnessMass(int i, int j, int k, float Brightness)
        {
            SetBrightnessMass(i, j, k,
                Mathf.RoundToInt(Brightness));
        }
        public void SetBrightnessMass(int i, int j, int k, int Brightness)
        {
            //GetVoxel (i, j, k).SetBrightnessUnChanged (this, i, j, k, Brightness);
        }
        /// <summary>
        /// Sets all lights to maximum brightness
        /// </summary>
        public void ResetLighting()
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                for (int j = 0; j < ChunkSize; j++)
                {
                    for (int k = 0; k < ChunkSize; k++)
                    {
                        SetBrightnessMass(i, j, k, MyWorld.SunBrightness);
                    }
                }
            }
        }
        /// <summary>
        /// Casts sunlight from the top most block, down to the bottom
        /// maybe later do directional sunlight?
        /// </summary>
        public void Sunlight()
        {
            //Debug.LogError("Updating Sunlight!");
            for (int i = 0; i < ChunkSize; i++)
                for (int k = 0; k < ChunkSize; k++)
                {
                    bool IsDarkness = false;
                    for (int j = ChunkSize - 1; j >= 0; j--)
                    {
                        Voxel ThisVoxel = GetVoxel(new Int3(i, j, k));
                        //bool HasChanged = false;
                        if (IsDarkness)
                        {
                            /*if (ThisVoxel.GetBlockIndex() == 6)   // if water
                            {
                                SetBrightnessMass(i, j, k, MyWorld.SunBrightness * MyWorld.PropogationDecreaseRate);
                            }
                            else
                            {*/
                            SetBrightnessMass(i, j, k, MyWorld.DefaultBrightness);
                            //}
                            //HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, DefaultBrightness);
                        }
                        else
                        {
                            if (ThisVoxel.GetVoxelType() == 0)
                            {
                                SetBrightnessMass(i, j, k, MyWorld.SunBrightness);
                                //HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, SunBrightness);
                            }
                            else
                            {
                                IsDarkness = true;
                                SetBrightnessMass(i, j, k, MyWorld.DefaultBrightness);
                                //HasChanged = GetVoxel (i, j, k).SetBrightness (this, i, j, k, DefaultBrightness);
                            }
                        }

                    }
                }
        }

        /// <summary>
        /// Starts the light propogation.
        /// </summary>
        public void PropogateLights()
        {
            Debug.Log("Propogating lights in " + name);
            //IsInLightQue = false;
            //IsRunningLightsThread = true;
            int Buffer = 1;
            if (MyWorld.IsLighting)
            {
                Sunlight();
                for (int i = -Buffer; i < ChunkSize + Buffer; i++)
                {
                    for (int j = -Buffer; j < ChunkSize + Buffer; j++)
                    {
                        for (int k = -Buffer; k < ChunkSize + Buffer; k++)
                        {
                            Voxel ThisVoxel = GetVoxel(new Int3(i, j, k));
                            if (ThisVoxel != null)
                            {
                                int Brightness = 255;// (int)ThisVoxel.GetLight ();
                                int BlockIndex = ThisVoxel.GetVoxelType();
                                if (BlockIndex == 0 || BlockIndex == 6)
                                {
                                    //PropogateLight (i, j, k, MyBrightnss, 0);
                                    PropogateLight(i + 1, j, k, Brightness, 1);
                                    PropogateLight(i - 1, j, k, Brightness, 1);
                                    PropogateLight(i, j + 1, k, Brightness, 1);
                                    PropogateLight(i, j - 1, k, Brightness, 1);
                                    PropogateLight(i, j, k + 1, Brightness, 1);
                                    PropogateLight(i, j, k - 1, Brightness, 1);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < ChunkSize; i++)
                    for (int j = 0; j < ChunkSize; j++)
                        for (int k = 0; k < ChunkSize; k++)
                        {
                            Voxel ThisVoxel = GetVoxel(new Int3(i, j, k));
                            //ThisVoxel.SetBrightness(this, i, j, k, MyWorld.SunBrightness);
                        }
            }
        }

        /// <summary>
        /// Propogates the light, using celluar automato, to move lighting accross voxels.
        /// </summary>
		public void PropogateLight(int i, int j, int k, int Brightness, int PropogationCount)
        {
            /*Voxel ThisVoxel = GetVoxel (i, j, k);
			Brightness = Mathf.RoundToInt (Brightness* MyWorld.PropogationDecreaseRate);
			if (ThisVoxel != null)
			if (PropogationCount <= 8
				&& Brightness > MyWorld.DefaultBrightness
				&& ThisVoxel.GetVoxelType() == 0
				&& Brightness > GetVoxel (i, j, k).GetLight ()
				// && IsInRange (i) && IsInRange (j) && IsInRange (k)		// is within this chunk
				)
			{
                    SetBrightnessMass(i,j,k, Brightness);
				PropogationCount++;
				PropogateLight (i + 1, j, k, Brightness,PropogationCount);
				PropogateLight (i - 1, j, k, Brightness,PropogationCount);
				PropogateLight (i, j + 1, k, Brightness,PropogationCount);
				PropogateLight (i, j - 1, k, Brightness,PropogationCount);	
				PropogateLight (i, j, k + 1, Brightness,PropogationCount);
				PropogateLight (i, j, k - 1, Brightness,PropogationCount);
			} else {
				//Debug.LogError("Blarg!: " + Brightness);
			}*/
        }
        #endregion
    }
}