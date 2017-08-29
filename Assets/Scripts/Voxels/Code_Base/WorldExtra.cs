using UnityEngine;
using System.Collections;

namespace Zeltex.Voxels
{

    public static class WorldExtra
    {

        /*

        public static Chunk GetChunkFromPosition(World MyWorld, Vector3 WorldPosition)
        {
            Int3 ChunkPosition = new Int3(GetChunkPosition(MyWorld, WorldPosition));
            return MyWorld.GetChunk(ChunkPosition);
        }
        */


        /*public static void UpdateBlockCamera4(Transform MyCamera, int NewType)
        {
            UpdateBlockCamera3(MyCamera, NewType, 1, 20);
        }

        public static void UpdateBlockCamera3(Transform MyCamera, int NewType, float BrushSize, float BrushRange)  // the character we are aiming from
        {
            // only characters layer
            var CharactersLayer = (1 << 15);
            // exclude characters layer
            var ExcludeCharactersLayer = ~CharactersLayer;
            RaycastHit MyHit;
            if (Physics.Raycast(MyCamera.position, MyCamera.forward, out MyHit, BrushRange, ExcludeCharactersLayer))
            {
                if (MyHit.collider.gameObject.transform.parent)
                {
                    World MyWorld = MyHit.collider.gameObject.transform.parent.GetComponent<World>();
                    if (MyWorld)
                    {
                        MyWorld.UpdateBlockCamera5(MyHit, NewType, BrushSize);
                    }
                }
            }
            return;
        }*/
    }
}
