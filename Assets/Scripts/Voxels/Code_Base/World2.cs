using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using Zeltex.Guis.Maker;

namespace Zeltex.Voxels
{
    /// <summary>
    /// File Loading section of the world
    /// </summary>
    public partial class World : MonoBehaviour
    {
        private UnityEngine.Events.UnityEvent OnLoadWorld = new UnityEngine.Events.UnityEvent();

        #region FileUtility

        /// <summary>
        /// Resets chunks to use for loading
        /// </summary>
        private void Reset()
        {
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        MyChunk.Reset(); // set them all to air since i've updated them
                    }
                }
            }
        }
        /// <summary>
        /// Set all the chunks voxels to 0 Air without triggering updates!
        /// </summary>
        public void SetAllVoxelsRaw()
        {
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        MyChunk.SetAllVoxelsRaw(0);
                    }
                }
            }
        }
        

        public void SetAllVoxelsUpdated()
        {
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        MyChunk.SetAllUpdated(true);
                    }
                }
            }
        }


        private void HideChunks()
        {
            SetChunksVisibility(false);
        }

        /// <summary>
        /// Turn all the renders off! Used when loading in new world
        /// </summary>
        private void SetChunksVisibility(bool NewState)
        {
            // first hide the chunks
            foreach (Int3 Key in MyChunkData.Keys)
            {
                if (MyChunkData.ContainsKey(Key))
                {
                    Chunk MyChunk = MyChunkData[Key];
                    if (MyChunk)
                    {
                        if (MyChunk != null)
                        {
                            MyChunk.GetComponent<MeshRenderer>().enabled = NewState;
                        }
                    }
                }
            }
        }

        public int GetMaxVoxelCount()
        {
            int VoxelCount = Mathf.RoundToInt(WorldSize.x * WorldSize.y * WorldSize.z);    // total world size
            VoxelCount *= Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;
            return VoxelCount;
        }
		#endregion

        #region Saving

        /// <summary>
        /// Saves all the chunks for a model maker
        /// </summary>
        public IEnumerator SaveRoutine(string FilePath, ModelMaker MyManager)
        {
            List<string> MyScriptList = new List<string>();
            if (IsSingleChunk())
            {
                MyScriptList.AddRange(GetChunk(new Int3()).GetScript());
                yield return null;
            }
            else
            {
                MyScriptList.Add("/World " + WorldSize.x + " " + WorldSize.y + " " + WorldSize.z);
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MyScriptList.Add("/Chunk " + MyKey.x + " " + MyKey.y + " " + MyKey.z);
                    MyScriptList.AddRange(MyChunk.GetScript());
                    yield return null;
                }
            }
            string MyScript = FileUtil.ConvertToSingle(MyScriptList);
            //File.WriteAllText(FilePath, MyScript);
            DataManager.Get().Set(DataFolderNames.VoxelModels, MyManager.GetSelectedIndex(), MyScript);
        }

        /// <summary>
        /// Gets an entire world script.
        /// </summary>
        public List<string> GetScript()
        {
            return GetScriptList();
        }

        /// <summary>
        /// Returns a list of data for a world
        /// </summary>
        public List<string> GetScriptList()
        {
            List<string> MyData = new List<string>();
            MyData.AddRange(MyLookupTable.GetScript());
            if (IsSingleChunk())
            {
                MyData.AddRange(GetChunk(new Int3()).GetScript());
            }
            else
            {
                MyData.Add("/World " + WorldSize.x + " " + WorldSize.y + " " + WorldSize.z);
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MyData.Add("/Chunk " + MyKey.x + " " + MyKey.y + " " + MyKey.z);
                    MyData.AddRange(MyChunk.GetScript());
                }
            }
            return MyData;
        }


        #endregion

        #region Loading


        public void AddOnLoad(UnityEngine.Events.UnityAction OnLoadAction)
        {
            OnLoadWorld.RemoveListener(OnLoadAction);
            OnLoadWorld.AddEvent(OnLoadAction);
        }
        public void RemoveOnLoad(UnityEngine.Events.UnityAction OnLoadAction)
        {
            OnLoadWorld.RemoveListener(OnLoadAction);
        }

        /// <summary>
        /// Loads the world script on a seperate routine
        /// </summary>
        public void RunScript(List<string> MyData)
        {
            UniversalCoroutine.CoroutineManager.StartCoroutine(RunScriptRoutine(MyData));
        }

        /// <summary>
        /// Run the script, loading the world including its meta data
        /// Voxel Chunk Data
        /// Lookup table
        /// The size of the chunks
        /// Any modifiers on the chunk - ie noise grid
        /// </summary>
        public IEnumerator RunScriptRoutine(List<string> MyData)
        {
            //Debug.LogError("Begun World RunScriptRoutine " + name);
            if (MyData.Count > 0)
            {
                HideChunks();
                SetAllVoxelsRaw();  // clear the voxel indexes
                List<string> LookupData = new List<string>();
                if (MyData[0] == VoxelLookupTable.BeginTag)
                {
                    for (int i = 0; i < MyData.Count; i++)
                    {
                        if (MyData[i] == VoxelLookupTable.EndTag)
                        {
                            LookupData = MyData.GetRange(0, i + 1);
                            MyData.RemoveRange(0, i + 1);
                            break;
                        }
                    }
                    // Remove lookup table from list
                }
                else
                {
                    Debug.LogError("Loading World without LookupTable Tag");
                }
                Reset();
                bool IsMultiChunk = (MyData[0].Contains("/World"));
                Int3 NewWorldSize;
                if (IsMultiChunk)   // if multiple chunks
                {
                    string[] MyWorldMeta = MyData[0].Split(' ');
                    if (MyWorldMeta.Length == 4)
                    {
                        NewWorldSize = new Int3(int.Parse(MyWorldMeta[1]), int.Parse(MyWorldMeta[2]), int.Parse(MyWorldMeta[3]));
                    }
                    else
                    {
                        NewWorldSize = Int3.Zero();
                    }
                }
                else
                {
                    NewWorldSize = new Int3(1, 1, 1);
                }
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(NewWorldSize)); // this will refresh it
                MyLookupTable.RunScript(GetMaxVoxelCount(), LookupData);
                if (IsMultiChunk)   // if multiple chunks
                {
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(RunScriptMultiWorldRoutine(MyData));
                    // TODO: Fix this!
                    GetChunk(Int3.Zero()).SetAllUpdated(true);
                    GetChunk(Int3.Zero()).OnMassUpdate();
                }
                else
                {
                    Chunk WorldChunk = gameObject.GetComponent<Chunk>();
                    if (WorldChunk)
                    {
                        //Debug.LogError("Begun WorldChunk RunScript " + name);
                        yield return null;
                        //Debug.LogError("Loading chunkworld script from world: " + name);
                        yield return UniversalCoroutine.CoroutineManager.StartCoroutine(WorldChunk.RunScript(MyData));
                        //Debug.LogError("Ended WorldChunk RunScript " + name);
                    }
                    else
                    {
                        Debug.LogError(name + " has no chunk.");
                    }
                }
            }
            else
            {
                //Debug.LogError("Loading world with no data");
                yield return UniversalCoroutine.CoroutineManager.StartCoroutine(SetWorldSizeRoutine(Int3.Zero())); // refresh! No more things!
            }
            OnLoadWorld.Invoke();
           // Debug.LogError("Ended World RunScriptRoutine " + name);
        }

        /// <summary>
        /// Runs the script in a routine
        /// </summary>
        private IEnumerator RunScriptMultiWorldRoutine(List<string> MyData)
        {
            IsUseUpdater = false;
            // Debug.LogError("Loading World on routine.");
            //List<Chunk> MyChunks = new List<Chunk>();
            for (int i = 1; i < MyData.Count; i++)
            {
                if (MyData[i].Contains("/Chunk"))
                {
                    string[] MyChunkMeta = MyData[i].Split(' ');
                    Chunk MyChunk = GetChunk(new Int3(int.Parse(MyChunkMeta[1]), int.Parse(MyChunkMeta[2]), int.Parse(MyChunkMeta[3])));
                    //i += Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;
                    int NextIndex = i + 1;
                    // Find Next Chunk In Data
                    for (int j = i + 1; j < MyData.Count; j++)
                    {
                        if (MyData[j].Contains("/Chunk"))
                        {
                            NextIndex = j - 1;
                            break;
                        }
                        if (j == MyData.Count - 1)
                        {
                            NextIndex = MyData.Count - 1;
                        }
                    }
                    // now we have next index
                    List<string> MyChunkData = MyData.GetRange(i + 1, NextIndex - i);
                    Debug.LogError(i + " Loading chunk: " + MyChunk.Position.GetVector().ToString() + " - " + MyChunkData.Count);
                    yield return UniversalCoroutine.CoroutineManager.StartCoroutine(MyChunk.RunScript(MyChunkData));
                    //MyChunks.Add(MyChunk);
                    i = NextIndex;
                    yield return null;
                }
            }
            //yield return MyUpdater.UpdateChunks(MyChunks);
            //Debug.LogError("Resizing World to 3:" + GetComponent<World>().WorldSize.ToString());
            IsUseUpdater = true;
        }

        #endregion

        #region ChunkLoading
        #endregion

        #region Networking
        // request chunk from server
        /*[PunRPC]
        public void ChunkRequest(int PlayerRequestedID, int ChunkPositionX, int ChunkPositionY, int ChunkPositionZ)
        {
            //Debug.LogError ("Requested to give chunk.");
            PhotonPlayer MyPlayerRequested = PhotonPlayer.Find(PlayerRequestedID);
            Chunk MyChunk = GetChunk(new Int3(ChunkPositionX, ChunkPositionY, ChunkPositionZ));
            if (MyChunk == null)
                return;

            gameObject.GetComponent<PhotonView>().RPC("RecieveChunkData",
                MyPlayerRequested,
                ChunkPositionX, ChunkPositionY, ChunkPositionZ,
                FileUtil.ConvertToSingle(MyChunk.GetScript())
                );
        }

        [PunRPC]
        public void RecieveChunkData(int ChunkPositionX, int ChunkPositionY, int ChunkPositionZ, string MyData)
        {
            //Debug.LogError ("Recieved chunk data.");
            Chunk MyChunk = GetChunk(new Int3(ChunkPositionX, ChunkPositionY, ChunkPositionZ));
            if (MyChunk == null)
                return;
            MyChunk.RunScript(FileUtil.ConvertToList(MyData));
        }*/
        #endregion
    }
}