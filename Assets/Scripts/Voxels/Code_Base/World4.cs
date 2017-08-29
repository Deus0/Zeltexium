using System.Collections.Generic;
using UnityEngine;
using Zeltex.Items;
using Zeltex.AI;
using Zeltex.Generators;

namespace Zeltex.Voxels
{
    /// <summary>
    /// Utility part of world
    /// </summary>
    public partial class World : MonoBehaviour
    {

        #region DroppingVoxels
        /// <summary>
        /// Creates a voxel byitself in the world.
        /// </summary>
        public void CreateBlockAtLocation(Vector3 Position, int TypeRemoved, Color32 MyTint)
        {
            //Debug.LogError("Creating new particle at: " + Position.ToString() + " of type: " + TypeRemoved);
            if (TypeRemoved == 0)
            {
                //Debug.LogError("Trying to create an air block.");
                return; // don't create air blocks lol
            }
            Item NewItem = ItemGenerator.Get().GenerateItem(TypeRemoved);   //MyDataBase.GenerateItem(TypeRemoved);
            //GameObject NewMoveableVoxel = new GameObject();
            GameObject NewMoveableVoxel = ItemManager.Get().SpawnItem(transform, NewItem);
            if (NewMoveableVoxel == null)
            {
                return;
            }
            NewMoveableVoxel.layer = gameObject.layer;
            NewMoveableVoxel.name = MyDataBase.GetMeta(TypeRemoved).Name;
            // Transform
            NewMoveableVoxel.transform.position = BlockToRealPosition(Position + (new Vector3(1, 1, 1)) / 2f);    // transform.TransformPoint(Position + VoxelScale/2f);
            NewMoveableVoxel.transform.rotation = transform.rotation;
            NewMoveableVoxel.transform.localScale = transform.lossyScale / 2f;
            if (IsDropParticles)
            {
                NewMoveableVoxel.transform.localScale = transform.lossyScale;
            }
            NewMoveableVoxel.transform.localScale = new Vector3(
                VoxelScale.x * NewMoveableVoxel.transform.localScale.x,
                VoxelScale.y * NewMoveableVoxel.transform.localScale.y,
                VoxelScale.z * NewMoveableVoxel.transform.localScale.z);
            // ColourTint
            if (NewMoveableVoxel.GetComponent<MeshFilter>() == null)
            {
                NewMoveableVoxel.AddComponent<MeshFilter>();
            }
            // Components
            // Destroy any previous colliders
            if (NewMoveableVoxel.GetComponent<SphereCollider>())
            {
                Destroy(NewMoveableVoxel.GetComponent<SphereCollider>());
            }
            // Create Components
            MeshCollider MyCollider = NewMoveableVoxel.GetComponent<MeshCollider>();
            if (MyCollider == null)
            {
                NewMoveableVoxel.AddComponent<MeshCollider>();
            }
            if (NewMoveableVoxel.GetComponent<Rigidbody>() == null)
            {
                NewMoveableVoxel.AddComponent<Rigidbody>();
            }
            MeshRenderer MyMeshRenderer = NewMoveableVoxel.GetComponent<MeshRenderer>();
            if (MyMeshRenderer == null)
            {
                MyMeshRenderer = NewMoveableVoxel.AddComponent<MeshRenderer>();
            }
            MyCollider.convex = true;
            MyMeshRenderer.sharedMaterial = MyMaterials[0];
            VoxelModelHandle MyModel = NewMoveableVoxel.AddComponent<VoxelModelHandle>();
            MyModel.UpdateWithSingleVoxelMesh(NewMoveableVoxel, TypeRemoved, MyTint);
            if (IsDropParticles)
            {
                Destroy(NewMoveableVoxel.GetComponent<ParticleSystem>());
                Destroy(NewMoveableVoxel.GetComponent<ItemObject>());
                Destroy(NewMoveableVoxel, UnityEngine.Random.Range(1, 15));
                if (NewMoveableVoxel.GetComponent<Rigidbody>() != null)
                {
                    NewMoveableVoxel.GetComponent<Rigidbody>().isKinematic = false;
                    NewMoveableVoxel.GetComponent<Rigidbody>().useGravity = false;
                    //Destroy(NewMoveableVoxel.GetComponent<Rigidbody>());
                    NewMoveableVoxel.AddComponent<ArtificialGravity>().GravityForce = new Vector3(0, -0.05f, 0);
                }
            }
            else
            {
                if (MyVoxelDestroyParticles)
                {
                    GameObject MyParticles = (GameObject)Instantiate(MyVoxelDestroyParticles, NewMoveableVoxel.transform.position, NewMoveableVoxel.transform.rotation);
                    Destroy(MyParticles, 3f);
                }
            }
        }
        #endregion

