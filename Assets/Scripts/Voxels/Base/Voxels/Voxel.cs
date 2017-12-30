using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.Voxels 
{
    /// <summary>
    /// Each voxel has 6 sides.
    /// </summary>
	[System.Serializable]
	public enum Direction
    {
		Up, 
		Down,
        Forward,
        Back,
        Right,
        Left,
    };

    /// <summary>
    /// The main unit of our game.
    /// </summary>
    [System.Serializable]
	public class Voxel
    {
        public static int DefaultBrightness = 255;
        [SerializeField]
        protected int Type = 0;				    // type of Voxel - -1 is default
		//[SerializeField] public byte Light = (byte)(255);				// 0 to 255 - general brightness
		[SerializeField]
        protected bool HasUpdated;
		[SerializeField]
        public MeshData MyMeshData;

        #region Initiate

        public Voxel()
        {
            Type = 0;
            HasUpdated = true;
            //Light = (byte)(Random.Range (0, 255));
            MyMeshData = new MeshData();
        }

        public Voxel(int NewType)
        {
            Type = NewType;
            HasUpdated = true;
            MyMeshData = new MeshData();
        }

        public Voxel(VoxelColor MyVoxelColor)
        {
            Type = MyVoxelColor.GetVoxelType();
            //Light = MyVoxelColor.Light;
            HasUpdated = MyVoxelColor.GetHasUpdated();
            MyMeshData = MyVoxelColor.MyMeshData;
        }

        /// <summary>
        /// Create a new voxel based ona previous one
        /// </summary>
        public Voxel(Voxel MyVoxel)
        {
            if (MyVoxel != null)
            {
                Type = MyVoxel.GetVoxelType();
                //Light = MyVoxel.Light;
                MyMeshData = MyVoxel.MyMeshData;
            }
            else
            {
                Type = 0;
                MyMeshData = new MeshData();
            }
            HasUpdated = true;
        }

        /// <summary>
        /// Get the color of a voxel
        /// </summary>
        public virtual Color GetColor()
        {
            return Color.white;
        }
        #endregion

        #region Updates

        /// <summary>
        /// Gets the Type - metaIndex - of the voxel
        /// </summary>
        public int GetVoxelType()
        {
            return Type;
        }

        /// <summary>
        /// Set the type of a voxel
        /// </summary>
        public bool SetType(int NewType)
        {
            if (Type != NewType)
            {
                Type = NewType;
                if (Type == 0)
                {
                    MyMeshData.Clear();
                }
                OnUpdated();
                //Debug.LogError("Set new type as: " + NewType);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Used if something exceeds database
        /// </summary>
        public void SetTypeRaw(int NewType)
        {
            Type = NewType;
            if (Type == 0)
            {
                MyMeshData.Clear();
            }
            //Debug.LogError("Set type raw: " + NewType);
        }

        /// <summary>
        /// Has the voxel updated
        /// </summary>
        public bool GetHasUpdated()
        {
            return HasUpdated;
        }

        public void OnUpdated()
        {
            HasUpdated = true;
        }

        /// <summary>
        /// Called when the voxels mesh data has been re built
        /// </summary>
        public void OnBuiltMesh()
        {
            HasUpdated = false;
        }

        /// <summary>
        /// Set if the voxels been updated
        /// </summary>
        /*public void SetHasUpdated(bool NewUpdated)
        {
            HasUpdated = NewUpdated;
        }*/
        #endregion
    }
}
/*#region Lighting
public void SetBrightnessUnChanged(Chunk MyChunk, int x, int y, int z, int Brightness)
{
    byte NewLight = (byte)(Brightness);
    Light = NewLight;
    UpdateSurroundings(MyChunk, x, y, z);
}
/// <summary>
/// Sets a new brightness of the voxel
/// </summary>
public bool SetBrightness(Chunk MyChunk, int x, int y, int z, int Brightness)
{
    byte NewLight = (byte)(Brightness);
    if (Light != NewLight)
    {
        Light = NewLight;
        HasUpdated = true;
        UpdateSurroundings(MyChunk, x, y, z);
        return true;
    }
    return false;
}

/// <summary>
/// Gets the light value of the voxel. Between 0 and 255.
/// </summary>
public byte GetLight()
{
    return Light;
}
/// <summary>
/// Gets the surrounding voxel lights. Overall returning 9+9+8=26(+the includedlight)=27 lights
/// </summary>
/// <param name="MyChunk"></param>
/// <param name="x"></param>
/// <param name="y"></param>
/// <param name="z"></param>
/// <param name="MaterialType"></param>
/// <returns></returns>
public int[] GetSurroundingLights(Chunk MyChunk, int x, int y, int z, int MaterialType)
{
    List<int> MyLights = new List<int>();
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            for (int k = -1; k <= 1; k++)
            {
                Voxel MyVoxel = MyChunk.GetVoxel(x + i, y + j, z + k);
                if (MyVoxel == null)
                {
                    MyLights.Add(DefaultBrightness);
                }
                else
                {
                    MyLights.Add(MyVoxel.GetLight());
                }
            }
        }
    }
    return MyLights.ToArray();
}
/// <summary>
/// Returns a basic set of light values around a voxel
/// </summary>
public int[] GetBasicLights(Chunk MyChunk, int x, int y, int z, int MaterialType) 
{
    int LightAbove = DefaultBrightness;
    Voxel VoxelAbove = MyChunk.GetVoxel(x, y + 1, z);
    if (VoxelAbove != null)
        LightAbove = VoxelAbove.GetLight ();

    int LightBelow = 255;
    Voxel VoxelBelow = MyChunk.GetVoxel(x, y - 1, z);
    if (VoxelBelow != null)
        LightBelow = VoxelBelow.GetLight ();

    int LightFront = DefaultBrightness;
    Voxel VoxelFront = MyChunk.GetVoxel(x, y, z + 1);
    if (VoxelFront != null)
        LightFront = VoxelFront.GetLight ();

    int LightBehind = DefaultBrightness;
    Voxel VoxelBehind = MyChunk.GetVoxel(x, y, z - 1);
    if (VoxelBehind != null)
        LightBehind = VoxelBehind.GetLight ();

    int LightRight = DefaultBrightness;
    Voxel VoxelRight = MyChunk.GetVoxel(x + 1, y, z);
    if (VoxelRight != null)
        LightRight = VoxelRight.GetLight();

    int LightLeft = DefaultBrightness;
    Voxel VoxelLeft = MyChunk.GetVoxel(x - 1, y, z);
    if (VoxelLeft != null)
        LightLeft = VoxelLeft.GetLight();

    int[] MyLights = new int[]
    {
        LightAbove, 
        LightBelow, 
        LightFront, 
        LightBehind, 
        LightRight, 
        LightLeft
    };
    return MyLights;
}
#endregion*/

/* protected void UpdateSurroundings(Chunk MyChunk, int x, int y, int z)
 {
     SetHasUpdated(MyChunk, x + 1, y, z);
     SetHasUpdated(MyChunk, x - 1, y, z);
     SetHasUpdated(MyChunk, x, y + 1, z);
     SetHasUpdated(MyChunk, x, y - 1, z);
     SetHasUpdated(MyChunk, x, y, z + 1);
     SetHasUpdated(MyChunk, x, y, z - 1);
 }*/

/*private void SetHasUpdated(Chunk MyChunk, int x, int y, int z)
{
    Voxel ThisVoxel = MyChunk.GetVoxel(x, y, z);
    if (ThisVoxel != null)
    {
        ThisVoxel.HasUpdated = true;
        if (MyChunk.GetWorld().IsDebug)
        {
            //Debug.Log("Chunk")
            Debug.Log("[" + x + ":" + y + ":" + z + "] Is Updating: " + MyChunk.name);
        }
    }
    else
    {
        if (MyChunk.GetWorld().IsDebug)
        {
            //Debug.Log("Chunk")
            Debug.Log("[" + x + ":" + y + ":" + z + "] Is not Updating: " + MyChunk.name);
        }
    }
}*/
