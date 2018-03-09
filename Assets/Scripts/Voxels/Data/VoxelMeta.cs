using UnityEngine;
using System.Collections.Generic;
using Zeltex.Characters;
using Zeltex.Combat;
using Zeltex.WorldUtilities;
using Zeltex.Items;
using Zeltex.Generators;
using Newtonsoft.Json;

namespace Zeltex.Voxels 
{
    /// <summary>
    /// The data file for each Voxel Type
    /// </summary>
	[System.Serializable]
    public class VoxelMeta : ElementCore
    {
        #region Variables
        //public string Name = "Empty";      
        //private string Description = "A cubey type object."; // used in gui on inspection
        public string ModelID;			                    // various models, ie cube, slopes, etc
        [SerializeField, JsonIgnore]
        private PolyModel MyModel = null;                          // reference to model
        public int TextureMapID;                            // various texture maps, ie grass, etc
        public int MaterialID;		                        // things like animated water, animated power cores, or normal dirt etc
        public List<string> Commands = new List<string>();
        // example commands: /indestructable /Summoner
        // commands will alter the logic or add a gameobject parented to the chunk - linking the voxel/position on that gameobject
        public Stats MyStats;
        public string DropItem; // which item to drop
        #endregion
        
        #region Spawning
        public VoxelHandle MyVoxelHandle;

        public override void Spawn()
        {
            if (MyVoxelHandle == null)
            {
                GameObject NewVoxelHandle = new GameObject();
                NewVoxelHandle.name = Name;// + "-Handler";
                MyVoxelHandle = NewVoxelHandle.AddComponent<VoxelHandle>();
                MyVoxelHandle.Load(this);
            }
            else
            {
                Debug.LogError("Trying to spawn when handler already exists for: " + Name);
            }
        }

        public override void DeSpawn()
        {
            if (MyVoxelHandle)
            {
                MyVoxelHandle.gameObject.Die();
            }
        }

        public override bool HasSpawned()
        {
            return (MyVoxelHandle != null);
        }
        #endregion

        #region Setters

        public void SetModelID(string NewModelName)
        {
            if (ModelID != NewModelName)
            {
                ModelID = NewModelName;
                OnModified();
            }
        }
        public void SetTextureMap(int NewTextureID)
        {
            if (NewTextureID != TextureMapID)
            {
                TextureMapID = NewTextureID;
                OnModified();
            }
        }
        #endregion

        #region Getters

        /// <summary>
        /// Gets the model for the voxel
        /// </summary>
        public PolyModel GetModel()
        {
			if (VoxelManager.Get() && Name != "Air")
			{
				if (MyModel == null || MyModel.Name == "Empty")
				{
					MyModel = DataManager.Get().GetElement(DataFolderNames.PolyModels, ModelID) as PolyModel;
				}
				if (MyModel == null)
                {
                    ModelID = "Block";
                    MyModel = DataManager.Get().GetElement(DataFolderNames.PolyModels, "Block") as PolyModel;
                }
				return MyModel;
			}
			else
			{
				return null;	// air is always null
			}
        }
        #endregion

        #region Files
        /// <summary>
        /// Loads the voxel meta data
        /// </summary>
        /*public override void RunScript(string Data)
        {
            string[] MyLines = Data.Split('\n');
            if (MyLines.Length < 5)
            {
                return;
            }
            Name = MyLines[0];
            ModelID = (MyLines[1]); //int.Parse
            TextureMapID = int.Parse(MyLines[2]);
            MaterialID = int.Parse(MyLines[3]);
            Description = MyLines[4];
            if (MyLines.Length >= 6 && MyLines[5].Contains("/Commands"))
            {
                for (int i = 6; i < MyLines.Length; i++)
                {
                    if (MyLines[i].Contains("/EndCommands"))
                    {
                        break;
                    }
                    else
                    {
                        Commands.Add(MyLines[i]);
                    }
                }
            }
            GetModel(); // make sure to link up reference model!
        }

        /// <summary>
        /// Gets the voxel meta data in script form
        /// </summary>
        public override string GetScript()
        {
            if (Description == "")
            {
                Description = "A voxel unit.";
            }
            string MyData = Name + "\n";
            MyData += ModelID + "\n";
            MyData += TextureMapID + "\n";
            MyData += MaterialID + "\n";
            MyData += Description + "\n";
            if (Commands.Count != 0)
            {
                MyData += "/Commands" + "\n";
                for (int i = 0; i < Commands.Count; i++)
                {
                    MyData += Commands[i] + "\n";
                }
                MyData += "/EndCommands" + "\n";
            }
            return MyData;
        }*/
        #endregion

