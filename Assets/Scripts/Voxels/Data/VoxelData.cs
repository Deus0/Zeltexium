using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Zeltex.Voxels 
{
	// various data constructs for worlds
	// World -> Chunk -> Voxel
	// Chunks are stored in a dictionary for quicker access
	// Voxels have meta data which helps in creating the chunk meshes
	// chunks are all parented to a world which helps position it together during rotations/movements

	[System.Serializable]
	public class VoxelDataK
    {
		[SerializeField]
        public Voxel Data;// = new Voxel();
	}

	[System.Serializable]
	public class VoxelDataJ
    {
		[SerializeField]
        public VoxelDataK[] Data;	// k

		public VoxelDataJ()
        {
            /*Data = new VoxelDataK[Chunk.ChunkSize]; // k
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
				Data[i] = new VoxelDataK();
			}
        }
        public VoxelDataJ(int SizeZ)
        {
            Data = new VoxelDataK[SizeZ]; // k
            for (int i = 0; i < SizeZ; i++)
            {
                Data[i] = new VoxelDataK();
            }*/
        }
    }

	[System.Serializable]
	public class VoxelDataI
    {
		[SerializeField]
        public VoxelDataJ[] Data;	// j

		public VoxelDataI()
        {
            /*Data = new VoxelDataJ[Chunk.ChunkSize];
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
				Data[i] = new VoxelDataJ();
			}*/
        }
        /*public VoxelDataI(int SizeY, int SizeZ)
        {
        }*/
    }

    /// <summary>
    /// Every chunk contains one of these. Holds a grid of voxels.
    /// </summary>
	[System.Serializable]
	public class VoxelData
    {
        #region Variables
        [HideInInspector]
        public VoxelDataI[] Data;
        private int SizeX;
        private int SizeY;
        private int SizeZ;
        private Int3 Size = Int3.Zero();

        private int VoxelIndexX = 0;
        private int VoxelIndexY = 0;
        private int VoxelIndexZ = 0;

        public Int3 GetSize()
        {
            return Size;
        }
        // Used for functions
        // Anything thats used for all voxels, make sure its instanced, so it isn't respawned per function call
        private VoxelColor MyVoxelColor;
        private bool HasColorChanged;
        private VoxelMeta PreviousMeta;
        private VoxelMeta ThisMeta;
        private int PreviousType;
        private bool HasTypeChanged;

        private bool hasCheckedSides;
        #endregion

        #region Init

        public VoxelData(int SizeX_, int SizeY_, int SizeZ_)
        {
            SizeX = SizeX_;
            SizeY = SizeY_;
            SizeZ = SizeZ_;
            Data = new VoxelDataI[SizeX]; // i
            Size.Set(SizeX, SizeY, SizeZ);
            Int3 CreatePosition = Int3.Zero();
            for (CreatePosition.x = 0; CreatePosition.x < SizeX; CreatePosition.x++)
            {
                Data[CreatePosition.x] = new VoxelDataI(); //SizeY, SizeZ
                // Initialize data for Y
                Data[CreatePosition.x].Data = new VoxelDataJ[SizeY];
                for (CreatePosition.y = 0; CreatePosition.y < SizeY; CreatePosition.y++)
                {
                    Data[CreatePosition.x].Data[CreatePosition.y] = new VoxelDataJ();
                    Data[CreatePosition.x].Data[CreatePosition.y].Data = new VoxelDataK[SizeZ];
                    for (CreatePosition.z = 0; CreatePosition.z < SizeZ; CreatePosition.z++)
                    {
                        Data[CreatePosition.x].Data[CreatePosition.y].Data[CreatePosition.z] = new VoxelDataK();
                        Data[CreatePosition.x].Data[CreatePosition.y].Data[CreatePosition.z].Data = null;//new Voxel();
                    }
                }
            }
        }
        #endregion

        #region Utility

        public bool IsInRangeX(int x)
        {
            return (x >= 0 && x < SizeX);
        }

        public bool IsInRangeY(int y)
        {
            return (y >= 0 && y < SizeY);
        }

        public bool IsInRangeZ(int z)
        {
            return (z >= 0 && z < SizeZ);
        }

        public bool IsInRange(Int3 Position)
        {
            return (Position.x >= 0 && Position.x < SizeX
                && Position.y >= 0 && Position.y < SizeY
                && Position.z >= 0 && Position.z < SizeZ);
        }

        public void Reset(int i, int j, int k)
        {
            if (IsInRangeX(i) && IsInRangeY(j) && IsInRangeZ(k))
            {
                Data[i].Data[j].Data[k].Data = new Voxel();
            }
        }

        /// <summary>
        /// Converts to a normal voxel from a mutated one
        /// </summary>
        public void ConvertToNormal(Int3 Position)
        {
            Voxel MyVoxel = GetVoxel(Position);
            if (MyVoxel != null)
            {
                VoxelColor MyVoxelTinted = MyVoxel as VoxelColor;
                if (MyVoxelTinted != null)
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new Voxel(MyVoxelTinted);
                }
            }
        }
        #endregion

        #region Setters
        /// <summary>
        /// Set the voxel raw
        /// </summary>
        public void SetVoxelRaw(Int3 Position, Voxel MyVoxel)
        {
            if (IsInRangeX(Position.x) && IsInRangeY(Position.y) && IsInRangeZ(Position.z))
            {
                Data[Position.x].Data[Position.y].Data[Position.z].Data = MyVoxel;
            }
        }

        public void SetVoxelTypeRaw(Int3 Position, int NewIndex)
        {
            if (NewIndex != 0)
            {
                if (Data[Position.x].Data[Position.y].Data[Position.z].Data == null)
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new Voxel();
                }
                Data[Position.x].Data[Position.y].Data[Position.z].Data.SetTypeRaw(NewIndex);
            }
            else
            {
                Data[Position.x].Data[Position.y].Data[Position.z].Data = null;
            }
        }

        public void SetVoxelTypeColorRaw(Int3 Position, int NewIndex, Color VoxelColor)
        {
            Data[Position.x].Data[Position.y].Data[Position.z].Data.SetTypeRaw(NewIndex);
            if (VoxelColor != Color.white)
            {
                SetVoxelAsColor(Position);
                MyVoxelColor = Data[Position.x].Data[Position.y].Data[Position.z].Data as VoxelColor;
                MyVoxelColor.SetColor(VoxelColor);
            }
            else
            {
                SetVoxelAsNormal(Position);
            }
        }

        private void SetVoxelAsColor(Int3 Position)
        {
            if (IsInRange(Position))
            {
                MyVoxelColor = Data[Position.x].Data[Position.y].Data[Position.z].Data as VoxelColor;
                if (MyVoxelColor == null)
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new VoxelColor(Data[Position.x].Data[Position.y].Data[Position.z].Data);
                }
            }
        }
        private void SetVoxelAsNormal(Int3 Position)
        {
            if (IsInRange(Position))
            {
                MyVoxelColor = Data[Position.x].Data[Position.y].Data[Position.z].Data as VoxelColor;
                if (MyVoxelColor != null)
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new Voxel(MyVoxelColor);
                }
            }
        }

        /// <summary>
        /// If Setting a voxel to a tinted color voxel
        /// </summary>
        public bool SetVoxelColor(Chunk MyChunk, Int3 Position, Color32 NewColor) 
        {
            if (IsInRange(Position))
            {
                if (Data[Position.x].Data[Position.y].Data[Position.z].Data == null
                    || Data[Position.x].Data[Position.y].Data[Position.z].Data.GetType() != typeof(VoxelColor))
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new VoxelColor(1, NewColor);
                    HasColorChanged = true;
                }
                else
                {
                    HasColorChanged = false;
                }
                return HasColorChanged;
            }
            return false;
        }

        /// <summary>
        /// The main function used to replace voxel data
        /// </summary>
        public bool SetVoxelType(Chunk MyChunk, Int3 Position, int Type) 
		{
            if (IsInRange(Position))
            {
                if (Data[Position.x].Data[Position.y].Data[Position.z].Data == null)
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new Voxel();
                    HasColorChanged = true;
                }
                else if (Data[Position.x].Data[Position.y].Data[Position.z].Data.GetType() == typeof(VoxelColor))
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new Voxel(Data[Position.x].Data[Position.y].Data[Position.z].Data);
                    HasColorChanged = true;
                }
                else
                {
                    HasColorChanged = false;
                }
                PreviousType = Data[Position.x].Data[Position.y].Data[Position.z].Data.GetVoxelType();
                HasTypeChanged = Data[Position.x].Data[Position.y].Data[Position.z].Data.SetType(Type);
                if (HasTypeChanged)
                {
                    PreviousMeta = DataManager.Get().GetElement(DataFolderNames.Voxels, PreviousType) as VoxelMeta;
                    PreviousMeta.OnVoxelDestroy(MyChunk, Position.GetVector(), Data[Position.x].Data[Position.y].Data[Position.z].Data);
                    ThisMeta = DataManager.Get().GetElement(DataFolderNames.Voxels, Type) as VoxelMeta;
                    ThisMeta.OnVoxelCreate(MyChunk, Position.GetVector(), Data[Position.x].Data[Position.y].Data[Position.z].Data);
                }
                return (HasColorChanged || HasTypeChanged);
            }
            return false;
        }
        #endregion

        #region Getters
        /// <summary>
        /// used in colour picking
        /// </summary>
        public Color GetVoxelColorColor(Int3 Position)
        {
            if (IsInRange(Position))
            {
                Voxel MyVoxel = Data[Position.x].Data[Position.y].Data[Position.z].Data;
                VoxelColor MyVoxelColor = MyVoxel as VoxelColor; // switch between tinted and non tinted
                if (MyVoxelColor != null)
                {
                    return MyVoxelColor.GetColor();
                }
            }
            return Color.white;
        }

        /// <summary>
        ///  tries to get tinted voxel, if its
        /// </summary>
        public VoxelColor GetVoxelColor(Int3 Position)
        {
            if (IsInRange(Position))
            {
                Voxel MyVoxel = Data[Position.x].Data[Position.y].Data[Position.z].Data;
                VoxelColor MyVoxelColor = MyVoxel as VoxelColor;
                if (MyVoxelColor == null)
                {
                    MyVoxelColor = new VoxelColor(MyVoxel);
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = MyVoxelColor;
                }
                return MyVoxelColor;
            }
            else
            {
                return null;
            }
        }


        public Voxel GetVoxelRaw(Int3 VoxelIndex)
        {
            return Data[VoxelIndex.x].Data[VoxelIndex.y].Data[VoxelIndex.z].Data;
        }

        public Voxel GetVoxel(Int3 Position)
        {
            if (IsInRange(Position))
            {
                return Data[Position.x].Data[Position.y].Data[Position.z].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the voxel type from a position
        /// </summary>
        public int GetVoxelType(Int3 Position)
        {
            if (IsInRange(Position))
            {
                try
                {
                    if (Data[Position.x].Data[Position.y].Data[Position.z].Data != null)
                    {
                        return Data[Position.x].Data[Position.y].Data[Position.z].Data.GetVoxelType();
                    }
                }
                catch (System.NullReferenceException)
                {

                }
            }
            return 0;
        }
        #endregion

        #region AllVoxelsOperations

        /// <summary>
        /// 
        /// </summary>
        public void SetHasUpdated(bool NewHasUpdated)
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    for (int k = 0; k < SizeZ; k++)
                    {
                        if (NewHasUpdated)
                        {
                            Data[i].Data[j].Data[k].Data.OnUpdated();
                        }
                        else
                        {
                            Data[i].Data[j].Data[k].Data.OnBuiltMesh();
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            if (hasCheckedSides)
            {
                hasCheckedSides = false;
            }
        }

        public void OnSidesUpdates()
        {
            if (!hasCheckedSides)
            {
                hasCheckedSides = true;
                for (VoxelIndexX = 0; VoxelIndexX < SizeX; VoxelIndexX++)
                {
                    for (VoxelIndexY = 0; VoxelIndexY < SizeY; VoxelIndexY++)
                    {
                        for (VoxelIndexZ = 0; VoxelIndexZ < SizeZ; VoxelIndexZ++)
                        {
                            if (VoxelIndexX == 0 || VoxelIndexX == SizeX - 1 ||
                                VoxelIndexY == 0 || VoxelIndexY == SizeY - 1 ||
                                VoxelIndexZ == 0 || VoxelIndexZ == SizeZ - 1)
                            {
                                if (Data[VoxelIndexX].Data[VoxelIndexY].Data[VoxelIndexZ].Data != null)
                                {
                                    Data[VoxelIndexX].Data[VoxelIndexY].Data[VoxelIndexZ].Data.OnUpdated();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// this takes 30ms, so break it (16/4 = 4) times, into 7.5ms per call
        /// </summary>
        public IEnumerator SetAllVoxelsToAir()
        {
            for (VoxelIndexX = 0; VoxelIndexX < Chunk.ChunkSize; VoxelIndexX++)
            {
                for (VoxelIndexY = 0; VoxelIndexY < Chunk.ChunkSize; VoxelIndexY++)
                {
                    for (VoxelIndexZ = 0; VoxelIndexZ < Chunk.ChunkSize; VoxelIndexZ++)
                    {
                        Data[VoxelIndexX].Data[VoxelIndexY].Data[VoxelIndexZ].Data.SetTypeRaw(0);
                    }
                }
                if ((VoxelIndexX + 1) % 4 == 0)
                {
                    yield return null;
                }
            }
        }
        #endregion
    }
}