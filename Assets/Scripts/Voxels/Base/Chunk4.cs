using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    public partial class Chunk : MonoBehaviour
    {
        #region File
        private static bool IsMutateColor = false;
        private static Color MutateColorAddition = new Color(0.88f, 0.4f, 0.58f, 1f);
        private static float MutateColorVariance = 0.14f;

        private Int3 LoadingVoxelIndex = Int3.Zero();
        int ScriptLineIndex = 0;

        public void SetDefaultVoxelNames()
        {
            MyWorld.MyLookupTable.Clear();
            for (int i = 0; i < DataManager.Get().GetSize(DataFolderNames.Voxels); i++)
            {
                MyWorld.MyLookupTable.AddName(DataManager.Get().GetName(DataFolderNames.Voxels, i));
            }
        }
        /// <summary>
        /// Loading the data for a particular chunk, need to call onMassUpdate after all the updates
        /// Used by WorldManager
        /// </summary>
        public IEnumerator RunScript(List<string> MyLines)
        {
            //Debug.LogError("Running script on chunk: " + name + ":" + MyLines.Count +  "\n" + FileUtil.ConvertToSingle(MyLines));
            //int UpdatedBlocks = 0;
            Color MutationColor = new Color(
                Random.Range(0, MutateColorAddition.r),
                Random.Range(0, MutateColorAddition.g),
                Random.Range(0, MutateColorAddition.b),
                1f);
            string[] MyInput;
            int MyBlockType;
            //bool DidUpdate;
            Color MyColor;
            ScriptLineIndex = 0;
            if (MyWorld == null)
            {
                Debug.LogError(name + " didnt have a World");
                MyWorld = GetComponent<World>();
                RefreshComponents();
            }
            if (MyWorld.MyLookupTable == null)
            {
                Debug.LogError(name + " didnt have a LookupTable");
                MyWorld.MyLookupTable = new VoxelLookupTable();
            }
            bool DidUpdateChunk = false;
            bool DidUpdateVoxel = false;
            MyWorld.MyLookupTable.AddName("Color");
            for (LoadingVoxelIndex.x = 0; LoadingVoxelIndex.x < Chunk.ChunkSize; LoadingVoxelIndex.x++)
            {
                for (LoadingVoxelIndex.y = 0; LoadingVoxelIndex.y < Chunk.ChunkSize; LoadingVoxelIndex.y++)
                {
                    for (LoadingVoxelIndex.z = 0; LoadingVoxelIndex.z < Chunk.ChunkSize; LoadingVoxelIndex.z++)
                    {
                        if (ScriptLineIndex < MyLines.Count)
                        {
                            MyInput = MyLines[ScriptLineIndex].Split(' ');
                            if (MyInput.Length == 1)
                            {
                                try
                                {
                                    MyBlockType = int.Parse(MyLines[ScriptLineIndex]);
                                    //if (MyBlockType != 0)
                                    {
                                        MassUpdateVoxelIndex = MyBlockType;
                                        if (MyWorld.MyLookupTable != null)
                                        {
                                            MassUpdateVoxelName = MyWorld.MyLookupTable.GetName(MyBlockType);
                                        }
                                        MassUpdateColor = Color.white;
                                        MassUpdatePosition.Set(LoadingVoxelIndex);
                                        DidUpdateVoxel = UpdateBlockTypeLoading();
                                        if (DidUpdateChunk == false && DidUpdateVoxel)
                                        {
                                            DidUpdateChunk = true;
                                        }
                                    }
                                    //DidUpdate = UpdateBlockTypeMass(VoxelIndex, MyBlockType, Color.white);
                                    /*if (DidUpdate)
									{
										UpdatedBlocks++;
									}*/
                                }
                                catch (System.FormatException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                                catch (System.NullReferenceException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                            }
                            else if (MyInput.Length == 4)
                            {
                                try
                                {
                                    MyBlockType = int.Parse(MyInput[0]);
                                    //if (MyBlockType != 0)
                                    {
                                        MyColor.r = int.Parse(MyInput[1]) / 255f;
                                        MyColor.g = int.Parse(MyInput[2]) / 255f;
                                        MyColor.b = int.Parse(MyInput[3]) / 255f;

                                        if (IsMutateColor)
                                        {
                                            MyColor.r = Mathf.Clamp((MyColor.r + Random.Range(-MutateColorVariance, MutateColorVariance) + MutationColor.r) / 2f, 0, 1);
                                            MyColor.g = Mathf.Clamp((MyColor.g + Random.Range(-MutateColorVariance, MutateColorVariance) + MutationColor.g) / 2f, 0, 1);
                                            MyColor.b = Mathf.Clamp((MyColor.b + Random.Range(-MutateColorVariance, MutateColorVariance) + MutationColor.b) / 2f, 0, 1);
                                        }
                                        MassUpdateColor.r = MyColor.r;
                                        MassUpdateColor.g = MyColor.g;
                                        MassUpdateColor.b = MyColor.b;
                                        if (MyWorld.MyLookupTable != null)
                                        {
                                            MassUpdateVoxelName = MyWorld.MyLookupTable.GetName(MyBlockType);
                                        }
                                        MassUpdateVoxelIndex = MyBlockType;
                                        MassUpdatePosition.Set(LoadingVoxelIndex);
                                        DidUpdateVoxel = UpdateBlockTypeLoading();
                                        if (DidUpdateChunk == false && DidUpdateVoxel)
                                        {
                                            DidUpdateChunk = true;
                                        }
                                    }
                                    //DidUpdate = UpdateBlockTypeMass(VoxelIndex, MyBlockType, MyColor);
                                    /*if (DidUpdate)
									{
										UpdatedBlocks++;
									}*/
                                }
                                catch (System.FormatException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                            }
                            ScriptLineIndex++;
                        }
                        //Debug.LogError("Running script for: " + name + "'s VoxelDatas:" + MyVoxels.GetVoxelRaw(DebugVoxelIndex).GetVoxelType().ToString());
                    }
                }
                if (DidUpdateChunk && LoadingVoxelIndex.x % 2 == 0)
                {
                    yield return null;
                }
            }
            //DebugChunkVoxels();
            WasMassUpdated = true;

            //Debug.LogError("Finished loading script on chunk: " + name + ":" + MyLines.Count + "\n" + FileUtil.ConvertToSingle(MyLines));
            if (DidUpdateChunk)
            {
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(BuildChunkMesh());
            }

            /*if (UnityEngine.Application.isEditor && UnityEngine.Application.isPlaying == false)
            {
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(this, BuildChunkMesh());
            }
            else
            {
                Debug.LogError("OnMassUpdate chunk: " + name);
                OnMassUpdate();
                while (HasLoaded() == false)
                {
                    yield return null;
                }
            }*/
           // Debug.LogError("Finished loading chunk: " + name);
            //Debug.Log("Updated chunk [" + MyChunk.name + "] with " + UpdatedBlocks + " block updates.");
            //yield return null;
        }

        private Int3 DebugVoxelIndex = Int3.Zero();
        public void DebugChunkVoxels()
        {
            Debug.LogError(name + " is debugging voxel types.");
            for (DebugVoxelIndex.x = 0; DebugVoxelIndex.x < ChunkSize; DebugVoxelIndex.x++)
            {
                for (DebugVoxelIndex.y = 0; DebugVoxelIndex.y < ChunkSize; DebugVoxelIndex.y++)
                {
                    for (DebugVoxelIndex.z = 0; DebugVoxelIndex.z < ChunkSize; DebugVoxelIndex.z++)
                    {
                        //if (MyVoxels.GetVoxelRaw(VoxelIndex).GetVoxelType() != 0)
                        {
                            Debug.LogError(name + "'s VoxelDatas:" + MyVoxels.GetVoxelRaw(DebugVoxelIndex).GetVoxelType().ToString());   // + VoxelIndex.GetVector().ToString() +
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of data for a Chunk
        /// </summary>
        public List<string> GetScript()
        {
            List<string> MyData = new List<string>();
            for (int i = 0; i < ChunkSize; i++)
            {
                for (int j = 0; j < ChunkSize; j++)
                {
                    for (int k = 0; k < ChunkSize; k++)
                    {
                        Voxel MyVoxel = GetVoxel(new Int3(i, j, k));
                        VoxelColor MyVoxelColor = MyVoxel as VoxelColor;
                        if (MyVoxelColor == null)
                        {
                            MyData.Add("" + MyVoxel.GetVoxelType());
                        }
                        else
                        {
                            Color MyColor = MyVoxelColor.GetColor();
                            int Red = (int)(255 * MyColor.r);
                            int Green = (int)(255 * MyColor.g);
                            int Blue = (int)(255 * MyColor.b);
                            MyData.Add("" + MyVoxelColor.GetVoxelType() + " " + Red + " " + Green + " " + Blue);
                        }
                    }
                }
            }
            return MyData;
        }


        /// <summary>
        /// Returns a list of data for a Chunk
        /// </summary>
        public string GetSerial()
        {
            string Data = "";
            Int3 Position = Int3.Zero();
            Voxel MyVoxel;
            VoxelColor MyVoxelColor;
            Color MyColor = Color.white;
            for (Position.x = 0; Position.x < ChunkSize; Position.x++)
            {
                for (Position.y = 0; Position.y < ChunkSize; Position.y++)
                {
                    for (Position.z = 0; Position.z < ChunkSize; Position.z++)
                    {
                        MyVoxel = MyVoxels.GetVoxelRaw(Position);
                        MyVoxelColor = MyVoxel as VoxelColor;
                        if (MyVoxelColor == null)
                        {
                            Data += ("" + MyVoxel.GetVoxelType()) + "\n";
                        }
                        else
                        {
                            MyColor = MyVoxelColor.GetColor();
                            Data += ("" + MyVoxelColor.GetVoxelType() +
                                " " + (int)(255 * MyColor.r)
                                + " " + (int)(255 * MyColor.g) 
                                + " " + (int)(255 * MyColor.b)) + "\n";
                        }
                    }
                }
            }
            return Data;
        }
        #endregion

        #region TerrainUpdates
        [HideInInspector]
        public string MassUpdateVoxelName;
        [HideInInspector]
        public Int3 MassUpdatePosition = Int3.Zero();
        [HideInInspector]
        public Color MassUpdateColor;
        private Voxel MassUpdateVoxel;
        private int MassUpdateVoxelIndex;
        private int PreviousIndex;
        private string PreviousVoxelName;
        /// <summary>
        /// Uses: MassUpdatePosition, MassUpdateVoxelName, MassUpdateColor, 
        /// Breaks up voxel updates from a mass into a single update
        /// </summary>
        public void UpdateBlockTypeMassTerrain()
        {
            MassUpdateVoxelIndex = MyWorld.MyLookupTable.GetIndex(MassUpdateVoxelName);
            UpdateBlockTypeLoading();
        }

        private bool UpdateBlockTypeLoading()
        {
            PreviousIndex = MyVoxels.GetVoxelType(MassUpdatePosition.x, MassUpdatePosition.y, MassUpdatePosition.z);
            bool DidUpdate = MyVoxels.SetVoxelType(
                this,
                MassUpdatePosition,
                MassUpdateVoxelIndex,
                MassUpdateColor);
            if (DidUpdate)
            {
                // get names
                PreviousVoxelName = MyWorld.MyLookupTable.GetName(PreviousIndex);
                //MassUpdateVoxelIndex = MyWorld.MyLookupTable.GetName(MassUpdateVoxelIndex);
                MyWorld.MyLookupTable.OnReplace(PreviousVoxelName, MassUpdateVoxelName);
                MyLookupTable.OnReplace(PreviousVoxelName, MassUpdateVoxelName);
                // surrounding chunks!
                // OnUpdatedAtPosition(InChunkPosition);
                //HasChangedAt(InChunkPositionX, InChunkPositionY, InChunkPositionZ, false);
            }
            return DidUpdate;
        }
        //Debug.Log("[" + name + "] Updated voxel in world position: " + WorldPosition.ToString() 
        //    + "\n" + InChunkPositionX + ":" + InChunkPositionY + ":" + InChunkPositionZ
        //    + "\n" + PreviousIndex + " to " + Type + " of color " + NewColor.ToString());
        #endregion
    }
}