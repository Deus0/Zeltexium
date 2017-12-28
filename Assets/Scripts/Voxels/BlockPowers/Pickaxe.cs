using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Pick the ores
    /// </summary>
    public class Pickaxe : MonoBehaviour
    {
        private Character MyCharacter;
        private bool IsPicking;
        private float PickTime = 1f;
        private float TimeStartedPicking;
        private bool WasMouseUsed;

        private void Awake()
        {
            MyCharacter = GetComponent<Characters.Character>();
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (IsPicking == false)
            {
                WasMouseUsed = Input.GetButtonDown("Fire1");
                if (WasMouseUsed || Input.GetAxis("Fire1") >= 0.5f)
                {
                    StartPicking();
                }
            }
            else
            {
                ContinuePicking();
            }
        }

        private void StartPicking()
        {
            TimeStartedPicking = Time.time;
            IsPicking = true;
        }

        private void ContinuePicking()
        {
            if (Time.time - TimeStartedPicking >= PickTime)
            {
                EndPicking();
            }
            else
            {
                // If stops aiming at voxel, or key is released, end picking
                if (!IsAimingAtVoxel() || 
                    (WasMouseUsed && Input.GetButtonUp("Fire1")) ||
                    (!WasMouseUsed && Input.GetAxis("Fire1") >= 0.5f))
                {
                    EndPicking();
                }
            }
        }

        private void EndPicking()
        {
            if (Time.time - TimeStartedPicking >= PickTime)
            {
                PickTheVoxel();
            }
            IsPicking = false;
        }

        private bool IsAimingAtVoxel()
        {
            return true;
        }

        private void PickTheVoxel()
        {
            UpdateBlockCamera("Air", 0, 8, gameObject);
        }

        public void UpdateBlockCamera(string VoxelName, float BrushSize, float BrushRange, GameObject MyCharacter)
        {
            UpdateBlockCamera(CameraManager.Get().GetMainCamera(), VoxelName, BrushSize, BrushRange, MyCharacter);
        }

        public void UpdateBlockCamera(Camera MyCamera, string VoxelName, float BrushSize, float BrushRange, GameObject MyCharacter)
        {
            if (MyCamera)
            {
                UpdateBlockCamera2(VoxelName,
                                    BrushSize,
                                    BrushRange,
                                    MyCamera.transform.position,
                                    MyCamera.transform.forward,
                                    MyCharacter);
            }
            else
            {
                Debug.LogError("Could not get a camera with voxelbrush: " + name);
            }
        }

        public void UpdateBlockCamera2(string VoxelName, float BrushSize, float BrushRange, Vector3 RayOrigin, Vector3 RayDirection, GameObject MyCharacter)  // the character we are aiming from
        {
            // only characters layer
            var CharactersLayer = (1 << 15);
            // exclude characters layer
            var ExcludeCharactersLayer = ~CharactersLayer;
            RaycastHit MyHitCharacter;
            // Find out about hit character
            bool DoesHitCharacter = UnityEngine.Physics.Raycast(RayOrigin, RayDirection, out MyHitCharacter, BrushRange, CharactersLayer);
            // if ray hits another character
            if (DoesHitCharacter && MyHitCharacter.collider.gameObject == MyCharacter)
            {
                DoesHitCharacter = false;
            }
            //Debug.LogError ("Updaating a block!");
            if (!DoesHitCharacter)  // if doesn't hit character, check for blocks collide
            {
                RaycastHit MyHit;
                if (UnityEngine.Physics.Raycast(RayOrigin, RayDirection, out MyHit, BrushRange, ExcludeCharactersLayer))
                {
                    Chunk MyChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                    //Debug.LogError("Hit object: " + MyHit.collider.transform.parent.name);
                    //if (MyHit.collider.transform.parent != null && MyHit.collider.transform.parent.gameObject == gameObject)
                    if (MyChunk)
                    {
                        World MyWorld = MyChunk.GetWorld();
                        MyWorld.IsDropItems = true;
                        Vector3 BlockPosition = MyWorld.RayHitToBlockPosition(MyHit.point, MyHit.normal, (VoxelName == "Air"));
                        MyWorld.UpdateBlockType(VoxelName, BlockPosition, BrushSize, Color.white);
                        MyWorld.IsDropItems = false;
                    }
                }
            }
        }
    }
}
