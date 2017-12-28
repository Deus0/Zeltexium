using UnityEngine;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Skeletons;

namespace Zeltex.Generators
{
    /// <summary>
    /// Generates Skeletons
    /// </summary>
    public class SkeletonGenerator : ManagerBase<SkeletonGenerator>
    {
        //private static SkeletonGenerator instance;
        public PolyModelGenerator MyModelGenerator;

        public new static SkeletonGenerator Get()
        {
            if (MyManager == null)
            {
                MyManager = GameObject.FindObjectOfType<SkeletonGenerator>();
            }
            return MyManager;
        }
        /// <summary>
        /// Generates skeletons
        /// </summary>
        public void Generate()
        {
            // now a skeleton
            List<string> SkeletonData = new List<string>();
            string SkeletonName = Zeltex.NameGenerator.GenerateVoxelName();
            GetBasicSkeleton(SkeletonData, SkeletonName);
           // DataManager.Get().Add("Skeletons", SkeletonName, FileUtil.ConvertToSingle(SkeletonData));
        }

        public string GenerateBasicSkeleton(string SkeletonName)
        {
            List<string> SkeletonData = new List<string>();
            GetBasicSkeleton(SkeletonData, SkeletonName);
            return FileUtil.ConvertToSingle(SkeletonData);
        }

        /// <summary>
        /// Generates a basic skeleton
        /// </summary>
        public void GetBasicSkeleton(List<string> MyData, string SkeletonName)
        {
            //float Radius = 0.1f;
            //List<string> MyData = new List<string>();
            MyData.Add("/BeginSkeleton " + SkeletonName);
            int ParentIndex = -1;
            float TotalHeight = 1;
            int BoneParts = Random.Range(3, 3);
            float BoneSize = TotalHeight / ((float)BoneParts);
            for (int i = 0; i < BoneParts; i++)
            {
                float MySize = BoneSize;// Random.Range(0.7f, 1f) - i * 0.2f;
                float PositionY = -BoneSize;
                if (i == 0)
                {
                    //MySize = 0.6f;
                    PositionY = TotalHeight / 2f;   // place all bones in middle
                }
                //MySize = BoneSize;
                List<string> MyBoneData = new List<string>();
                AddBone(ParentIndex, new Vector3(0, PositionY, 0), MySize, MyBoneData);
                MyData.AddRange(MyBoneData);
                ParentIndex++;
            }
            MyData.Add("/EndSkeleton");
            //return MyData;
        }

        /// <summary>
        /// Create a bone!
        /// </summary>
        private void AddBone(int ParentIndex, Vector3 MyPosition, float MySize, List<string> MyData)
        {
            //MyData = new List<string>();
            MyData.Add("/Bone");
            MyData.Add(ParentIndex + "");
            if (MyPosition != Vector3.zero)
            {
                MyData.Add("/Position");
                MyData.Add("" + (MyPosition.x));  // position
                MyData.Add("" + (MyPosition.y));  // position
                MyData.Add("" + (MyPosition.z));  // position
                MyData.Add("/EndPosition");
            }
            MyData.Add("/VoxelMesh");
            //int MeshIndex = Random.Range(0, MyPolyModelMaker.GetSize());
            MyData.AddRange(FileUtil.ConvertToList(MyModelGenerator.GetSphere()));// GetData(MeshIndex)));
            //MyData.Add("0");    // returns body mesh
            MyData.Add("/EndVoxelMesh");
            MyData.Add("/MeshScale");
            MyData.Add("" + MySize);
            MyData.Add("" + MySize);
            MyData.Add("" + MySize);
            MyData.Add("/EndMeshScale");
            MyData.Add("/EndBone");
        }
    }
}