        #region Utility

        /// <summary>
        /// Using the index of voxel data, it will use lookup table to get the name, then return the databasse's meta data
        /// </summary>
        public VoxelMeta GetVoxelMeta(int VoxelIndex)
        {
            return MyDataBase.GetMeta(MyLookupTable.GetName(VoxelIndex));
        }

        public bool HasCollision()
        {
            return IsColliders;
        }
        /// <summary>
        /// Set the colliders of the worl
        /// </summary>
        public void SetColliders(bool NewIsColliders)
        {
            if (IsColliders != NewIsColliders)
            {
                IsColliders = NewIsColliders;
                SetCollidersRaw(IsColliders);
            }
        }

        private void SetCollidersRaw(bool NewState)
        {
            if (NewState)
            {
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    if (MyChunk)
                    {
                        MeshCollider MyMeshCollider = MyChunk.gameObject.GetComponent<MeshCollider>();
                        if (MyMeshCollider == null)
                        {
                            MyMeshCollider = MyChunk.gameObject.AddComponent<MeshCollider>();
                            MyMeshCollider.convex = IsConvex;
                        }
                        MyMeshCollider.sharedMesh = MyChunk.gameObject.GetComponent<MeshFilter>().mesh;
                    }
                }
            }
            else
            {
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    if (MyChunk)
                    {
                        MeshCollider MyMeshCollider = MyChunk.gameObject.GetComponent<MeshCollider>();
                        if (MyMeshCollider)
                        {
                            Destroy(MyMeshCollider);
                        }
                    }
                }
            }
        }

        public void SetConvex(bool NewState)
        {
            IsConvex = NewState;
            if (IsColliders)
            {
                foreach (Int3 MyKey in MyChunkData.Keys)
                {
                    Chunk MyChunk = MyChunkData[MyKey];
                    MeshCollider MyMeshCollider = MyChunk.gameObject.GetComponent<MeshCollider>();
                    if (MyMeshCollider)
                    {
                        MyMeshCollider.convex = true;
                    }
                }
            }
        }

        /// <summary>
        /// Flip the world around an axis
        /// </summary>
        public void Flip(string FlipType)
        {
            VoxelData MyVoxelData = new VoxelData(
                    Mathf.RoundToInt(WorldSize.x * Chunk.ChunkSize),
                    Mathf.RoundToInt(WorldSize.y * Chunk.ChunkSize),
                    Mathf.RoundToInt(WorldSize.z * Chunk.ChunkSize));
            Int3 VoxelPosition = Int3.Zero();
            Voxel OldVoxel;
            for (VoxelPosition.x = 0; VoxelPosition.x < MyVoxelData.GetSize().x; VoxelPosition.x++)
            {
                for (VoxelPosition.y = 0; VoxelPosition.y < MyVoxelData.GetSize().y; VoxelPosition.y++)
                {
                    for (VoxelPosition.z = 0; VoxelPosition.z < MyVoxelData.GetSize().z; VoxelPosition.z++)
                    {
                        OldVoxel = GetVoxel(VoxelPosition);
                        VoxelColor OldVoxelTInted = OldVoxel as VoxelColor;
                        if (OldVoxelTInted == null)
                        {
                            if (FlipType == "FlipY")
                            {
                                MyVoxelData.SetVoxelRaw(VoxelPosition.x, Mathf.FloorToInt(MyVoxelData.GetSize().y - 1 - VoxelPosition.y), VoxelPosition.z, new Voxel(OldVoxel));   // -1
                            }
                            else if (FlipType == "FlipX")
                            {
                                MyVoxelData.SetVoxelRaw(Mathf.RoundToInt(MyVoxelData.GetSize().x - 1 - VoxelPosition.x), VoxelPosition.y, VoxelPosition.z, new Voxel(OldVoxel));
                            }
                            else if (FlipType == "FlipZ")
                            {
                                MyVoxelData.SetVoxelRaw(VoxelPosition.x, VoxelPosition.y, Mathf.RoundToInt(MyVoxelData.GetSize().z - 1 - VoxelPosition.z), new Voxel(OldVoxel));
                            }
                        }
                        else
                        {
                            if (FlipType == "FlipY")
                            {
                                MyVoxelData.SetVoxelRaw(VoxelPosition.x, Mathf.FloorToInt(MyVoxelData.GetSize().y - 1 - VoxelPosition.y), VoxelPosition.z, new VoxelColor(OldVoxelTInted));   // -1
                            }
                            else if (FlipType == "FlipX")
                            {
                                MyVoxelData.SetVoxelRaw(Mathf.RoundToInt(MyVoxelData.GetSize().x - 1 - VoxelPosition.x), VoxelPosition.y, VoxelPosition.z, new VoxelColor(OldVoxelTInted));
                            }
                            else if (FlipType == "FlipZ")
                            {
                                MyVoxelData.SetVoxelRaw(VoxelPosition.x, VoxelPosition.y, Mathf.RoundToInt(MyVoxelData.GetSize().z - 1 - VoxelPosition.z), new VoxelColor(OldVoxelTInted));
                            }
                        }
                    }
                }
            }
            string VoxelName = "Air";
            int VoxelType = 0;
            int OldVoxelType = 0;
            Color VoxelColor = Color.white;
            for (VoxelPosition.x = 0; VoxelPosition.x < MyVoxelData.GetSize().x; VoxelPosition.x++)
            {
                for (VoxelPosition.y = 0; VoxelPosition.y < MyVoxelData.GetSize().y; VoxelPosition.y++)
                {
                    for (VoxelPosition.z = 0; VoxelPosition.z < MyVoxelData.GetSize().z; VoxelPosition.z++)
                    {
                        VoxelType = MyVoxelData.GetVoxelType(VoxelPosition);
                        if (VoxelType != OldVoxelType)
                        {
                            VoxelName = MyLookupTable.GetName(VoxelType);
                        }
                        VoxelColor = MyVoxelData.GetVoxelColorColor(VoxelPosition);
                        UpdateBlockTypeMass(VoxelName, VoxelPosition, VoxelColor);
                        GetVoxel(VoxelPosition).OnUpdated();
                        OldVoxelType = VoxelType;
                    }
                }
            }
            OnMassUpdate();
        }

        public bool IsMeshVisible = true;
        public void SetMeshVisibility(bool NewVisibility)
        {
            if (IsMeshVisible != NewVisibility)
            {
                IsMeshVisible = NewVisibility;
                Debug.Log("Setting world " + name + " to mesh visibility of " + NewVisibility);
                foreach (Int3 KeyInKeys in MyChunkData.Keys)
                {
                    MyChunkData[KeyInKeys].SetMeshVisibility(NewVisibility);
                }
            }
        }

        /// <summary>
        /// Perform an action on a group of blocks
        /// </summary>
        public void ApplyAction(string ActionType, List<Int3> BlockPositions)
        {
            ApplyAction(ActionType, BlockPositions, new Color(1, 1, 1));
        }

        /// <summary>
        /// Flip the world around an axis
        /// </summary>
        public void ApplyAction(string ActionType, List<Int3> BlockPositions, Color MyColor)
        {
            VoxelData MyVoxelData = new VoxelData(
                    Mathf.RoundToInt(WorldSize.x * Chunk.ChunkSize),
                    Mathf.RoundToInt(WorldSize.y * Chunk.ChunkSize),
                    Mathf.RoundToInt(WorldSize.z * Chunk.ChunkSize));
            for (int a = 0; a < BlockPositions.Count; a++)
            {
                int i = BlockPositions[a].x;
                int j = BlockPositions[a].y;
                int k = BlockPositions[a].z;
                if (ActionType == "MoveLeft")
                {
                    MyVoxelData.SetVoxelRaw(i + 1, j, k, new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveRight")
                {
                    MyVoxelData.SetVoxelRaw(i - 1, j, k, new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveForward")
                {
                    MyVoxelData.SetVoxelRaw(i, j, k - 1, new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveBack")
                {
                    MyVoxelData.SetVoxelRaw(i, j, k + 1, new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveUp")
                {
                    MyVoxelData.SetVoxelRaw(i, j + 1, k, new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "MoveDown")
                {
                    MyVoxelData.SetVoxelRaw(i, j - 1, k, new Voxel(GetVoxel(BlockPositions[a])));
                }
                else if (ActionType == "Erase")
                {
                    UpdateBlockTypeMass("Air", BlockPositions[a]);
                }
                else if (ActionType == "Color")
                {
                    UpdateBlockTypeMass(GetVoxelName(BlockPositions[a]), BlockPositions[a], MyColor);
                }
                else if (ActionType == "CutToNewModel")
                {
                    Voxel MyVoxel = GetVoxel(BlockPositions[a]);
                    VoxelColor MyVoxelColor = MyVoxel as VoxelColor;
                    if (MyVoxelColor == null)
                    {
                        MyVoxelData.SetVoxelRaw(i, j, k, new Voxel(MyVoxel));
                    }
                    else
                    {
                        MyVoxelData.SetVoxelRaw(i, j, k, new VoxelColor(MyVoxelColor));
                    }
                }
            }
            Int3 MovePosition = Int3.Zero();
            if (ActionType.Contains("Move"))
            {
                // Now wipe previous ones
                for (int a = 0; a < BlockPositions.Count; a++)
                {
                    UpdateBlockTypeMass("Air", new Int3(BlockPositions[a]));
                }
                // Move them over!
                for (MovePosition.x = 0; MovePosition.x < WorldSize.x * Chunk.ChunkSize; MovePosition.x++)
                {
                    for (MovePosition.y = 0; MovePosition.y < WorldSize.y * Chunk.ChunkSize; MovePosition.y++)
                    {
                        for (MovePosition.z = 0; MovePosition.z < WorldSize.z * Chunk.ChunkSize; MovePosition.z++)
                        {
                            int MyType = MyVoxelData.GetVoxelType(MovePosition);
                            if (MyType != 0)
                            {
                                UpdateBlockTypeMass(MyType, MovePosition);
                            }
                        }
                    }
                }
            }
            if (ActionType.Contains("CutToNewModel"))
            {
                // Move them over!
                for (MovePosition.x = 0; MovePosition.x < WorldSize.x * Chunk.ChunkSize; MovePosition.x++)
                {
                    for (MovePosition.y = 0; MovePosition.y < WorldSize.y * Chunk.ChunkSize; MovePosition.y++)
                    {
                        for (MovePosition.z = 0; MovePosition.z < WorldSize.z * Chunk.ChunkSize; MovePosition.z++)
                        {
                            int MyType = MyVoxelData.GetVoxelType(MovePosition);
                            Color VoxelColor = MyVoxelData.GetVoxelColorColor(MovePosition);
                            UpdateBlockTypeMass(MyLookupTable.GetName(MyType), MovePosition, VoxelColor);
                        }
                    }
                }
            }
            OnMassUpdate();
        }

        public void GetNeighborsBySolid(Int3 BlockPosition, List<Int3> MyNeighbors)
        {
            //List<Int3> MyNeighbors = new List<Int3>();
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel != null)
            {
                // if the same type, and doesn't contain this position, flood fill from here
                if (MyVoxel.GetVoxelType() != 0 && MyNeighbors.Contains(BlockPosition) == false)
                {
                    MyNeighbors.Add(BlockPosition);
                    if (MyNeighbors.Contains(BlockPosition.Left()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Left()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Right()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Right()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Above()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Above()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Below()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Below()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Front()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Front()), MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Behind()) == false)
                    {
                        GetNeighborsBySolid(new Int3(BlockPosition.Behind()), MyNeighbors);
                    }
                }
            }
        }

        public void GetNeighborsByColor(Int3 BlockPosition, Color32 VoxelColor, List<Int3> MyNeighbors)
        {
            //List<Int3> MyNeighbors = new List<Int3>();
            Voxel MyVoxel = GetVoxel(BlockPosition);
            if (MyVoxel != null)
            {
                // if the same type, and doesn't contain this position, flood fill from here
                if (MyVoxel.GetColor() == VoxelColor && MyNeighbors.Contains(BlockPosition) == false)
                {
                    MyNeighbors.Add(BlockPosition);
                    if (MyNeighbors.Contains(BlockPosition.Left()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Left()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Right()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Right()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Above()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Above()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Below()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Below()), VoxelColor, MyNeighbors);
                    }

                    if (MyNeighbors.Contains(BlockPosition.Front()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Front()), VoxelColor, MyNeighbors);
                    }
                    if (MyNeighbors.Contains(BlockPosition.Behind()) == false)
                    {
                        GetNeighborsByColor(new Int3(BlockPosition.Behind()), VoxelColor, MyNeighbors);
                    }
                }
            }
        }

        /// <summary>
        /// Flip the world around an axis
        /// </summary>
        public void CropSelected(List<Int3> BlockPositions)
        {
            Int3 MinimumPosition = GetWorldBlockSize().ToInt3();
            Int3 MaximumPosition = Int3.Zero();
            int i, j, k;
            for (int a = 0; a < BlockPositions.Count; a++)
            {
                i = BlockPositions[a].x;
                j = BlockPositions[a].y;
                k = BlockPositions[a].z;
                if (i < MinimumPosition.x)
                {
                    MinimumPosition.x = i;
                }
                else if (i > MaximumPosition.x)
                {
                    MaximumPosition.x = i;
                }
                if (j < MinimumPosition.y)
                {
                    MinimumPosition.y = j;
                }
                else if (j > MaximumPosition.y)
                {
                    MaximumPosition.y = j;
                }
                if (k < MinimumPosition.z)
                {
                    MinimumPosition.z = k;
                }
                else if (k > MaximumPosition.z)
                {
                    MaximumPosition.z = k;
                }
            }
            Debug.LogError("Max is: " + MaximumPosition.GetVector().ToString() + " - Min is: " + MinimumPosition.ToString());
            Int3 TotalSize = MaximumPosition - MinimumPosition;
            Int3 NewWorldSize = new Int3(
                Mathf.CeilToInt(TotalSize.x / (float)Chunk.ChunkSize),
                Mathf.CeilToInt(TotalSize.y / (float)Chunk.ChunkSize),
                Mathf.CeilToInt(TotalSize.z / (float)Chunk.ChunkSize));
            NewWorldSize.x = Mathf.Max(NewWorldSize.x, 1);
            NewWorldSize.y = Mathf.Max(NewWorldSize.y, 1);
            NewWorldSize.z = Mathf.Max(NewWorldSize.z, 1);
            // adjust to chunk size
            TotalSize.x = Chunk.ChunkSize * NewWorldSize.x;
            TotalSize.y = Chunk.ChunkSize * NewWorldSize.y;
            TotalSize.z = Chunk.ChunkSize * NewWorldSize.z;
            Debug.LogError("NewWorldSize is: " + NewWorldSize.ToString() + " - TotalSize is: " + TotalSize.ToString());
            VoxelData MyVoxelData = new VoxelData(TotalSize.x, TotalSize.y, TotalSize.z);
            Voxel MyVoxel;
            VoxelColor MyVoxelColor;
            for (int a = 0; a < BlockPositions.Count; a++)
            {
                MyVoxel = GetVoxel(BlockPositions[a]);
                MyVoxelColor = MyVoxel as VoxelColor;
                i = BlockPositions[a].x - MinimumPosition.x;
                j = BlockPositions[a].y - MinimumPosition.y;
                k = BlockPositions[a].z - MinimumPosition.z;
                if (MyVoxelColor == null)
                {
                    MyVoxelData.SetVoxelRaw(i, j, k, new Voxel(MyVoxel));
                }
                else
                {
                    Debug.LogError("Setting: " + i + ":" + j + ":" + k + " to - : " + MyVoxelColor.GetVoxelType());
                    MyVoxelData.SetVoxelRaw(i, j, k, new VoxelColor(MyVoxelColor));
                }
                UpdateBlockTypeMass("Air", new Int3(i,j,k));
            }
            Color VoxelColor = Color.white;
            int MyType = 0;
            int OldType = 0;
            string VoxelName = "Air";
            Int3 VoxelPosition = Int3.Zero();
            // Move them over!
            for (i = 0; i < TotalSize.x; i++)
            {
                for (j = 0; j < TotalSize.y; j++)
                {
                    for (k = 0; k < TotalSize.z; k++)
                    {
                        VoxelPosition.Set(i, j, k);
                        MyType = MyVoxelData.GetVoxelType(VoxelPosition);
                        if (MyType != OldType)
                        {
                            VoxelName = MyLookupTable.GetName(MyType);
                        }
                        VoxelColor = MyVoxelData.GetVoxelColorColor(VoxelPosition);
                        /*if (MyType != 0)
                        {
                            Debug.LogError("Setting: " + i + ":" + j + ":" + k + " to - : " + VoxelName);
                        }*/
                        UpdateBlockTypeMass(VoxelName, VoxelPosition, VoxelColor);
                        GetVoxel(VoxelPosition).OnUpdated();
                        OldType = MyType;
                    }
                }
            }
            StartCoroutine(SetWorldSizeRoutine(NewWorldSize, 
                (ResizedWorld) => 
                {
                    StartCoroutine(RefreshInTime(ResizedWorld));
                }));
        }

        public void ForceRefresh()
        {
            SetAllVoxelsUpdated();
            OnMassUpdate();
        }

        private System.Collections.IEnumerator RefreshInTime(World ResizedWorld)
        {
            yield return null;
            Debug.LogError("Refreshing world");
            ResizedWorld.SetAllVoxelsUpdated();
            ResizedWorld.OnMassUpdate();
        }
        #endregion

        #region Seek

        /// <summary>
        /// Find the closest position within voxel distance - of the same type as this position
        /// </summary>
        public Vector3 FindClosestVoxelPosition(Vector3 ThisPosition, int MaxVoxelDistance)
        {
            int VoxelIndex = GetVoxelType(new Int3(ThisPosition));
            //Debug.LogError("Looking for: " + ThisPosition.ToString() + ":" + GetVoxelType(ThisPosition) + " with Distance of: " + MaxVoxelDistance);
            return FindClosestVoxelPosition(ThisPosition, VoxelIndex, MaxVoxelDistance, 1);
        }
        /// <summary>
        /// Find the closest position within voxel distance - of the same type as this position
        /// </summary>
        public Vector3 FindClosestVoxelPosition(Vector3 ThisPosition, int VoxelIndex, int MaxVoxelDistance)
        {
            //Debug.LogError("Looking for: " + ThisPosition.ToString() + ":" + GetVoxelType(ThisPosition) + " with Distance of: " + MaxVoxelDistance);
            return FindClosestVoxelPosition(ThisPosition, VoxelIndex, MaxVoxelDistance, 1);
        }
        /// <summary>
        /// Find the closest position within voxel distance - of the same type as this position
        /// This is the recursive part of the function
        /// </summary>
        public Vector3 FindClosestVoxelPosition(Vector3 ThisPosition, int VoxelIndex, int MaxVoxelDistance, int VoxelDistance)
        {
            if (VoxelDistance > MaxVoxelDistance)
            {
                //Debug.LogError("Could not find voxel!");
                return ThisPosition;
            }
            for (int i = -VoxelDistance; i <= VoxelDistance; i++)
            {
                for (int j = -VoxelDistance; j <= VoxelDistance; j++)
                {
                    for (int k = -VoxelDistance; k <= VoxelDistance; k++)
                    {
                        if (i == -VoxelDistance || i == VoxelDistance ||
                            j == -VoxelDistance || j == VoxelDistance ||
                            k == -VoxelDistance || k == VoxelDistance)
                        {
                            Int3 OtherPosition = new Int3(ThisPosition) + new Int3(i, j, k);
                            int OtherType = GetVoxelType(OtherPosition);
                            if (VoxelIndex == OtherType)
                            {
                                //Debug.LogError("FoundVoxel!");
                                return OtherPosition.GetVector();
                            }
                        }
                    }
                }
            }
            VoxelDistance++;
            return FindClosestVoxelPosition(ThisPosition, VoxelIndex, MaxVoxelDistance, VoxelDistance);    // if failed
        }
        #endregion
    }
}

/*private List<string> InstantDebug()
{
    List<string> MyData = new List<string>();
    List<int> MetaCount = new List<int>();
    for (int i = 0; i < MyDataBase.Data.Count; i++)
    {
        MetaCount.Add(0);
    }
    for (int a = 0; a < MyChunkData.Values.Count; a++)
    {
        Chunk MyChunk = MyChunkData.Values[a];
        for (int i = 0; i < Chunk.ChunkSize; i++)
        {
            for (int j = 0; j < Chunk.ChunkSize; j++)
            {
                for (int k = 0; k < Chunk.ChunkSize; k++)
                {
                    MetaCount[MyChunk.GetVoxelType(i, j, k)]++;
                }
            }
        }
    }
    for (int i = 0; i < MyDataBase.Data.Count; i++)
    {
        if (MetaCount[i] > 0)
        {
            MyData.Add(MyDataBase.Data[i].Name + " x" + MetaCount[i]);
        }
    }
    return MyData;
}*/
