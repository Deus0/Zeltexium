using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Zeltex.AnimationUtilities;
using Zeltex.Util;
using Zeltex.Guis;
using Zeltex.Characters;
using Zeltex.Combat;

namespace Zeltex.Items 
{
    /// <summary>
    /// Used for interaction with objects in the world
    /// A character ray traces the objects, and interacts with them
    /// </summary>
    public class ItemHandler : MonoBehaviour
	{
        #region Variables
        [Header("Interaction")]
		private bool HasActivated = false;
        public bool IsPickupOnCollide = false;
        [Tooltip("Is the item destroyed when picked up?")]
		public bool IsDestroyedOnPickup = true;
		[Tooltip("Is the item destroyed when collided")]
		public bool IsDestroyOnCollide = false;

        [Header("Item Pickup")]
        [Tooltip("Is the item is picked up?")]
        public bool IsItemPickup = true;
        [Tooltip("Added to the characters inventory when picked up")]
        public Item MyItem = new Item();

        [Header("Stats Pick")]
        [Tooltip("Is the item is picked up?")]
        public bool IsStatPickup = false;
        [Tooltip("The stat to add to the character")]
        public Stat MyStat = new Stat();
        [Header("Inspection")]
        [Tooltip("Used for the player to get information about an item")]
        public GameObject MyItemInspectPrefab;
        private GameObject SpawnedInspectGui;

        [Header("Sounds")]
        [Tooltip("Played when item is picked up")]
        public AudioClip MyPickupSound;

        [Header("Events")]
        public EventObject OnDestroyed;
        [Tooltip("Functions are called when item is interacted with (mouseclick)")]
		public EventObject OnItemInteract;
		[Tooltip("When character collides with the item")]
		public MyEvent2 OnCollision;
		
		[Header("SpecialEffects")]
		[Tooltip("The Particles created when item is picked up")]
		public GameObject ParticlesPrefab;
        #endregion

        #region Begin
        private void Awake()
        {
			StartCoroutine(AwakeRoutine());
        }

		private IEnumerator AwakeRoutine()
		{
			yield return null;
			ItemManager.Get().Add(this);
		}
        #endregion

        #region Destroy
        /// <summary>
        /// Used as loot items
        /// Character drops
        /// </summary>
        public void DestroyInTime(float MyTime)
        {
            StartCoroutine(Destroy2(MyTime));
        }
        IEnumerator Destroy2(float MyTime)
        {
            yield return new WaitForSeconds(MyTime);
            Destroy();
        }
        /// <summary>
        /// Remove it from any lists and destroy it
        /// </summary>
		public void Destroy()
        {
            OnDestroyed.Invoke(gameObject);
            ItemManager.Get().Remove(this);
            if (SpawnedInspectGui)
            {
                Destroy(SpawnedInspectGui);
            }
            Destroy(gameObject);
        }
        #endregion

        #region Using
        public bool HasUsed()
        {
            return HasActivated;
        }

        public void Use()
        {
            HasActivated = true;
        }

        public void Reset()
        {
            HasActivated = false;
        }
        #endregion

        #region ItemPickup
        /// <summary>
        /// Links the item to the item object
        /// </summary>
        public void SetItem(Item NewItem) 
		{
            MyItem = NewItem;
            RefreshMesh();
			UpdateItemInspectGui();
		}

		public Item GetItem() 
		{
			return MyItem;
        }

        /// <summary>
        /// For raycasting only!
        /// </summary>
		public void CharacterPickup(Character MyCharacter)
        {
            //Debug.LogError(MyCharacter.name + " is picking up " + MyItem.Name + ":" + name);
            if (IsItemPickup && HasUsed() == false)
            {
                if (OnItemInteract != null)
                {
                    OnItemInteract.Invoke(MyCharacter.gameObject);
                }
                //Debug.LogError(MyCharacter.name + " is still picking up " + MyItem.Name + ":" + name);
                Inventory MyInventory = MyCharacter.GetBackpackItems();
                if (MyInventory != null)
                {
                    MyInventory.PickupItem(this);
                }
                Activate();
            }
        }
        #endregion

        #region Activate
        /// <summary>
        /// When item is used up
        /// </summary>
        private void Activate() 
		{
            Use();
			if (ParticlesPrefab) // destruction particles
			{
				GameObject ItemLeftOver = (GameObject)Instantiate (ParticlesPrefab, transform.position, transform.rotation);
				ItemLeftOver.transform.localScale = transform.localScale;
				if (MyPickupSound && MyPickupSound != null)
                {
					AudioSource MySource = ItemLeftOver.AddComponent<AudioSource> ();
					MySource.PlayOneShot (MyPickupSound);
				}
				ItemLeftOver.AddComponent<ParticlesEmmisionOverLifetime> ();
			}
            Destroy ();
		}
        #endregion

        #region Collisions
        void OnTriggerEnter(Collider collision) 
		{
            OnContact(collision.gameObject);
		}

		void OnCollisionEnter(Collision collision) 
		{
			OnContact (collision.gameObject);
		}

