using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex
{
    /// <summary>
    /// Generates names using the common rules of the ZelZel peoples
    /// </summary>
    public static class NameGenerator
    {
        /// <summary>
        /// returns a randomly generated name for a voxel
        /// </summary>
        /// <returns></returns>
        public static string GenerateVoxelName()
        {
            string MyVoxelName = "";
            List<string> MySyla = GetSyllabells();
            int SylabelCount = (int)Random.RandomRange(2, 4);
            for (int i = 0; i < SylabelCount; i++)
            {
                MyVoxelName += MySyla[(int)Random.RandomRange(0, MySyla.Count - 1)];
            }
            MyVoxelName = MyVoxelName.Substring(0, 1).ToUpper() + MyVoxelName.Substring(1, MyVoxelName.Length - 1);
            return MyVoxelName;
        }

        public static List<string> GetSyllabells()
        {
            List<string> MyData = new List<string>();
            MyData.Add("mo");
            MyData.Add("monn");
            MyData.Add("fay");
            MyData.Add("shi");
            MyData.Add("zag");
            MyData.Add("zen");
            MyData.Add("tex");
            MyData.Add("zel");
            MyData.Add("pie");

            // 2 letters
            MyData.Add("ze");
            MyData.Add("zi");
            MyData.Add("me");
            MyData.Add("mi");
            MyData.Add("el");
            MyData.Add("te");
            MyData.Add("ex");
            return MyData;
        }
    }
}