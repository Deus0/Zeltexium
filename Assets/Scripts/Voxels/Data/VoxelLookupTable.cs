using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Voxels
{
    [System.Serializable()]
    public class StringIntDictionary : SerializableDictionaryBase<string, int>
    {

    }
    [System.Serializable()]
    public class IntStringDictionary : SerializableDictionaryBase<int, string>
    {

    }
    /// <summary>
    /// A lookup table for voxel indexes - to make them dynamic
    /// Links a voxel name to an index
    /// Air is default, then anything added after that is added into it
    /// </summary>
    [System.Serializable]
    public class VoxelLookupTable
    {
        public static string BeginTag = "/BeginVoxelLookup";
        public static string EndTag = "/EndVoxelLookup";
        public static char SplitterTag = ':';
        [SerializeField]
        private StringIntDictionary MyLookupTable = new StringIntDictionary();
        [SerializeField]
        private StringIntDictionary VoxelCount = new StringIntDictionary();
        [SerializeField]
        private IntStringDictionary MyReverseLookupTable = new IntStringDictionary();    // put in an integer and get a string back

        #region Init
        public VoxelLookupTable()
        {
            Clear();
        }

        public bool ContainsMeshIndex(int MeshIndex)
        {
            return MyReverseLookupTable.ContainsKey(MeshIndex);
        }
        #endregion

        #region Debug
        /// <summary>
        /// A debug list of the values
        /// </summary>
        public List<string> GetStatistics()
        {
            List<string> MyData = new List<string>();
            foreach (string MyKey in MyLookupTable.Keys)
            {
                MyData.Add("[" + MyLookupTable[MyKey] + " - " + MyKey + "] [x" + VoxelCount[MyKey] + "]");
            }
            return MyData;
        }
        #endregion

        #region Counting

        /// <summary>
        /// Clears the lookup table and adds default variable to it
        /// </summary>
        public void Clear()
        {
            MyLookupTable.Clear();
            MyReverseLookupTable.Clear();
            VoxelCount.Clear();
            // Default Count!
            Add("Air");  // make sure this is the first one
            //VoxelCount["Air"] = Chunk.ChunkSize * Chunk.ChunkSize;  // default count for a voxel!
        }

        /// <summary>
        /// When an old voxel is replaced by a new one
        /// </summary>
        public void OnReplace(string OldVoxel, string NewVoxel)
        {
            Decrease(OldVoxel);
            Increase(NewVoxel);
        }

        /// <summary>
        /// Decreases a voxel count
        /// </summary>
        private void Decrease(string OldVoxel)
        {
            if (VoxelCount.ContainsKey(OldVoxel))
            {
                VoxelCount[OldVoxel]--;
                // if voxel count == 0, remove from dictionary
                if (VoxelCount[OldVoxel] == 0)
                {
                    Remove(OldVoxel);
                }
            }
            else
            {
                Debug.LogError("Old Voxel: " + OldVoxel + " not in lookup table");
            }
        }

        /// <summary>
        /// Increase a voxels count
        /// </summary>
        private void Increase(string NewVoxel)
        {
            if (VoxelCount.ContainsKey(NewVoxel) == false)
            {
                Add(NewVoxel);
            }
            VoxelCount[NewVoxel]++;
        }
        #endregion

        #region Indexing
        // Example  MyLookupTable["Air"] == 0
        //          MyLookupTable["Dirt"] == 1
        /// <summary>
        /// Get an index corresponding to the value
        /// </summary>
        public int GetIndex(string VoxelName)
        {
            if (MyLookupTable.ContainsKey(VoxelName))
            {
                return MyLookupTable[VoxelName];
            }
            else
            {
                Add(VoxelName);
                return MyLookupTable[VoxelName];
            }
        }
        /// <summary>
        /// Get the name corresponding to the index value
        /// </summary>
        public string GetName(int MyValue)
        {
            if (MyReverseLookupTable.ContainsKey(MyValue))
            {
                return MyReverseLookupTable[MyValue];
            }
            /*else// if (MyLookupTable.ContainsValue(MyValue))
            {
                foreach (string MyKey in MyLookupTable.Keys)
                {
                    if (MyLookupTable[MyKey] == MyValue)
                    {
                        return MyKey;
                    }
                }
            }*/
            return "Air";   // default
        }
        #endregion

        #region ChunkUpdates
        /// <summary>
        /// Add Air to entire table
        /// When a chunk is created and initialized, call this from world
        /// </summary>
        public void OnAddChunk(Chunk MyChunk)
        {
            Add("Air"); // if isnt already in it
            VoxelCount["Air"] += Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;  // default count for a voxel!
            MyChunk.MyLookupTable.InitializeChunkTable();
        }
        /// <summary>
        /// When a new chunk is initialized
        /// </summary>
        public void InitializeChunkTable()
        {
            Clear();
            Add("Air"); // if isnt already in it
            VoxelCount["Air"] += Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;  // default count for a voxel!
        }
        /*public void Add(VoxelLookupTable OtherLookupTable)
        {
            Add("Air"); // if isnt already in it
            VoxelCount["Air"] += Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;  // default count for a voxel!
        }*/

        /// <summary>
        /// On remove chunk, decrease the keys
        /// </summary>
        public void OnRemoveChunk(Chunk MyChunk)
        {
            // reduce all the counts for every voxel type in the chunk
            //for (int i = 0; i < MyChunk.MyLookupTable.VoxelCount.Count; i++)
            foreach (string MyKey in MyChunk.MyLookupTable.MyLookupTable.Keys)
            {
                if (VoxelCount.ContainsKey(MyKey))
                {
                    VoxelCount[MyKey] -= MyChunk.MyLookupTable.VoxelCount[MyKey];
                    if (VoxelCount[MyKey] == 0)
                    {
                        Debug.Log("Removing Key: " + MyKey);
                        Remove(MyKey);
                    }
                    else if (VoxelCount[MyKey] < 0)
                    {
                        Debug.LogError("Table has spazzed out. It needs help.");
                    }
                }
                else
                {
                    Debug.LogError("Chunk contains a key the world does not [" + MyKey + "]");
                }
            }
        }
        #endregion

        #region Data

        /// <summary>
        /// Remove a voxelname key!
        /// Never removes air!
        /// </summary>
        private void Remove(string VoxelName)
        {
            if (MyLookupTable.ContainsKey(VoxelName) == true && VoxelName != "Air" && VoxelName != "Color")
            {
                MyReverseLookupTable.Remove(MyLookupTable[VoxelName]);
                MyLookupTable.Remove(VoxelName);
                VoxelCount.Remove(VoxelName);
            }
        }

        /// <summary>
        /// Add a new voxel to the table! Auto generates the index
        /// </summary>
        private void Add(string VoxelName)
        {
            if (MyLookupTable.ContainsKey(VoxelName) == false)
            {
                int NewValue = 0;  // find the first unused key
                bool IsFound = false;
                while (IsFound == false)
                {
                    if (MyReverseLookupTable.ContainsKey(NewValue))
                    {
                        NewValue++; // already has it, so increase until found new value
                    }
                    else
                    {
                        IsFound = true;
                    }
                }
                //Debug.Log("Adding new Voxel to lookuptable: " + VoxelName);
                MyLookupTable.Add(VoxelName, NewValue);
                MyReverseLookupTable.Add(NewValue, VoxelName);
                VoxelCount.Add(VoxelName, 0);
                // Instead of using Voxel Index, it will use the new key!
            }
        }

        public void AddName(string VoxelName, int VoxelMeshingIndex)
        {
            if (MyLookupTable.ContainsKey(VoxelName) == false && MyReverseLookupTable.ContainsKey(VoxelMeshingIndex) == false)
            {
                MyLookupTable.Add(VoxelName, VoxelMeshingIndex);
                MyReverseLookupTable.Add(VoxelMeshingIndex, VoxelName);
                VoxelCount.Add(VoxelName, 0);
            }
        }

        public bool ContainsVoxel(string VoxelName)
        {
            return MyLookupTable.ContainsKey(VoxelName);
        }

        #endregion

        #region File

        /// <summary>
        /// When world is loaded without lookup data, generate it
        /// This is used for save games
        /// </summary>
        public void Generate(World MyWorld)
        {
            Clear();
            Debug.Log("Loading World [" + MyWorld.name + "] without LookupTable Tag: Generating Lookup Table");
            for (int i = 0; i < DataManager.Get().GetSizeElements(DataFolderNames.Voxels); i++)
            {
                Add(DataManager.Get().GetName(DataFolderNames.Voxels, i));
            }
            foreach (KeyValuePair<Int3, Chunk> MyPair in MyWorld.MyChunkData)
            {
                Int3 ChunkPosition = Int3.Zero();
                for (ChunkPosition.x = 0; ChunkPosition.x < Chunk.ChunkSize; ChunkPosition.x++)
                {
                    for (ChunkPosition.y = 0; ChunkPosition.y < Chunk.ChunkSize; ChunkPosition.y++)
                    {
                        for (ChunkPosition.z = 0; ChunkPosition.z < Chunk.ChunkSize; ChunkPosition.z++)
                        {
                            Increase(DataManager.Get().GetName(DataFolderNames.Voxels, MyPair.Value.GetVoxelType(ChunkPosition)));
                        }
                    }
                }
            }
            //DebugLog();
        }

        /// <summary>
        /// Returns the script for the lookup table
        /// </summary>
        public List<string> GetScript()
        {
            List<string> MyScript = new List<string>();
            if (MyLookupTable.Count > 0)
            {
                MyScript.Add(BeginTag);
                foreach (string MyKey in MyLookupTable.Keys)
                {
                    MyScript.Add(("" + MyLookupTable[MyKey]) + SplitterTag + MyKey);
                }
                MyScript.Add(EndTag);
            }
            return MyScript;
        }

        /// <summary>
        /// Loads the lookup table
        /// </summary>
        public void RunScript(int AirCount, List<string> MyScript)
        {
            Clear();
            VoxelCount["Air"] = AirCount;// Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize;  // default count for a voxel!
            //Debug.Log(FileUtil.ConvertToSingle(MyScript));
            bool IsReading = false;
            for (int i = 0; i < MyScript.Count; i++)
            {
                if (IsReading == false)
                {
                    if (MyScript[i] == BeginTag)
                    {
                        IsReading = true;
                    }
                }
                else
                {
                    if (MyScript[i] == EndTag)
                    {
                        IsReading = false;
                        break;
                    }
                    else
                    {
                        // Example 0:Air
                        int MySplitIndex = MyScript[i].IndexOf(SplitterTag);
                        string ValueString = MyScript[i].Substring(0, MySplitIndex);
                        //Debug.Log("ValueSting: " + ValueString);
                        int MyValue = int.Parse(ValueString);
                        string MyKey = MyScript[i].Substring(MySplitIndex + 1); 
                        //Debug.Log("MyKey: " + MyKey);
                        if (MyLookupTable.ContainsKey(MyKey) == false && MyReverseLookupTable.ContainsKey(MyValue) == false)
                        {
                            MyLookupTable.Add(MyKey, MyValue);
                            MyReverseLookupTable.Add(MyValue, MyKey);
                            if (VoxelCount.ContainsKey(MyKey) == false)
                            {
                                VoxelCount.Add(MyKey, 0);
                            }
                        }
                    }
                }
            }
        }

        public void DebugLog()
        {
            int Index = 0;
            Debug.Log("Printing " + MyLookupTable.Count + " Lookup Voxels");
            foreach (KeyValuePair<string, int> MyPair in MyLookupTable)
            {
                Debug.Log(" [" + Index + "] ~LookupTable " + MyPair.Key + " [" + MyPair.Value + "]");
                Index++;
            }
            Debug.Log("Now printing " + DataManager.Get().GetSizeElements(DataFolderNames.Voxels) + " VoxelMetas");
            for (int i = 0; i < DataManager.Get().GetSizeElements(DataFolderNames.Voxels); i++)
            {
                Debug.Log(i + ": " + DataManager.Get().GetName(DataFolderNames.Voxels, i));
            }
        }
        #endregion

        #region Levels

        public void AddName(string VoxelName)
        {
            Add(VoxelName);
        }
        #endregion
    }
}

/// <summary>
/// Generate a table from whats already in the model
/// If List is empty, generate it!
/// </summary>
/*public void Generate(List<string> VoxelNames)
{
    MyLookupTable.Clear();
    for (int i = 0; i < VoxelNames.Count; i++)
    {
        MyLookupTable.Add(VoxelNames[i], i);
    }*
}*/
 