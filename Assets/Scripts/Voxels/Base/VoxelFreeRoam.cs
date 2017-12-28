using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;
using Zeltex.AI;
using Zeltex.Game;
using Zeltex;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Loads chunks around camera when entering a new chunk
    /// </summary>
    public class VoxelFreeRoam : MonoBehaviour
    {
        #region Variables
        public World MyWorld;
        public bool IsFreeRoam = false;
        public float Speed = 10;
        public bool IsAutoMoveForward;
        public Vector3 MyPosition = new Vector3();
        public Int3 MyChunkPosition = new Int3();
        public KeyCode AutoMoveKey = KeyCode.Numlock;
        private static VoxelFreeRoam Instance;

        public static VoxelFreeRoam Get()
        {
			if (Instance == null)
			{
				Instance = Camera.main.gameObject.AddComponent<VoxelFreeRoam>();
			}
            return Instance;
        }

        public void BeginRoaming()
        {
            if (!enabled)
            {
                enabled = true;
                StartCoroutine(BeginRoamingInTime(4f));
            }
        }

        public IEnumerator BeginRoamingInTime(float TimeOffset)
        {
            yield return new WaitForSeconds(TimeOffset);
            if (MyWorld == null)
            {
                MyWorld = WorldManager.Get().MyWorlds[0];
            }
            MyChunkPosition = MyWorld.GetChunkPosition(transform.position);
            IsFreeRoam = true;
            //Camera.main.transform.position = Vector3.zero;
        }
        #endregion

        #region Mono
        private void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (IsFreeRoam && MyWorld)
            {
                if (Input.GetKeyDown(AutoMoveKey))
                {
                    IsAutoMoveForward = !IsAutoMoveForward;
                    if (IsAutoMoveForward)
                    {
                        GetComponent<Player>().SetInput(false);
                    }
                }
                UpdateFreeRoam();
                if (IsAutoMoveForward)
                {
                    if (GetComponent<Player>().GetCharacter() != null)
                    {
                        GetComponent<Player>().GetCharacter().GetComponent<BasicController>().Input(0, 3, false);    // keep moving forward
                    }
                    else
                    {
                        Vector3 NewPosition = Camera.main.transform.position + Camera.main.transform.TransformDirection(Vector3.forward);
                        NewPosition.y = Camera.main.transform.position.y;
                        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, NewPosition, Time.deltaTime * Speed);
                    }
                }
            }
            /*if (Input.GetKeyDown(KeyCode.K))
            {
                IsFreeRoam = true;
                if (IsFreeRoam)
                {
                    Camera.main.transform.position = (MyWorld.GetWorldSize()) / 2f;
                }
                IsAutoMoveForward = true;
            }*/
        }
        #endregion

        #region Roaming
        /// <summary>
        /// Begin free roaming
        /// </summary>
        private void BeginFreeRoam()
        {
            // make sure all chunks are inside render distance
            OnUpdatedChunk();
        }
        /// <summary>
        /// Check for changes in chunk position
        /// </summary>
        private void UpdateFreeRoam()
        {
            Vector3 NewPosition = MyWorld.RayHitToBlockPosition(transform.position, Vector3.zero);
            if (NewPosition != MyPosition)
            {
                MyPosition = NewPosition;
                OnUpdatedChunk();
            }
        }
        /// <summary>
        /// When a new chunk position is found
        /// </summary>
        private void OnUpdatedChunk()
        {
            if (IsFreeRoam)
            {
                Int3 NewChunkPosition = MyWorld.GetChunkPosition(transform.position);
                if (MyChunkPosition != NewChunkPosition)
                {
                    Int3 DifferenceChunk = NewChunkPosition - MyChunkPosition;
                    if (VoxelDebugger.IsDebug)
                    {
                        VoxelDebugger.Get().AddNewChunk("[" + Time.realtimeSinceStartup + "] Moved- [" + NewChunkPosition.ToString() + " from " + MyChunkPosition.ToString() + " - Difference:" + DifferenceChunk.ToString());
                    }
                    StartCoroutine(MyWorld.SetPositionOffset(NewChunkPosition));
                    
                    //StartCoroutine();
                }
                MyChunkPosition = NewChunkPosition;
            }
        }
        #endregion
    }

}

/*void OnGUI()
{
    if (IsFreeRoam)
    {
        if (IsAutoMoveForward)
        {
            GUILayout.Label("FreeRoaming - AutoMoving.");
        }
        else
        {
            GUILayout.Label("FreeRoaming - Free Movement.");
        }
    }
    else
    {
        GUILayout.Label("Press K to enable Free Roam");
    }
}*/