        /// <summary>
        /// when character collides with item object
        /// </summary>
		public void OnContact(GameObject CollidingObject)
		{
            Character MyCharacter = CollidingObject.GetComponent<Character>();
            if (MyCharacter)
            {
                if (OnCollision != null)
                {
                    OnCollision.Invoke(gameObject, CollidingObject);
                }
                if (IsStatPickup)
                {
                    MyCharacter.GetStats().AddStatState(MyStat.Name, MyStat.GetValue());
                    Activate();
                }
                else if (IsPickupOnCollide)    // if collides with character
                {
                    CharacterPickup(MyCharacter);
                }
            }
        }
        #endregion

        #region Inspection

        public bool HasSpawnedGui()
        {
            return (SpawnedInspectGui != null);
        }
        public void ShowGui()
        {
            if (SpawnedInspectGui == null)
            {
                SpawnItemInspectGui();
            }
        }
        public void ToggleGui()
        {
            if (SpawnedInspectGui == null)
            {
                SpawnItemInspectGui();
            }
            else
            {
                SpawnedInspectGui.GetComponent<ZelGui>().Toggle();
                AnimateLine MyLineThing = SpawnedInspectGui.GetComponent<AnimateLine>();
                if (MyLineThing)
                    MyLineThing.SetVisibity(SpawnedInspectGui.GetComponent<ZelGui>().GetState());
            }
        }
        public void HideGui()
        {
            SetGuiVisible(false);
        }

        private void SetGuiVisible(bool IsVisible)
        {
            if (MyItemInspectPrefab)
            {
                SpawnedInspectGui.GetComponent<ZelGui>().SetState(IsVisible);
                AnimateLine MyLineThing = SpawnedInspectGui.GetComponent<AnimateLine>();
                if (MyLineThing)
                    MyLineThing.SetVisibity(IsVisible);
            }
        }
        /// <summary>
        /// Spawns the inspection gui
        /// </summary>
        public void SpawnItemInspectGui() 
		{
			if (MyItemInspectPrefab) 
			{
				SpawnedInspectGui = (GameObject) Instantiate(MyItemInspectPrefab, transform.position, Quaternion.identity);
				//SpawnedInspectGui.transform.SetParent(transform);
				Follower MyFollower = SpawnedInspectGui.GetComponent<Follower>();
				MyFollower.SetTarget(transform);
				AnimateLine MyLineThing = SpawnedInspectGui.GetComponent<AnimateLine>();
				MyLineThing.SetTarget(gameObject);
				SpawnedInspectGui.transform.localScale = new Vector3(
					(2f/transform.localScale.x)*0.001f/4f,
					(2f/transform.localScale.y)*0.001f/4f,
					(2f/transform.localScale.z)*0.001f/4f);
				UpdateItemInspectGui ();
			}
		}

        /// <summary>
        ///  Updates the text of the gui
        /// </summary>
        private void UpdateItemInspectGui() 
		{
			if (SpawnedInspectGui)
			{
				SpawnedInspectGui.name = MyItem.Name + "_InspectGui";
				Text MyLabelText = SpawnedInspectGui.transform.Find ("LabelText").GetComponent<Text> ();
				MyLabelText.text = MyItem.Name;
				Text MyDescriptionText = SpawnedInspectGui.transform.Find("DescriptionText").GetComponent<Text> ();
				MyDescriptionText.text = MyItem.GetDescription ();
			}
		}
        #endregion

        public void RefreshMesh()
        {
            if (MyItem == null || MyItem.MyModel == null || MyItem.MyModel.Name == "" || MyItem.MyModel.Name == "Empty")
            {
                DestroyWorld();
            }
            else
            {
                // Create world
                MyVoxelModelHandle = GetComponent<Voxels.World>();
                if (MyVoxelModelHandle == null)
                {
                    MyVoxelModelHandle = gameObject.AddComponent<Voxels.World>();
                }
                MyVoxelModelHandle.VoxelScale = new Vector3(0.01f, 0.01f, 0.01f);
                transform.localScale = new Vector3(1, 1, 1);
                MyVoxelModelHandle.RunScript(Zeltex.Util.FileUtil.ConvertToList(MyItem.MyModel.VoxelData));
                return; // no need to check other meshes
            }
            // 
            if (MyItem == null || MyItem.MyPolyModel == null || MyItem.MyPolyModel.Name == "" || MyItem.MyPolyModel.Name == "Empty")
            {
                DestroyPolyHandle();
            }
            else
            {
                MyPolyHandle = GetComponent<Voxels.PolyModelHandle>();
                if (MyPolyHandle == null)
                {
                    MyPolyHandle = gameObject.AddComponent<Voxels.PolyModelHandle>();
                }
                transform.localScale = 0.2f * (new Vector3(1, 1, 1));
                MyPolyHandle.LoadVoxelMesh(MyItem.MyPolyModel, MyItem.TextureMapIndex);
            }
        }

        private Voxels.PolyModelHandle MyPolyHandle;
        private Voxels.World MyVoxelModelHandle;

        private void DestroyPolyHandle() 
        {
            MyPolyHandle = GetComponent<Voxels.PolyModelHandle>();
            if (MyPolyHandle)
            {
                Destroy(MyPolyHandle);
            }
        }

        private void DestroyWorld() 
        {
            MyVoxelModelHandle = GetComponent<Voxels.World>();
            if (MyVoxelModelHandle) 
            {
                Destroy(MyVoxelModelHandle);
            }
        }
    }
}
