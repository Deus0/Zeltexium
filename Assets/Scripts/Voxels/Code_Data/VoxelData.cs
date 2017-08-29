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
        public Voxel Data = new Voxel();
	}

	[System.Serializable]
	public class VoxelDataJ
    {
		[SerializeField]
        public VoxelDataK[] Data;	// k

		public VoxelDataJ()
        {
            Data = new VoxelDataK[Chunk.ChunkSize]; // k
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
            }
        }
    }

	[System.Serializable]
	public class VoxelDataI
    {
		[SerializeField]
        public VoxelDataJ[] Data;	// j

		public VoxelDataI()
        {
            Data = new VoxelDataJ[Chunk.ChunkSize];
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
				Data[i] = new VoxelDataJ();
			}
        }
        public VoxelDataI(int SizeY, int SizeZ)
        {
            Data = new VoxelDataJ[SizeY];
            for (int i = 0; i < SizeY; i++)
            {
                Data[i] = new VoxelDataJ(SizeZ);
            }
        }
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

        public VoxelData()
        {
            SizeX = Chunk.ChunkSize;
            SizeY = Chunk.ChunkSize;
            SizeZ = Chunk.ChunkSize;
            Data = new VoxelDataI[Chunk.ChunkSize]; // i
            Size.Set(SizeX, SizeY, SizeZ);
            for (int i = 0; i < Chunk.ChunkSize; i++)
            {
                Data[i] = new VoxelDataI();
            }
        }

        public VoxelData(int SizeX_, int SizeY_, int SizeZ_)
        {
            SizeX = SizeX_;
            SizeY = SizeY_;
            SizeZ = SizeZ_;
            Data = new VoxelDataI[SizeX]; // i
            Size.Set(SizeX, SizeY, SizeZ);
            for (int i = 0; i < SizeX; i++)
            {
				Data[i] = new VoxelDataI(SizeY, SizeZ);
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
                && Position.z >= 0 && Position.z < SizeZ) ;
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
        public void ConvertToNormal(int i, int j, int k)
        {
            Voxel MyVoxel = GetVoxel(i, j, k);
            if (MyVoxel != null)
            {
                VoxelColor MyVoxelTinted = MyVoxel as VoxelColor;
                if (MyVoxelTinted != null)
                {
                    Data[i].Data[j].Data[k].Data = new Voxel(MyVoxelTinted);
                }
            }
        }
        #endregion

        #region Setters
        public void SetVoxelRaw(int i, int j, int k, Voxel MyVoxel)
        {
            SetVoxelRaw(new Int3(i, j, k), MyVoxel);
        }
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

        /// <summary>
        /// Sets the vvoxel type raw without triggering updates
        /// </summary>
        public void SetVoxelTypeRaw(int i, int j, int k, int NewIndex)
        {
            //if (IsInRangeX(i) && IsInRangeY(j) && IsInRangeZ(k))
           // {
                //if (Data[i].Data[j].Data[k].Data.GetVoxelType() != NewIndex)
                //{
                    Data[i].Data[j].Data[k].Data.SetTypeRaw(NewIndex);
            /*if (NewIndex == 0)
            {
                Data[i].Data[j].Data[k].Data.MyMeshData.Clear();
            }*/
            //}
            // }
        }

        public void SetVoxelTypeRaw(Int3 Position, int NewIndex)
        {
            Data[Position.x].Data[Position.y].Data[Position.z].Data.SetTypeRaw(NewIndex);
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

        public bool SetVoxelType(Chunk MyChunk, int i, int j, int k, int Type, Color MyTint)
        {
            return SetVoxelType(MyChunk, new Int3(i, j, k), Type, MyTint);
        }

        // don't use Voxel for this, as it gets confused
        /// <summary>
        /// The main function used to replace voxel data
        /// </summary>
        public bool SetVoxelType(Chunk MyChunk, Int3 Position, int Type, Color MyTint) 
		{
            if (Type == 0)  // air is always white
            {
                MyTint = Color.white;
            }
            if (IsInRange(Position))
            {
                MyVoxelColor = Data[Position.x].Data[Position.y].Data[Position.z].Data as VoxelColor; // switch between tinted and non tinted
                HasColorChanged = false;
                if (MyVoxelColor == null && MyTint != Color.white) // if needs to be a tinted voxel but is not!
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new VoxelColor(Data[Position.x].Data[Position.y].Data[Position.z].Data);
                    MyVoxelColor = Data[Position.x].Data[Position.y].Data[Position.z].Data as VoxelColor;    // convert to tinted voxel
                }
                else if (MyTint == Color.white && MyVoxelColor != null)    // if tinted voxel but colour is just white switch back!
                {
                    Data[Position.x].Data[Position.y].Data[Position.z].Data = new Voxel(MyVoxelColor);    // convert to normal!
                    Data[Position.x].Data[Position.y].Data[Position.z].Data.OnUpdated();
                    HasColorChanged = true;
                }
                if (MyTint != Color.white && MyTint != MyVoxelColor.GetColor())  // assuming tinted voxel has been site now!
                {
                    MyVoxelColor.SetColor(MyTint);
                    Data[Position.x].Data[Position.y].Data[Position.z].Data.OnUpdated();
                    HasColorChanged = true;
                }
                PreviousType = Data[Position.x].Data[Position.y].Data[Position.z].Data.GetVoxelType();
                HasTypeChanged = Data[Position.x].Data[Position.y].Data[Position.z].Data.SetType(Type);
                if (HasTypeChanged)
                {
                    PreviousMeta = MyChunk.GetWorld().MyDataBase.GetMeta(PreviousType);
                    PreviousMeta.OnVoxelDestroy(MyChunk, Position.GetVector(), Data[Position.x].Data[Position.y].Data[Position.z].Data);
                    ThisMeta = MyChunk.GetWorld().MyDataBase.GetMeta(Type);
                    ThisMeta.OnVoxelCreate(MyChunk, Position.GetVector(), Data[Position.x].Data[Position.y].Data[Position.z].Data);
                }
                return (HasColorChanged || HasTypeChanged);
            }
            else
            {
                return false;
            }
		}
        #endregion

        #region Getters
       /* public Color GetVoxelColor(Int3 Position)
        {
            if (IsInRangeX(Position.x) && IsInRangeY(Position.y) && IsInRangeZ(Position.z))
            {
                Voxel MyVoxel = Data[Position.x].Data[Position.y].Data[Position.z].Data;
                VoxelColor MyVoxelColor = MyVoxel as VoxelColor; // switch between tinted and non tinted
                if (MyVoxelColor != null)
                {
                    return MyVoxelColor.GetColor();
                }
            }
            return Color.white;
        }*/
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

        public Voxel GetVoxel(int i, int j, int k)
        {
            if (IsInRangeX(i) && IsInRangeY(j) && IsInRangeZ(k))
                return Data[i].Data[j].Data[k].Data;
            else
                return null;
        }

        [System.Obsolete]
        public int GetVoxelType(int i, int j, int k)
        {
            if (IsInRangeX(i) && IsInRangeY(j) && IsInRangeZ(k))
            {
                return Data[i].Data[j].Data[k].Data.GetVoxelType();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the voxel type from a position
        /// </summary>
        public int GetVoxelType(Int3 Position)
        {
            if (IsInRange(Position))
            {
                return Data[Position.x].Data[Position.y].Data[Position.z].Data.GetVoxelType();
            }
            else
            {
                return 0;
            }
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
                                Data[VoxelIndexX].Data[VoxelIndexY].Data[VoxelIndexZ].Data.OnUpdated();
                                // Data[VoxelIndexX].Data[VoxelIndexY].Data[VoxelIndexZ].Data.OnBuiltMesh();
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