        #region GettersAndSetters
        public int GetTextureMapID(int Side) 
		{
			return TextureMapID;
		}

        public bool HasCommand(string MyCommand)
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                if (Commands[i].Contains(MyCommand))
                    return true;
            }
            return false;
        }
        #endregion

        #region BlockPowers
        public void CharacterActivate(
            Character MyCharacter,
            Chunk MyChunk,
            Vector3 BlockPosition,
            Voxel MyVoxel)
        {
            Debug.Log("Block at: " + BlockPosition.ToString() +":" + MyVoxel.GetVoxelType() + " - is being activated.");
            // turn to new object if its a door
            if (HasCommand("/Door"))
            {
                OnActivateDoor(MyChunk, BlockPosition);
            }
            else if (HasCommand("/Spawner"))
            {
                // Add Spawner Child Object to chunk - Voxel_i_j_k ->name it this
                // Add script to this object
                // when block is destroyed, destroy this object
                // give block position to spawner
                // Spawner will spawn a minion every 30 seconds if no minion already spawned
                // if already added to chunk, deactivate it - destroy the minion (desummonanimation)
                OnActivateSpawner(MyChunk, BlockPosition);
            }
            else if (HasCommand("/Switch"))
            {
                // if switch, flood any power near by with the current voxel logic
            }
            else if (HasCommand("/Torch"))
            {
                OnActivateTorch(MyChunk, BlockPosition);
            }
            else if (HasCommand("/Teleporter"))
            {
                // Toggle Teleporter or just activate!
                // find nearest teleporter block!
                OnActivateTeleporter(MyChunk, BlockPosition);
            }
        }

        /// <summary>
        /// Called when Creating a voxel
        /// Inputs in the voxels spawning data:
        ///     - World MyWorld - World of spawning
        ///     - VoxelManager MetaData - The database the VoxelMeta is in
        ///     - Vector3 BlockPosition - The position of the block in the world
        ///     - Voxel MyVoxel - The voxel data - can be converted for various data - ie chestvoxel or colourVoxelColor
        /// </summary>
        public void OnVoxelCreate(
            Chunk MyChunk,
            Vector3 BlockPosition,
            Voxel MyVoxel)
        {
            if (MyChunk.GetWorld().IsGameWorld)
            {
                if (HasCommand("/Spawner"))
                {
                    OnCreateSpawner(MyChunk, BlockPosition);
                }
                else if (HasCommand("/Torch"))
                {
                    OnCreateTorch(MyChunk, BlockPosition);
                }
                else if (HasCommand("/Teleporter"))
                {
                    OnCreateTeleporter(MyChunk, BlockPosition);
                }
                else if (HasCommand("/Door"))
                {
                    //OnCreateDoor(MyChunk, BlockPosition);
                }
            }
        }
        public void OnVoxelDestroy(
            Chunk MyChunk,
            Vector3 BlockPosition,
            Voxel MyVoxel)
        {
            if (MyChunk.GetWorld().IsGameWorld)
            {
                if (HasCommand("/Spawner"))
                {
                    OnDestroySpawner(MyChunk, BlockPosition);
                }
                else if (HasCommand("/Torch"))
                {
                    OnDestroyTorch(MyChunk, BlockPosition);
                }
                else if (HasCommand("/Teleporter"))
                {
                    OnDestroyTeleporter(MyChunk, BlockPosition);
                }
            }
        }
        #endregion

        #region Teleporter
        private void OnCreateTeleporter(Chunk MyChunk, Vector3 BlockPosition)
        {
            //Debug.Log("Creating Teleporter at position " + BlockPosition.ToString());
            // Spawn a child object with prefab of torch!
            GameObject MyPrefab = (GameObject)Resources.Load("Prefabs/WorldVoxel/Teleporter");
            GameObject MyTeleporter = (GameObject)GameObject.Instantiate(
                MyPrefab,
                BlockPosition,
                MyChunk.transform.rotation);
            MyTeleporter.name = GetTeleporterName(BlockPosition);
            MyTeleporter.transform.SetParent(MyChunk.transform);
            Vector3 OtherPosition = MyChunk.GetWorld().FindClosestVoxelPosition(BlockPosition, 14); // finds the closest teleporter block
            if (OtherPosition != BlockPosition)
            {
                MyTeleporter.GetComponent<Teleporter>().LinkTeleporter(MyChunk.transform.Find(GetTeleporterName(OtherPosition)));
            }
            //MyFireParticles.transform.localPosition = BlockPosition + new Vector3(0.5f, 0.7f, 0.5f);
            BlockPosition += new Vector3(0.5f, 0f, 0.5f); // adjust for mid point
            BlockPosition += new Vector3(0, 0.15f, 0); // adjust for teleporter
            MyTeleporter.transform.localPosition = new Vector3(
                BlockPosition.x * MyChunk.GetWorld().VoxelScale.x,
                BlockPosition.y * MyChunk.GetWorld().VoxelScale.y,
                BlockPosition.z * MyChunk.GetWorld().VoxelScale.z);
        }
        private void OnDestroyTeleporter(Chunk MyChunk, Vector3 BlockPosition)
        {
            //Debug.Log("Destroying Torch!");
            // remove child object of chunk!
            Transform MyUniqueVoxel = MyChunk.transform.Find(GetTeleporterName(BlockPosition));
            if (MyUniqueVoxel)
            {
                MyUniqueVoxel.gameObject.Die();
            }
        }
        private void OnActivateTeleporter(Chunk MyChunk, Vector3 BlockPosition)
        {
            //Debug.Log("Destroying Teleporter!");
            // remove child object of chunk!
            Transform MyTeleporter = MyChunk.transform.Find(GetTeleporterName(BlockPosition));
            MyTeleporter.gameObject.SetActive(!MyTeleporter.gameObject.activeSelf);   // toggle the flames!
        }
        string GetTeleporterName(Vector3 BlockPosition)
        {
            BlockPosition.x = Mathf.RoundToInt(BlockPosition.x) % Chunk.ChunkSize;
            BlockPosition.y = Mathf.RoundToInt(BlockPosition.y) % Chunk.ChunkSize;
            BlockPosition.z = Mathf.RoundToInt(BlockPosition.z) % Chunk.ChunkSize;
            string MyName = "Teleporter_" + ((int)BlockPosition.x) + "_" + ((int)BlockPosition.y) + "_" + ((int)BlockPosition.z);
            return MyName;
        }
        #endregion

        #region Torch

        private void OnActivateTorch(Chunk MyChunk, Vector3 BlockPosition)
        {
            // remove child object of chunk!
            Transform MyTorch = MyChunk.transform.Find(GetTorchName(BlockPosition));
            MyTorch.gameObject.SetActive(!MyTorch.gameObject.activeSelf);   // toggle the flames!
        }
        private void OnCreateTorch(Chunk MyChunk, Vector3 BlockPosition)
        {
            //Debug.Log("Creating Torch at position " + BlockPosition.ToString());
            // Spawn a child object with prefab of torch!
            GameObject MyPrefab = (GameObject)Resources.Load("Prefabs/WorldVoxel/Torch");
            GameObject MyTorch = (GameObject)GameObject.Instantiate(
                MyPrefab,
                BlockPosition + new Vector3(0.5f, 0.7f, 0.5f),
                MyChunk.transform.rotation);
            MyTorch.name = GetTorchName(BlockPosition);
            MyTorch.transform.SetParent(MyChunk.transform);
            BlockPosition += new Vector3(0.5f, 0.5f, 0.5f); // adjust for mid point
            BlockPosition += new Vector3(0, 0.2f, 0); // adjust fortorch
            MyTorch.transform.localPosition = new Vector3(
                BlockPosition.x * MyChunk.GetWorld().VoxelScale.x,
                BlockPosition.y * MyChunk.GetWorld().VoxelScale.y,
                BlockPosition.z * MyChunk.GetWorld().VoxelScale.z);
           // MyTorch.transform.localPosition = BlockPosition;
        }
        private void OnDestroyTorch(Chunk MyChunk, Vector3 BlockPosition)
        {
            // remove child object of chunk!
            Transform MyTorch = MyChunk.transform.Find(GetTorchName(BlockPosition));
            if (MyTorch)
            {
                MyTorch.gameObject.Die();
            }
        }
        string GetTorchName(Vector3 BlockPosition)
        {
            BlockPosition.x = Mathf.RoundToInt(BlockPosition.x) % Chunk.ChunkSize;
            BlockPosition.y = Mathf.RoundToInt(BlockPosition.y) % Chunk.ChunkSize;
            BlockPosition.z = Mathf.RoundToInt(BlockPosition.z) % Chunk.ChunkSize;
            string MyName = "Torch_" + ((int)BlockPosition.x) + "_" + ((int)BlockPosition.y) + "_" + ((int)BlockPosition.z);
            return MyName;
        }
        #endregion

        #region Spawners
        private void OnActivateSpawner(Chunk MyChunk, Vector3 BlockPosition)
        {
            Transform MySpawner = MyChunk.transform.Find(GetSpawnerName(BlockPosition));
            MySpawner.gameObject.SetActive(!MySpawner.gameObject.activeSelf);
            Debug.Log("Toggling Spawner! " + MySpawner.gameObject.activeSelf.ToString());
        }
        private void OnCreateSpawner(Chunk MyChunk, Vector3 BlockPosition)
        {
            Debug.Log("Creating Spawner at position " + BlockPosition.ToString());
            // Spawn a child object with prefab of torch!
            GameObject MyPrefab = (GameObject)Resources.Load("Prefabs/WorldVoxel/CharacterSpawner");
            GameObject MySpawner = (GameObject)GameObject.Instantiate(
                MyPrefab,
                BlockPosition + new Vector3(0.5f, 0.5f, 0.5f),
                MyChunk.transform.rotation);
            MySpawner.name = GetSpawnerName(BlockPosition);
            MySpawner.transform.SetParent(MyChunk.transform);
            // "Spawner_" + ((int)BlockPosition.x) + "_" + ((int)BlockPosition.y) + "_" + ((int)BlockPosition.z);
            //MySpawner.transform.localPosition = BlockPosition + new Vector3(0.5f, 1.5f, 0.5f);
            BlockPosition += new Vector3(0.5f, 0.5f, 0.5f); // adjust for mid point
            BlockPosition += new Vector3(0, 1, 0); // block above
            MySpawner.transform.localPosition = new Vector3(
                BlockPosition.x * MyChunk.GetWorld().VoxelScale.x,
                BlockPosition.y * MyChunk.GetWorld().VoxelScale.y,
                BlockPosition.z * MyChunk.GetWorld().VoxelScale.z);
        }
        string GetSpawnerName(Vector3 BlockPosition)
        {
            BlockPosition.x = Mathf.RoundToInt(BlockPosition.x) % Chunk.ChunkSize;
            BlockPosition.y = Mathf.RoundToInt(BlockPosition.y) % Chunk.ChunkSize;
            BlockPosition.z = Mathf.RoundToInt(BlockPosition.z) % Chunk.ChunkSize;
            string MyName = "Spawner_" + ((int)BlockPosition.x) + "_" + ((int)BlockPosition.y) + "_" + ((int)BlockPosition.z);
            return MyName;
        }
        /// <summary>
        /// When destroyed, destroy the child object
        /// </summary>
        private void OnDestroySpawner(Chunk MyChunk, Vector3 BlockPosition)
        {
            Debug.Log("Destroying Spawner!");
            // remove child object of chunk!
            string MyName = "Spawner_" + ((int)BlockPosition.x) + "_" + ((int)BlockPosition.y) + "_" + ((int)BlockPosition.z);
            Transform MySpawner = MyChunk.transform.Find(MyName);
            if (MySpawner)
            {
                MySpawner.gameObject.Die();
            }
        }
        #endregion

        #region Door
        private void OnActivateDoor(Chunk MyChunk, Vector3 BlockPosition2)
        {
            Int3 BlockPosition = new Int3(BlockPosition2);
            List<Door> MyDoors = new List<Door>();
            Door MyDoor = CreateDoorAtLocation(MyChunk.GetWorld(), VoxelManager.Get(), BlockPosition.GetVector());
            if (MyDoor != null)
            {
                MyDoors.Add(MyDoor);
                Voxel VoxelAbove = MyChunk.GetWorld().GetVoxel(BlockPosition.Above());
                VoxelMeta MetaAbove = MyChunk.GetWorld().GetVoxelMeta(VoxelAbove.GetVoxelType());
                if (MetaAbove.HasCommand("/Door"))
                {
                    Door MyDoor2 = CreateDoorAtLocation(
                        MyChunk.GetWorld(),
                        VoxelManager.Get(),
                        BlockPosition.Above().GetVector());
                    MyDoors.Add(MyDoor2);
                }
                Voxel VoxelBelow = MyChunk.GetWorld().GetVoxel(BlockPosition.Below());
                VoxelMeta MetaBelow = MyChunk.GetWorld().GetVoxelMeta(VoxelBelow.GetVoxelType());
                if (MetaBelow.HasCommand("/Door"))
                {
                    Door MyDoor3 = CreateDoorAtLocation(
                        MyChunk.GetWorld(), 
                        VoxelManager.Get(), 
                        BlockPosition.Below().GetVector());
                    MyDoors.Add(MyDoor3);
                }
                for (int i = 0; i < MyDoors.Count; i++)
                {
                    MyDoors[i].LinkedDoors = MyDoors;
                    MyDoors[i].ToggleDoor();
                }
            }
        }
        // i kind of need to combine to meshes in the chunk to get the door
        /// <summary>
        /// Door Function
        /// Removes voxels
        /// </summary>
        public Door CreateDoorAtLocation(World MyWorld, VoxelManager MyDataBase, Vector3 Position)
        {
            int VoxelType = MyWorld.GetVoxelType(new Int3(Position));
            MyWorld.UpdateBlockType("Air", Position, 0);
            Item NewItem = ItemGenerator.Get().GenerateItem(VoxelType);   //MyDataBase.GenerateItem(TypeRemoved);
            //GameObject NewMoveableVoxel = new GameObject();
            GameObject NewMoveableVoxel = Zeltex.Items.ItemManager.Get().SpawnItem(MyWorld.transform, NewItem);
            //NewMoveableVoxel.name = MyDataBase.GetMeta(VoxelType).Name;
            // Transform
            Vector3 BlockPosition = Position + new Vector3(0.5f, 0.5f, 0.5f);   // since world mesh is centred!
            NewMoveableVoxel.transform.localPosition = new Vector3(
                BlockPosition.x * MyWorld.VoxelScale.x,
                BlockPosition.y * MyWorld.VoxelScale.y,
                BlockPosition.z * MyWorld.VoxelScale.z);// MyWorld.transform.TransformPoint(Position + new Vector3(0.5f, 0.5f, 0.5f));
            NewMoveableVoxel.transform.rotation = MyWorld.transform.rotation;
            NewMoveableVoxel.transform.localScale = new Vector3(
                MyWorld.transform.lossyScale.x * MyWorld.VoxelScale.x,
                MyWorld.transform.lossyScale.y * MyWorld.VoxelScale.y,
                MyWorld.transform.lossyScale.z * MyWorld.VoxelScale.z);
            // Components
            if (NewMoveableVoxel.GetComponent<MeshFilter>() == null)
                NewMoveableVoxel.AddComponent<MeshFilter>();
            // Destroy any previous colliders
            if (NewMoveableVoxel.GetComponent<SphereCollider>())
            {
                NewMoveableVoxel.GetComponent<SphereCollider>().Die();
            }
            MeshCollider MyCollider = NewMoveableVoxel.GetComponent<MeshCollider>();
            if (MyCollider == null)
            {
                NewMoveableVoxel.AddComponent<MeshCollider>();
            }
            MyCollider.convex = true;
            if (NewMoveableVoxel.GetComponent<Rigidbody>() != null)
            {
                NewMoveableVoxel.GetComponent<Rigidbody>().Die();
            }
            MeshRenderer MyMeshRenderer = NewMoveableVoxel.GetComponent<MeshRenderer>();
            if (MyMeshRenderer == null)
            {
                MyMeshRenderer = NewMoveableVoxel.AddComponent<MeshRenderer>();
            }
            MyMeshRenderer.sharedMaterial = VoxelManager.Get().MyMaterials[0];
            //MyDataBase.UpdateWithSingleVoxelMesh(NewMoveableVoxel, VoxelType, Color.white);
            if (NewMoveableVoxel.GetComponent<ItemHandler>())
            {
                NewMoveableVoxel.GetComponent<ItemHandler>().Die();
            }
            if (NewMoveableVoxel.GetComponent<ParticleSystem>())
            {
                NewMoveableVoxel.GetComponent<ParticleSystem>().Die();
            }
            Door MyDoor = NewMoveableVoxel.AddComponent<Door>();
            return MyDoor;
        }
        #endregion
    }
}