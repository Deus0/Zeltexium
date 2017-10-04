using UnityEngine;
using System.Collections;
using Zeltex.Items;

namespace Zeltex.Voxels 
{
	/// <summary>
    /// Main brush used to paint voxels
    /// </summary>
	public class VoxelBrush : MonoBehaviour
    {
        private static bool UnlimitedMode = true;
        [Header("Brush Settings")]
        [SerializeField]
        private bool IsPickaxe;
        [SerializeField]
        private float VoxelBrushSize = 0;
		[SerializeField]
        private string VoxelBrushType = "Air";
		[SerializeField]
        private int VoxelBrushRange = 15;
        private float LastUpdatedTime;
        private Inventory MyInventory;
        private Item MyItem;
        private int SelectedItemIndex;

        public void SetAsPickaxe()
        {
            IsPickaxe = true;
        }
        /// <summary>
        /// Use the item to place blocks!
        /// </summary>
        public void UpdateItem(Inventory NewInventory, Item NewItem, int NewSelectedItemIndex)
        {
            IsPickaxe = false;
            MyInventory = NewInventory;
            MyItem = NewItem;
            SelectedItemIndex = NewSelectedItemIndex;
        }
        /*public void UpdateItemType(string NewType)
        {
            VoxelBrushType = NewType;
        }*/

        public void UpdateBrushType(string NewBrushType)
		{
			VoxelBrushType = NewBrushType;
		}
		public void UpdateBrushSize(float NewVoxelBrushSize)
		{
			VoxelBrushSize = NewVoxelBrushSize;
		}
		public void Activate() 
		{
            if (MyItem == null)
            {
                UpdateItem(GetComponent<Characters.Character>().GetSkillbarItems(), 
                    GetComponent<Combat.Skillbar>().GetSelectedItem(), 
                    GetComponent<Combat.Skillbar>().GetSelectedIndex());
            }
            LastUpdatedTime = Time.realtimeSinceStartup;
            if (IsPickaxe)
            {
                    
                // i need to get back the amount of blocks required from this placement..
                UpdateBlockCamera("Air", VoxelBrushSize, VoxelBrushRange, gameObject);
            }
            else if (MyItem.GetQuantity() >= 1 || UnlimitedMode)
            {
                // i need to get back the amount of blocks required from this placement..
                UpdateBlockCamera(VoxelBrushType, VoxelBrushSize, VoxelBrushRange, gameObject);
                // whatever i just need to create a new block here based on those removed
                if (!UnlimitedMode)
                {
                    MyInventory.IncreaseQuantity(SelectedItemIndex, -1);
                }
            }
        }

        public void UpdateBlockCamera(string VoxelName, float BrushSize, float BrushRange)
        {
            UpdateBlockCamera(Camera.main, VoxelName, BrushSize, BrushRange, null);
        }

        public void UpdateBlockCamera(string VoxelName, float BrushSize, float BrushRange, GameObject MyCharacter)
        {
            UpdateBlockCamera(Camera.main, VoxelName, BrushSize, BrushRange, MyCharacter);
        }

        public void UpdateBlockCamera(Camera MyCamera, string VoxelName, float BrushSize, float BrushRange, GameObject MyCharacter)
        {
            UpdateBlockCamera2( VoxelName,
                                BrushSize,
                                BrushRange,
                                MyCamera.transform.position,
                                MyCamera.transform.forward,
                                MyCharacter);
        }

        public void UpdateBlockCamera2(string VoxelName, float BrushSize, float BrushRange, Vector3 RayOrigin, Vector3 RayDirection, GameObject MyCharacter)  // the character we are aiming from
        {
            // only characters layer
            var CharactersLayer = (1 << 15);
            // exclude characters layer
            var ExcludeCharactersLayer = ~CharactersLayer;
            RaycastHit MyHitCharacter;
            // Find out about hit character
            bool DoesHitCharacter = Physics.Raycast(RayOrigin, RayDirection, out MyHitCharacter, BrushRange, CharactersLayer);
            // if ray hits another character
            if (DoesHitCharacter && MyHitCharacter.collider.gameObject == MyCharacter)
            {
                DoesHitCharacter = false;
            }
            //Debug.LogError ("Updaating a block!");
            if (!DoesHitCharacter)  // if doesn't hit character, check for blocks collide
            {
                RaycastHit MyHit;
                if (Physics.Raycast(RayOrigin, RayDirection, out MyHit, BrushRange, ExcludeCharactersLayer))
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