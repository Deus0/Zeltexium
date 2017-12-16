using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Guis.Maker;

namespace Zeltex.Items 
{
    /// <summary>
    /// Manages Item Data for the map
    /// </summary>
	public class ItemManager : ManagerBase<ItemManager> 
	{
        #region Variables
        public List<ItemObject> MyItemObjects = new List<ItemObject>();
        //[Header("References")]
        //public TextureMaker MyTextureManager;
		//[Tooltip("A list of the items that will be used in the game")]
		//public List<Item> MyItems;    // use inventory i nstead
        //public Texture DefaultTexture;
        public Texture EmptyTexture;
        public List<Mesh> MyMeshes;
        public List<Material> MyMaterials;
        public GameObject ItemObjectPrefab;
        #endregion

        #region Static

        public new static ItemManager Get()
        {
            if (MyManager == null)
            {
                GameObject ManagerObject = GameObject.Find("ItemManager");
                if (ManagerObject)
                {
                    MyManager = ManagerObject.GetComponent<ItemManager>();
                }
                else
                {
                    Debug.LogError("Could not find ItemManager [ItemManager].");
                }
            }
            return MyManager;
        }
        #endregion

        #region ItemMeshes
        public Material GetMaterial(string MeshName)
        {
            for (int i = 0; i < MyMaterials.Count; i++)
            {
                if (MyMaterials[i])
                {
                    if (MyMaterials[i].name == MeshName)
                    {
                        return MyMaterials[i];
                    }
                }
                else
                {
                    Debug.LogError(i + " Material in Item Manager is null.");
                }
            }
            return null;
        }
        /// <summary>
        /// Gets an item mesh with a name
        /// </summary>
        public Mesh GetMesh(string MeshName)
        {
            for (int i = 0; i < MyMeshes.Count; i++)
            {
                if (MyMeshes[i])
                {
                    if (MyMeshes[i].name == MeshName)
                    {
                        return MyMeshes[i];
                    }
                }
                else
                {
                    Debug.LogError(i + " Mesh in Item Manager is null.");
                }
            }
            return null;
        }
        #endregion

        #region ItemObjects

        /// <summary>
        /// Spawns an item from a drop transform
        /// </summary>
        public GameObject SpawnItem(Transform DropTransform, Item MyItem)
        {
            if (ItemObjectPrefab == null)
            {
                return null;
            }
            GameObject ItemWorld;
            if (DropTransform)
            {
                ItemWorld = (GameObject)Instantiate(ItemObjectPrefab, 
                    DropTransform.TransformPoint(new Vector3(0, 0, 0.5f)),
                    DropTransform.rotation);
            }
            else
            {
                ItemWorld = (GameObject)Instantiate(ItemObjectPrefab, Vector3.zero, Quaternion.identity);
            }
            ItemWorld.transform.SetParent(GameObject.Find("ItemObjects").transform);
            ItemWorld.name = MyItem.Name;
            ItemWorld.transform.eulerAngles += new Vector3(0, 180, 0);
            if (MyItem.MeshType == ItemMeshType.VoxelReference || MyItem.MeshType == ItemMeshType.Voxel)
            {
                World MyItemWorld = ItemWorld.AddComponent<World>();
                MyItemWorld.IsConvex = true;
                MyItemWorld.MyDataBase = GameObject.Find("World").GetComponent<World>().MyDataBase;
                MyItemWorld.MyUpdater = GameObject.Find("World").GetComponent<World>().MyUpdater;
                MyItemWorld.MyMaterials = GameObject.Find("World").GetComponent<World>().MyMaterials;
                //StartCoroutine(LoadItemInWorld(MyItemWorld, Zeltex.Util.FileUtil.ConvertToList(MyItem.MyModel)));
            }
            else
            {
                // Make into mesh
                MeshFilter MyFilter = ItemWorld.AddComponent<MeshFilter>();
                MeshCollider MyMeshCollider = ItemWorld.AddComponent<MeshCollider>();
                MeshRenderer MyRenderer = ItemWorld.AddComponent<MeshRenderer>();
                if (MyFilter)
                {
                    //Debug.LogError("Getting mesh for item: " + MyItem.Name + " of mesh name: " + MyItem.MeshName);
                    Mesh NewMesh = MyItem.GetMesh();
                    if (NewMesh != null)
                    {
                        MyFilter.sharedMesh = NewMesh;
                        MyMeshCollider.sharedMesh = NewMesh;
                    }
                }
                if (MyRenderer)
                {
                    MyRenderer.material = GameObject.Find("World").GetComponent<Zeltex.Voxels.World>().MyMaterials[0];
                    //MyRenderer.material = MyItem.GetMaterial();
                }
            }
            ItemObject MyItemObject = ItemWorld.GetComponent<ItemObject>();
            MyItemObject.SetItem(MyItem.Clone<Item>());
            MyItemObject.DestroyInTime(60 + Random.Range(-30, 30));
            Add(MyItemObject);
            return ItemWorld;
        }

        IEnumerator LoadItemInWorld(World ItemWorld, List<string> MyScript)
        {
            ItemWorld.GetComponent<Rigidbody>().isKinematic = true;
            yield return ItemWorld.RunScriptRoutine(MyScript);
            ItemWorld.GetComponent<Rigidbody>().isKinematic = false;
        }

