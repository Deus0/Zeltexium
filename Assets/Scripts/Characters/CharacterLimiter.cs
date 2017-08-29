using UnityEngine;
using System.Collections;
using Zeltex.AI;
using Zeltex.Guis;
using Zeltex.Skeletons;
using Zeltex.Voxels;

namespace Zeltex.Characters
{
    /// <summary>
    /// Limits character within map!
    /// </summary>
    /*public class CharacterLimiter : MonoBehaviour
    {
        private static bool IsFreeRoam;
        [SerializeField]
        private Vector3 PositionMinimum = new Vector3(0, 0, 0);
        [SerializeField]
        private Int3 PositionMaximum = new Int3(16 * 4 , 16 * 2, 16 * 4);
        [SerializeField]
        Vector3 MyChunkPosition = Vector3.zero;
        private Skeleton MySkeleton;
        private GameObject MyWorld;
        private bool HasLoaded = false;

        void Start()
        {
            if (!IsFreeRoam)
            {
                MyWorld = GameObject.Find("World");
                MySkeleton = transform.FindChild("Body").GetComponent<Skeleton>();
                StartCoroutine(LoadWorldSize());
            }
            else
            {
                enabled = false;
            }
        }
        void Update()
        {
            if (HasLoaded && MyWorld && Camera.main.gameObject.GetComponent<VoxelFreeRoam>().MyChunkPosition != MyChunkPosition)
            {
                MyChunkPosition = Camera.main.gameObject.GetComponent<VoxelFreeRoam>().MyChunkPosition;
                Vector3 HalfWorld = MyWorld.GetComponent<World>().WorldSize / 2f;
                HalfWorld.x = Mathf.CeilToInt(HalfWorld.x);
                HalfWorld.y = Mathf.CeilToInt(HalfWorld.y);
                HalfWorld.z = Mathf.CeilToInt(HalfWorld.z);
                PositionMinimum = (MyChunkPosition - HalfWorld) * Chunk.ChunkSize;
                PositionMaximum = (MyChunkPosition + HalfWorld) * Chunk.ChunkSize;
                PositionMinimum.y = 0;
                PositionMaximum.y = (MyWorld.GetComponent<World>().WorldSize * Chunk.ChunkSize).y;
            }
        }
        /// <summary>
        /// Load the worlds size
        /// </summary>
        IEnumerator LoadWorldSize()
        {
            yield return new WaitForSeconds(2);
            HasLoaded = false;
            int CheckCount = 0;
            while (!HasLoaded || CheckCount >= 1000)
            {
                CheckCount++;
                if (MyWorld)
                {
                    PositionMaximum = MyWorld.GetComponent<World>().WorldSize * Chunk.ChunkSize;
                    if (PositionMaximum.x != 0 && PositionMaximum.y != 0 && PositionMaximum.z != 0)
                    {
                        HasLoaded = true;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        void FixedUpdate()
        {
            if (HasLoaded && enabled)
            {
                Bounds MyBounds = MySkeleton.GetBounds();
                bool IsFallingThroughBottom = (transform.position.y <= PositionMinimum.y + MyBounds.extents.y);
                BasicController MyController = gameObject.GetComponent<BasicController>();
                if (MyController)
                {
                    //if (IsFallingThroughBottom)
                    //    MyController.SetGravityOffWithTime(0.05f);
                }
                // Clamp Position with grid, and within bounds of object
                transform.position = new Vector3(   Mathf.Clamp(transform.position.x, 
                                                            PositionMinimum.x + MyBounds.extents.x, 
                                                            PositionMaximum.x - MyBounds.extents.x),
                                                     Mathf.Clamp(transform.position.y,
                                                        PositionMinimum.y + MyBounds.extents.y, 
                                                        PositionMaximum.y - MyBounds.extents.y),
                                                     Mathf.Clamp(transform.position.z, 
                                                        PositionMinimum.z + MyBounds.extents.z, 
                                                        PositionMaximum.z - MyBounds.extents.z));

            }
        }
        // position limiter
        void OnDrawGizmosSelected()
        {
            if (DebugArea)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5F);
                Gizmos.DrawCube((PositionMinimum + PositionMaximum) / 2f,
                            PositionMaximum - PositionMinimum);
            }
            if (DebugBody)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5F);
                Gizmos.matrix = transform.localToWorldMatrix;   // align to body rotation
                //Gizmos.DrawCube(transform.position, BodySize);
                Gizmos.DrawCube(new Vector3(0,0,0), BodySize);
            }
        }
    }*/
}
