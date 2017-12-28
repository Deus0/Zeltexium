using UnityEngine;
using System.Collections;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Finds thing
    /// </summary>
    public class SpawnPositionFinder : MonoBehaviour
    {
        public World MyWorld;
        public bool IsRandom = true;
        public bool IsFindClosest;
        public bool IsFindNew;

        void Update()
        {
            if (IsFindNew)
            {
                IsFindNew = false;
                transform.position = FindNewPosition();
            }
        }

        public Vector3 FindNewPosition()
        {
            transform.localScale = new Vector3(
                MyWorld.transform.lossyScale.x * MyWorld.VoxelScale.x,
                MyWorld.transform.lossyScale.y * MyWorld.VoxelScale.y,
                MyWorld.transform.lossyScale.z * MyWorld.VoxelScale.z);
            Vector3 SpawnPosition = FindNewPosition(MyWorld);
            transform.position = SpawnPosition;
            return SpawnPosition;
        }

        public static Vector3 FindNewPositionChunkBoundaries(World MyWorld)
        {
            Vector3 Minimum = new Vector3(0, 0, 0);
            Vector3 Maximum = new Vector3(
                Chunk.ChunkSize,
                Chunk.ChunkSize - 1,
                Chunk.ChunkSize);
            return FindNewPosition(MyWorld, Minimum, Maximum);
        }

        /// <summary>
        /// Used for custom map
        /// </summary>
        public static Vector3 FindNewPositionWorldBoundaries(World MyWorld)
        {
            Vector3 Minimum = new Vector3(0, 0, 0);
            Vector3 Maximum = MyWorld.GetWorldBlockSize();
            Maximum.y--;
            return FindNewPosition(MyWorld, Minimum, Maximum);
        }

        public static Vector3 FindClosestPositionInChunk(World MyWorld, Int3 SpawnPosition)
        {
            Int3 MyChunkPosition = MyWorld.GetChunkPosition(SpawnPosition);//Camera.main.gameObject.GetComponent<VoxelFreeRoam>().MyChunkPosition;
            
            Vector3 Minimum = MyChunkPosition.ChunkToBlockPosition().GetVector();
            Vector3 Maximum = Minimum + new Vector3(16, 0, 16);// new Vector3(16 * 4, 16 * 1 - 1, 16 * 4);
            Maximum.y = MyWorld.GetWorldBlockSize().y;
            return FindNewPosition(MyWorld, Minimum, Maximum);
        }

        /// <summary>
        /// Used by VoxelRoam class
        /// </summary>
        public static Vector3 FindNewPosition(World MyWorld)
        {
            return FindNewPosition(MyWorld, Camera.main.transform);
        }

        public static Vector3 FindNewPosition(World MyWorld, Transform MyTransform)
        {
            //Int3 MyChunkPosition = MyWorld.GetChunkPosition(MyTransform.position);//Camera.main.gameObject.GetComponent<VoxelFreeRoam>().MyChunkPosition;
            Vector3 Minimum = (MyWorld.PositionOffset * Chunk.ChunkSize).GetVector() - MyWorld.GetHalfSize() * Chunk.ChunkSize;// new Vector3(0, 0, 0);
            Vector3 Maximum = (MyWorld.PositionOffset * Chunk.ChunkSize).GetVector() + MyWorld.GetHalfSize() * Chunk.ChunkSize; //new Vector3(16 * 4, 16 * 1 - 1, 16 * 4);
            /*Int3 HalfWorld = new Int3(MyWorld.GetComponent<World>().WorldSize * 0.5f);
            HalfWorld.x = Mathf.CeilToInt(HalfWorld.x);
            HalfWorld.y = Mathf.CeilToInt(HalfWorld.y);
            HalfWorld.z = Mathf.CeilToInt(HalfWorld.z);*/
            //Minimum = ((MyChunkPosition - HalfWorld) * Chunk.ChunkSize).GetVector();
            //Maximum = ((MyChunkPosition + HalfWorld) * Chunk.ChunkSize).GetVector();
            //Minimum.y = 0;
            //Maximum.y = (MyWorld.GetComponent<World>().WorldSize * Chunk.ChunkSize).y;
            return FindNewPosition(MyWorld, Minimum, Maximum);
        }
        public static int MaxChecks = 50000;
        public static Vector3 FindNewPosition(World MyWorld, Vector3 Minimum, Vector3 Maximum)
        {
            int Checks = 0;
            //Debug.LogError("Checking position in world: " + MyWorld.name + " with min: " + Minimum.ToString() + " and max: " + Maximum.ToString());
            Vector3 VoxelUnit = MyWorld.GetUnit();  // real world unity scale
            while (Checks <= MaxChecks)
            {
                Int3 NewPosition = new Int3(    Mathf.RoundToInt(Random.Range(Minimum.x, Maximum.x)),
                                                 Mathf.RoundToInt(Random.Range(Minimum.y, Maximum.y)),
                                                 Mathf.RoundToInt(Random.Range(Minimum.z, Maximum.z)));
                if (MyWorld.GetVoxelType(NewPosition) == 0)// && MyWorld.GetVoxelType(NewPosition.Above()) == 0)    // for the entire area of the zone
                {
                    if (MyWorld.GetVoxelType(NewPosition.Below()) != 0)  // if below is ground
                    {
                        Vector3 MyPosition = MyWorld.BlockToRealPosition(NewPosition.GetVector()) + VoxelUnit / 2f;
                        //Debug.LogError("Found position: " + MyPosition.ToString() + " --- with voxel unit of: " + VoxelUnit.ToString() + 
                        //    "\n Original position: " + NewPosition.GetVector().ToString() + " --- VoxelScale: " + MyWorld.VoxelScale.ToString());
                        return MyPosition;
                    }
                }
                Checks++;
            }
            Debug.LogError("Could not find position in world: " + MyWorld.name + " with " + MaxChecks + " checks.");
            return MyWorld.transform.position + MyWorld.GetWorldSize() / 2f;
            //return new Vector3(8, 35, 8);   // really tall in first chunk
        }
    }
}


/*Vector3 MyPosition = NewPosition + new Vector3(0.5f, 0.5f, 0.5f);    // new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z)/2f;
MyPosition = new Vector3(
    MyPosition.x * MyWorld.transform.lossyScale.x * MyWorld.VoxelScale.x,
    MyPosition.y * MyWorld.transform.lossyScale.y * MyWorld.VoxelScale.y,
    MyPosition.z * MyWorld.transform.lossyScale.z * MyWorld.VoxelScale.z
);
//IsRandom = false;
return MyPosition;*/
/*if (IsFindClosest)
{
    //Debug.Log("SpawnPoint at: " + transform.position.ToString());
    Vector3 SpawnPosition = MyWorld.BlockToRealPosition(MyWorld.FindClosestVoxelPosition(MyWorld.RealToBlockPosition(transform.position), 0, 12));
    SpawnPosition += new Vector3(1, 1, 1) * 0.25f;
    return SpawnPosition;
}
// find random position
else
{

}*/