        /// <summary>
        /// Add to list when an item is spawned to world
        /// </summary>
        public void Add(ItemObject NewItemObject)
        {
            if (MyItemObjects.Contains(NewItemObject) == false)
            {
                MyItemObjects.Add(NewItemObject);
            }
        }

        /// <summary>
        /// Remove from list when an item is removed from world
        /// </summary>
        public void Remove(ItemObject MyItemObject) 
		{
            if (MyItemObjects.Contains(MyItemObject))
            {
                MyItemObjects.Remove(MyItemObject);
            }
		}

        /// <summary>
        /// Clear all items from world
        /// </summary>
		public void Clear() 
		{
			for (int i = 0; i < MyItemObjects.Count; i++) 
			{
				if (MyItemObjects [i])
                {
                    MyItemObjects[i].Destroy();
                }
			}
			MyItemObjects.Clear ();
		}
        #endregion
    }
}


/*public void GatherItemsFromScene() 
{
    MyItems.Clear ();
    GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
    foreach (GameObject MyObject in AllObjects) {
        //if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None) 
        {
            if (MyObject.GetComponent<ItemObject> ()) {
                CheckForItemAdds (MyObject.GetComponent<ItemObject> ().GetItem());	
            } else if (MyObject.GetComponent<Inventory> ()) {
                Inventory MyInventory = MyObject.GetComponent<Inventory> ();
                for (int j = 0; j < MyInventory.MyItems.Count; j++) {
                    CheckForItemAdds (MyInventory.MyItems [j]);
                }
            }
        }
    }
}*/

/*private void CheckForItemAdds(Item MyItem)
{
    for (int i = 0; i < MyItems.Count; i++) 
    {
        if (MyItems[i].Name == MyItem.Name) 
        {
            return;
        }
    }
    MyItems.Add (MyItem);
}*/

/*public void UpdateAllItems() 
{
    GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
    foreach (GameObject MyObject in AllObjects)
    {
        //if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None) 
        {
            if (MyObject.GetComponent<ItemObject> ()) {
                Item MyItem = MyObject.GetComponent<ItemObject> ().GetItem();
                CheckItemForReplace (MyItem);	
            } else if (MyObject.GetComponent<Inventory> ()) {
                Inventory MyInventory = MyObject.GetComponent<Inventory> ();

                for (int j = 0; j < MyInventory.GetSize(); j++) {
                    Item MyItem = MyInventory.MyItems [j];
                    CheckItemForReplace (MyItem);
                }
            }
        }
    }
}*/
/*private void CheckItemForReplace(Item NewItem) 
{
    for (int i = 0; i < MyItems.Count; i++)
    {
        if (Item.ReplaceItem(NewItem, MyItems[i]))
            i = MyItems.Count;
    }
}*/
/*public void UpdateAllStats() 
{
    UpdateItemsWithStatData ();
    UpdateAllItems ();

    GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
    foreach (GameObject MyObject in AllObjects)
    {
        CharacterStats MyCharacterStats = MyObject.GetComponent<CharacterStats> ();
        if (MyCharacterStats)
        {
            for (int i = 0; i < MyStats.GetSize(); i++) 
            {
                MyCharacterStats.BaseStats.ReplaceStatData(MyStats.GetStat(i));
                //CheckStatForReplacement (MyItem);
            }
        } 
    }
}*/

/*public void UpdateItemsWithStatData() 
{
    for (int i = 0; i < MyItems.Count; i++) 
    {
        for (int k = 0; k < MyStats.GetSize (); k++)
        {
                MyItems[i].MyStats.ReplaceStatData(MyStats.GetStat(k));
        }
    }
}*/

/*private void GatherStatsFromScene() 
{
    GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
    foreach (GameObject MyObject in AllObjects) {
        if (MyObject.GetComponent<ItemObject> ()) {
            CheckForStatsAdd (MyObject.GetComponent<ItemObject> ().GetItem());	
        } else if (MyObject.GetComponent<Inventory> ()) {
            Inventory MyInventory = MyObject.GetComponent<Inventory> ();
            for (int j = 0; j < MyInventory.MyItems.Count; j++) {
                CheckForStatsAdd (MyInventory.MyItems [j]);
            }
        }
    }
}

private void CheckForStatsAdd(Item MyItem)
{
    for (int i = 0; i < MyItem.MyStats.GetSize(); i++) 
    {
        if (!MyStats.HasStat(MyItem.MyStats.GetStat(i))) 
        {
            MyStats.Add (MyItem.MyStats.GetStat(i));
        }
    }
}*/

/*public static List<GameObject> GatherAllItemObjects(string ComponentName)
{
    List<GameObject> MyItems = new List<GameObject> ();
    GameObject[] AllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(UnityEngine.GameObject));
    foreach (GameObject MyObject in AllObjects) {
#if UNITY_EDITOR
        if (MyObject.GetComponent(ComponentName) && MyObject.activeSelf)
        {
            //Debug.Log ("Found: " + MyObject.name + " : " + UnityEditor.PrefabUtility.GetPrefabType(MyObject).ToString());
            if (UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.None ||
                UnityEditor.PrefabUtility.GetPrefabType(MyObject) == UnityEditor.PrefabType.DisconnectedPrefabInstance) 
            {
                MyItems.Add (MyObject);
            }
        }
#else
        if (MyObject.GetComponent(ComponentName) && MyObject.activeSelf)
            MyItems.Add (MyObject);
#endif
    }
    return MyItems;
}*/
