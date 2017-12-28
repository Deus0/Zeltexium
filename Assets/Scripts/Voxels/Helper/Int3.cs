using UnityEngine;
using UnityEngine.Events;
using Zeltex.Voxels;

namespace Zeltex
{
    public static class VectorExtentions
    {
        public static Int3 ToInt3(this Vector3 MyVector3)
        {
            return new Int3(MyVector3);
        }
    }
    /// <summary>
    /// Used for events!
    /// </summary>
    [System.Serializable]
    public class MyChunkEvent : UnityEvent<Int3>
    {

    }
    /// <summary>
    /// Used for Chunk Positions!
    /// </summary>
    [System.Serializable]
    public class Int3
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;

        public Int3()
        {

        }
        public Int3(Int3 OldInt3)
        {
            x = OldInt3.x;
            y = OldInt3.y;
            z = OldInt3.z;
        }
        public Int3(int x_, int y_, int z_)
        {
            x = x_;
            y = y_;
            z = z_;
        }
        public Int3(float x_, float y_, float z_)
        {
            x = Mathf.FloorToInt(x_);
            y = Mathf.FloorToInt(y_);
            z = Mathf.FloorToInt(z_);
        }
        public Int3(Vector3 MyVector)
        {
            this.x = Mathf.RoundToInt(MyVector.x);
            this.y = Mathf.RoundToInt(MyVector.y);
            this.z = Mathf.RoundToInt(MyVector.z);
        }

        public void Set(Int3 NewInt3)
        {
            x = NewInt3.x;
            y = NewInt3.y;
            z = NewInt3.z;
        }

        public void Set(float NewX, float NewY, float NewZ)
        {
            x = Mathf.RoundToInt(NewX);
            y = Mathf.RoundToInt(NewY);
            z = Mathf.RoundToInt(NewZ);
        }

        public void Set(int NewX, int NewY, int NewZ)
        {
            x = NewX; y = NewY; z = NewZ;
        }
        /// <summary>
        /// Overload + operator to add two Box objects.
        /// </summary>
        public void Add(Vector3 InputB)
        {
            x += Mathf.RoundToInt(InputB.x);
            y += Mathf.RoundToInt(InputB.y);
            z += Mathf.RoundToInt(InputB.z);
        }
        /// <summary>
        /// Overload + operator to add two Box objects.
        /// </summary>
        public void Add(Int3 InputB)
        {
            x += InputB.x;
            y += (InputB.y);
            z += (InputB.z);
        }
        /// <summary>
        /// Returns the Int3 as a vector3
        /// </summary>
        public Vector3 GetVector()
        {
            return new Vector3(x, y, z);
        }

        public static Int3 Zero()
        {
            return new Int3(0, 0, 0);
        }

        public override string ToString()
        {
            return "[" + x + ":" + y + ":" + z + "]";
        }

        #region Operators
        /// <summary>
        ///  Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator /(Int3 Input, float Multiplier)
        {
            Int3 Output = new Int3();
            Output.x = Input.x / (int)Multiplier;
            Output.y = Input.y / (int)Multiplier;
            Output.z = Input.z / (int)Multiplier;
            return Output;
        }

        /// <summary>
        ///  Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator *(float Multiplier, Int3 Input)
        {
            Int3 Output = new Int3();
            Output.x = Input.x * (int)Multiplier;
            Output.y = Input.y * (int)Multiplier;
            Output.z = Input.z * (int)Multiplier;
            return Output;
        }

        /// <summary>
        ///  Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator *(Int3 Input, float Multiplier)
        {
            Int3 Output = new Int3();
            Output.x = Input.x * (int)Multiplier;
            Output.y = Input.y * (int)Multiplier;
            Output.z = Input.z * (int)Multiplier;
            return Output;
        }

        /// <summary>
        /// Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator *(Int3 Input, int Multiplier)
        {
            Int3 Output = new Int3();
            Output.x = Input.x * Multiplier;
            Output.y = Input.y * Multiplier;
            Output.z = Input.z * Multiplier;
            return Output;
        }

        /// <summary>
        ///  Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator /(Int3 InputA, int InputB)
        {
            Int3 Addition = new Int3();
            Addition.x = InputA.x / InputB;
            Addition.y = InputA.y / InputB;
            Addition.z = InputA.z / InputB;
            return Addition;
        }

