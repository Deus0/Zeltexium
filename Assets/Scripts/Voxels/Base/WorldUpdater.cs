using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniversalCoroutine;

namespace Zeltex.Voxels 
{
    /// <summary>
    /// Update the chunk mesh data
    /// Send and upload to the gpu
    /// Update Cycle -> Primary Chunk, Outer chunks for light, Primary Chunk Outer part! (remove all light propogation from inner chunk, out of outer chunk)
    /// Or maybe just reverse propogate light(in primary chunk), where it goes darkness to bright, and then update the surrounding chunks again
    /// </summary>
    [ExecuteInEditMode]
	public class WorldUpdater : ManagerBase<WorldUpdater>
    {
        #region Variables
		public EditorAction ActionReset = new EditorAction();
        [Header("Options")]
        public bool IsCollectOnUpdate = false;
        public static bool IsReversed = false;
		public List<Chunk> MyChunks = new List<Chunk>();
		private Zeltine MyHandle;
        [Header("Debug")]
        public bool IsUpdating = false;
        public bool IsUpdatingMesh = false;
        private Chunk UpdatingChunk = null;

        public new static WorldUpdater Get()
        {
            if (MyManager == null)
            {
                GameObject LayerManagerObject = GameObject.Find("WorldUpdater");
                if (LayerManagerObject)
                {
                    MyManager = LayerManagerObject.GetComponent<WorldUpdater>();
                }
                else
                {
                    Debug.LogError("Could not find [LayerManager].");
                }
            }
            return MyManager;
        }
		#endregion

		private void Update()
		{
			InitializeUpdates();
			if (ActionReset.IsTriggered())
			{
				MyHandle = null;
			}
		}

        #region Misc

        /// <summary>
        /// Clears the thread
        /// </summary>
        public void Clear(World MyWorld) 
		{
            int Removed = 0;
            for (int i = MyChunks.Count-1; i >= 0; i--)
            {
                if (MyChunks[i] == null || MyChunks[i].GetWorld() == MyWorld)
                {
                    MyChunks.RemoveAt(i);
                    Removed++;
                }
            }
            if (MyChunks.Count == 0)
            {
                IsUpdating = false;
            }
        }

        /// <summary>
        /// Gets closest chunk to camera
        /// </summary>
		public void GetClosestChunk(out Chunk MyChunk) 
		{
            MyChunk =  MyChunks[0];
            /*MyChunk = null;
            if (MyChunks != null && MyChunks.Count != 0)
			{
                int ChunkIndex = -1;
                MyChunk = null;// MyChunks [ChunkIndex];
			    float DistanceToCamera = 10000;	//Vector3.Distance (MyChunk.transform.position, Camera.main.transform.position);
                if (IsReversed)
                {
                    DistanceToCamera = 0;   // least close distance to cam
                }
                // Check list ofr null first
                for (int i = MyChunks.Count - 1; i >= 0; i--)
                {
                    if (MyChunks[i] == null)
                    {
                        Debug.LogError("Removing null chunk at: " + i);
                        MyChunks.RemoveAt(i);
                        //ChunkIndex--;
                    }
                }
                // find closest chunk to camera
                if (MyChunks.Count > 0)
                {
			        for (int i = 0; i < MyChunks.Count; i++) 
			        {
					    float CurrentDistance = Vector3.Distance (MyChunks [i].transform.position, Camera.main.transform.position);
					    if ((!IsReversed && CurrentDistance < DistanceToCamera)
                            || (IsReversed && CurrentDistance < DistanceToCamera)) 
					    {
						    DistanceToCamera = CurrentDistance;
						    MyChunk = MyChunks [i];
						    ChunkIndex = i;
					    }
                    }
                    if (ChunkIndex == -1)
                    {
                        Debug.LogError ("NoChunks!");
                        return;
                    }
                    else
                    {
                        //MyChunks.RemoveAt(ChunkIndex);
                    }
                }
            }*/
            //return MyChunk;
        }
        #endregion

        #region Updating

        void InitializeUpdates()
        {
            if (MyChunks.Count == 0)
            {
                MyHandle = null;
            }
            if (MyChunks.Count > 0 &&
                (MyHandle == null || MyHandle.IsUpdating() == false))
            {
                IsUpdating = true;
                MyHandle = RoutineManager.Get().StartCoroutine(MainUpdate());
                Debug.LogError("Success to initialize WorldUpdater");
            }
        }

        /// <summary>
        /// Add a chunk onto the update list! Only if not already in it!
        /// </summary>
        public void Add(Chunk MyChunk)
        {
            // if already added chunk to updater, return
            if (MyChunks.Contains(MyChunk) == false)// && BuildingChunks.Contains(MyChunk) == false)
            {
                //Debug.LogError("Adding Chunk: " + MyChunk.name);
                MyChunks.Add(MyChunk);
                InitializeUpdates();
            }
        }

        IEnumerator MainUpdate()
        {
            float TimeStarted = Time.realtimeSinceStartup;
            while (MyChunks.Count > 0)
            {
                IsUpdatingMesh = true;
                float TimeStartedChunk = Time.realtimeSinceStartup;
                UpdatingChunk = null;
                GetClosestChunk(out UpdatingChunk);
                if (UpdatingChunk)
                {
                    float TimeBegun = Time.realtimeSinceStartup;
                    MyChunks.Remove(UpdatingChunk);
                    UpdatingChunk.PreWorldBuilderBuildMesh();
                    yield return UpdatingChunk.BuildChunkMesh();
                    /*if (LogManager.Get())
                    {
                        string DebugLine = UpdatingChunk.name + "[" + Mathf.RoundToInt((Time.realtimeSinceStartup - TimeStartedChunk) * 1000) + "]";
                        LogManager.Get().Log(DebugLine, "WorldUpdater");
                    }*/
                }
                //Debug.LogError("Time Taken for Chunk [" + Mathf.RoundToInt((Time.realtimeSinceStartup - TimeBegunUpdating)*1000) + "ms]");
                IsUpdatingMesh = false;
            }
            if (LogManager.Get())
            {
                string DebugLine2 = name + ":Time[" + Mathf.RoundToInt((Time.realtimeSinceStartup - TimeStarted) * 1000) + "]";
                LogManager.Get().Log(DebugLine2, "WorldUpdater");
            }
            /*if (IsCollectOnUpdate)
            {
                System.GC.Collect();    // force collect after remeshed
            }*/
            IsUpdating = false;
            MyHandle = null;
            //yield return null;
            //	Debug.LogError ("Total Time [" + ((int)LoadTimes[LoadTimes.Count-1]) + "ms]");
        }
        
        #endregion
    }
}