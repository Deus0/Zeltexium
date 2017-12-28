using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using Zeltex;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generate Voxel Models!!
    /// </summary>
    public class PolyModelGenerator : MonoBehaviour
    {
        public static PolyModelGenerator Myself;

        private void Start()
        {
            Myself = this;
        }
        public static PolyModelGenerator Get()
        {
            return Myself;
        }
        public IEnumerator GenerateData()
        {
            // First generate a voxel model
            int MyRed = Random.Range(1, 255);
			//DataManager.Get().Add("PolyModels", "Model 1", GetSphere());

            /*List<string> MyPolyModelData2 = new List<string>();
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
                for (int j = 0; j < Chunk.ChunkSize; j++)
                {
                    for (int k = 0; k < Chunk.ChunkSize; k++)
                    {
                        float MyDistance = Vector3.Distance(new Vector3(i, j, k), new Vector3(7, 7, 7)) + Random.Range(0, 3);
                        if (MyDistance < 8)
                        {
                            //MyPolyModelData2.Add("" + 1);
                            MyPolyModelData2.Add("" + 1 + " " + Random.Range(1, 155) + " " + Random.Range(1, 55) + " " + Random.Range(1, 55));
                        }
                        else
                        {
                            MyPolyModelData2.Add("" + 0);
                        }
                    }
                }
            }
            MyPolyModelMaker.AddData("Model 2", FileUtil.ConvertToSingle(MyPolyModelData2));*/
            //yield return new WaitForSeconds(0.01f);
            yield break;
        }

        public string GetSphere()
        {
            Color BeginColor = new Color(Random.Range(1, 155), Random.Range(1, 155), Random.Range(1, 155));
            int Variation = 16;
            List<string> MyScript = new List<string>();
            //string MyColorBlock = "Color";
            //int ColorBlockIndex = MyBlockMaker.MyDatabase.GetMeta("Color");

            // First lookup table
            MyScript.Add(VoxelLookupTable.BeginTag);
            int AirIndex = 0;
            MyScript.Add(("" + AirIndex) + VoxelLookupTable.SplitterTag + "Air");
            int ColorBlockIndex = 1;
            MyScript.Add(("" + ColorBlockIndex) + VoxelLookupTable.SplitterTag + "Color");
            MyScript.Add(VoxelLookupTable.EndTag);

            // Then chunk data
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
                for (int j = 0; j < Chunk.ChunkSize; j++)
                {
                    for (int k = 0; k < Chunk.ChunkSize; k++)
                    {
                        if (Vector3.Distance(new Vector3(i, j, k), new Vector3(8, 8, 8)) < 8)
                        {
                            int ThisVariationR = Random.Range(-Variation, Variation);
                            Color ThisColor = new Color(
                                (byte)(BeginColor.r + ThisVariationR),
                                (byte)(BeginColor.g + ThisVariationR),
                                (byte)(BeginColor.b + ThisVariationR));
                            MyScript.Add("" + ColorBlockIndex + " " + (int)ThisColor.r + " " + (int)ThisColor.g + " " + (int)ThisColor.b);    // random colour
                        }
                        else
                        {
                            MyScript.Add("" + AirIndex);
                        }
                    }
                }
            }
            return FileUtil.ConvertToSingle(MyScript);
        }
    }
}