        /// <summary>
        /// Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator +(Int3 InputA, Int3 InputB)
        {
            Int3 Addition = new Int3();
            Addition.x = InputA.x + InputB.x;
            Addition.y = InputA.y + InputB.y;
            Addition.z = InputA.z + InputB.z;
            return Addition;
        }
        /// <summary>
        /// Overload + operator to add two Box objects.
        /// </summary>
        public static Int3 operator +(Int3 InputA, Vector3 InputB)
        {
            Int3 Addition = new Int3();
            Addition.x = InputA.x + Mathf.RoundToInt(InputB.x);
            Addition.y = InputA.y + Mathf.RoundToInt(InputB.y);
            Addition.z = InputA.z + Mathf.RoundToInt(InputB.z);
            return Addition;
        }
        // Overload + operator to add two Box objects.
        /*public static Vector3 operator +(Int3 InputA, Vector3 InputB)
        {
            Vector3 Addition = new Vector3();
            Addition.x = InputA.x + InputB.x;
            Addition.y = InputA.y + InputB.y;
            Addition.z = InputA.z + InputB.z;
            return Addition;
        }*/

        // Overload + operator to add two Box objects.
        public static Int3 operator -(Int3 InputA, Int3 InputB)
        {
            Int3 Addition = new Int3();
            Addition.x = InputA.x - InputB.x;
            Addition.y = InputA.y - InputB.y;
            Addition.z = InputA.z - InputB.z;
            return Addition;
        }

        // Overload + operator to add two Box objects.
        public static bool operator ==(Int3 InputA, Int3 InputB)
        {
            if ((object)InputA == null || (object)InputB == null)
            {
                return false;
            }
            return (InputA.x == InputB.x && InputA.y == InputB.y && InputA.z == InputB.z);
        }

        public static bool operator !=(Int3 InputA, Int3 InputB)
        {
            if ((object)InputA == null || (object)InputB == null)
            {
                return true;
            }
            return (InputA.x != InputB.x || InputA.y != InputB.y || InputA.z != InputB.z);
        }
        #endregion

        #region ListSorting
        public override bool Equals(object MyObject)
        {
            if (MyObject == null)
            {
                return false;
            }
            if (this.GetType() != MyObject.GetType())
            {
                return false;
            }
            Int3 MyInt = (Int3)MyObject;
            return (this.x == MyInt.x && this.y == MyInt.y && this.z == MyInt.z);
        }

        public override int GetHashCode()
        {
            // Which is preferred?

            //return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
            //return base.GetHashCode();
            unchecked
            {
                //return this.FooId.GetHashCode();
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region CloseVoxels
        public static Int3 RightInt3 = Int3.Zero();
        public static Int3 LeftInt3 = Int3.Zero();
        public static Int3 AboveInt3 = Int3.Zero();
        public static Int3 BelowInt3 = Int3.Zero();
        public static Int3 FrontInt3 = Int3.Zero();
        public static Int3 BehindInt3 = Int3.Zero();

        /// <summary>
        /// Gets the position to the right (x + 1)
        /// </summary>
        public Int3 Right()
        {
            RightInt3.Set(x + 1, y, z);
            return RightInt3;
            // new Int3(x + 1, y, z);
        }

        /// <summary>
        /// Gets the position to the left (x - 1)
        /// </summary>
        public Int3 Left()
        {
            LeftInt3.Set(x - 1, y, z);
            return LeftInt3;// new Int3(x + 1, y, z);
            //return new Int3(x - 1, y, z);
        }

        public Int3 Above()
        {
           // return new Int3(x, y + 1, z);
            AboveInt3.Set(x, y + 1, z);
            return AboveInt3;
            //return new Int3(x - 1, y, z);
        }
        public Int3 Below()
        {
            //return new Int3(x, y - 1, z);
            BelowInt3.Set(x, y - 1, z);
            return BelowInt3;
        }

        public Int3 Front()
        {
            FrontInt3.Set(x, y, z + 1);
            return FrontInt3;
            //return new Int3(x, y, z + 1);
        }
        public Int3 Behind()
        {
            BehindInt3.Set(x, y, z - 1);
            return BehindInt3;
            //return new Int3(x, y, z - 1);
        }
        #endregion
        
        #region ChunkEdges

        /// <summary>
        /// Sets x to chunksize - 1
        /// </summary>
        public Int3 RightSide()
        {
            return new Int3(Chunk.ChunkSize - 1, y, z);
        }

        /// <summary>
        /// Sets x to 0
        /// </summary>
        public Int3 LeftSide()
        {
            return new Int3(0, y, z);
        }


        public Int3 BelowSide()
        {
            return new Int3(x, Chunk.ChunkSize - 1, z);
        }


        public Int3 AboveSide()
        {
            return new Int3(x, 0, z);
        }


        public Int3 FrontSide()
        {
            return new Int3(x, y, Chunk.ChunkSize - 1);
        }


        public Int3 BehindSide()
        {
            return new Int3(x, y, 0);
        }
        #endregion

        public Int3 ChunkToBlockPosition()
        {
            return new Int3(x * Chunk.ChunkSize, y * Chunk.ChunkSize, z * Chunk.ChunkSize);
        }

    }
}