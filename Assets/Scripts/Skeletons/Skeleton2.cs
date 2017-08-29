using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using Zeltex.Voxels;
using Zeltex.Characters;
using UniversalCoroutine;

namespace Zeltex.Skeletons
{
    /// <summary>
    /// File IO Part of skeletons
    /// </summary>
    public partial class Skeleton : MonoBehaviour
    {

        #region File

        /// <summary>
        /// Returns the skeletons data
        /// </summary>
        public static List<string> GetSkeletonSection(List<string> MyLines)
        {
            //List<string> MySections = SplitSections(MyFiles);
            List<string> MySkeletonScript = new List<string>();
            int BeginIndex = -1;
            for (int i = 0; i < MyLines.Count; i++)
            {
                if (MyLines[i].Contains("/BeginSkeleton"))
                {
                    BeginIndex = i;
                }
                else if (MyLines[i].Contains("/EndSkeleton"))
                {
                    if (BeginIndex != -1)
                    {
                        MySkeletonScript = MyLines.GetRange(BeginIndex, i - BeginIndex + 1);
                        //Debug.LogError("SkeletonScript: " + MySkeletonScript.Count + ":" + MySkeletonScript[MySkeletonScript.Count-1]);
                        Debug.LogError("Skeleton Section: \n" + FileUtil.ConvertToSingle(MySkeletonScript));
                        return MySkeletonScript;
                    }
                    break;  // end search here
                }
            }
            Debug.LogError("Could not find skeleton section:\n" + FileUtil.ConvertToSingle(MyLines));
            return MySkeletonScript;
        }

        /// <summary>
        /// Returns a list of strings (commands and data) to read in from a file.
        /// </summary>
        public List<string> GetScriptList()
        {
            List<string> Data = new List<string>();
            Skeleton MySkeleton = transform.GetComponent<Skeleton>();
            // for each bone, save its index, and the index of its parent bone
            // save its position, rotation, scale
            Data.Add("/BeginSkeleton " + SkeletonName);
            GetCapsule();
            if (CapsuleCollider)
            {
                Data.Add("/Capsule");
                Data.Add(CapsuleCollider.height + "");
                Data.Add(CapsuleCollider.radius + "");
                Data.Add(CapsuleCollider.center.x + "");
                Data.Add(CapsuleCollider.center.y + "");
                Data.Add(CapsuleCollider.center.z + "");
            }
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            { // - MySkeletonPosition.x
                Bone MyBone = MySkeleton.MyBones[i];
                Transform ThisJoint = MyBone.MyTransform;
                if (ThisJoint)
                {
                    Data.Add("/Bone");
                    Data.Add("" + GetParentIndex(MySkeleton.MyBones[i]));
                    if (MyBone.Name != "")
                    {
                        Data.Add("/Name");
                        Data.Add(MyBone.Name);
                    }
                    if (MyBone.Tag != "")
                    {
                        Data.Add("/Tag");
                        Data.Add(MyBone.Tag);
                    }
                    if (ThisJoint.localPosition != new Vector3(0, 0, 0))
                    {
                        Data.Add("/Position");
                        Data.Add("" + (ThisJoint.localPosition.x));  // position
                        Data.Add("" + (ThisJoint.localPosition.y));  // position
                        Data.Add("" + (ThisJoint.localPosition.z));  // position
                        Data.Add("/EndPosition");
                    }
                    if (ThisJoint.localRotation != Quaternion.identity)
                    {
                        Data.Add("/Rotation");
                        Data.Add("" + ThisJoint.localRotation.x);  // rotation
                        Data.Add("" + ThisJoint.localRotation.y);  // rotation
                        Data.Add("" + ThisJoint.localRotation.z);  // rotation
                        Data.Add("" + ThisJoint.localRotation.w);  // rotation
                        Data.Add("/EndRotation");
                    }
                    if (ThisJoint.localScale != new Vector3(1, 1, 1))
                    {
                        Data.Add("/Scale");
                        Data.Add("" + ThisJoint.localScale.x);  // scale
                        Data.Add("" + ThisJoint.localScale.y);  // scale
                        Data.Add("" + ThisJoint.localScale.z);  // scale
                        Data.Add("/EndScale");
                    }
                    if (MyBone.VoxelMesh)
                    {
                        Data.Add("/VoxelMesh");
                        // World script
                        List<string> MyMeshScript = MyBone.VoxelMesh.GetComponent<World>().GetScript();
                        Data.AddRange(MyMeshScript);
                        // Mesh Name
                        Data.Add("/EndVoxelMesh");
                        if (MyBone.VoxelMesh.localPosition != new Vector3(0, 0, 0))
                        {
                            Data.Add("/MeshPosition");
                            Data.Add("" + (MyBone.VoxelMesh.localPosition.x));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localPosition.y));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localPosition.z));  // position
                            Data.Add("/EndMeshPosition");
                        }
                        if (MyBone.VoxelMesh.localRotation != Quaternion.identity)
                        {
                            Data.Add("/MeshRotation");
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.x));
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.y));
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.z));
                            Data.Add("" + (MyBone.VoxelMesh.localRotation.w));
                            Data.Add("/EndMeshRotation");
                        }
                        if (MyBone.VoxelMesh.localScale != new Vector3(1, 1, 1))
                        {
                            Data.Add("/MeshScale");
                            Data.Add("" + (MyBone.VoxelMesh.localScale.x));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localScale.y));  // position
                            Data.Add("" + (MyBone.VoxelMesh.localScale.z));  // position
                            Data.Add("/EndMeshScale");
                        }
                    }
                    Data.Add("/EndBone");
                }
            }
            // Skeleton Data, save all these things as default pose!
            SkeletonAnimator MySkeletonAnimator = GetComponent<SkeletonAnimator>();
            Data.AddRange(MySkeletonAnimator.GetScript());
            Data.Add("/EndSkeleton");
            return Data;
        }

        /// <summary>
        /// Runs the script
        /// </summary>
        public void RunScript(List<string> Data)
        {
            if (LoadRoutine != null)
            {
                this.UniStopCoroutine(LoadRoutine);
            }
            LoadRoutine = this.UniStartCoroutine(RunScriptRoutine(Data));
        }

        /// <summary>
        /// Loads the skeleton in a routine and gives it a name
        /// </summary>
        public IEnumerator Load(string RaceName, List<string> Data)
        {
            transform.parent.gameObject.GetComponent<Character>().SetRace(RaceName);
            yield return this.UniStartCoroutine(RunScriptRoutine(Data));
        }

        /// <summary>
        /// Is loading skeleton
        /// </summary>
        public bool IsLoadingSkeleton()
        {
            return IsLoading;
        }

        public void ForceStopLoad()
        {
            if (IsLoading)
            {
                IsLoading = false;
                if (LoadRoutine != null)
                {
                    this.UniStopCoroutine(LoadRoutine);
                }
            }
        }

        /// <summary>
        /// Loads the skeleton in a routine
        /// </summary>
        public IEnumerator RunScriptRoutine(List<string> Data)
        {
            if (!IsLoading)
            {
                IsLoading = true;
                float LoadSkeletonBeginTime = Time.realtimeSinceStartup;
                yield return (Clear()); //CharacterManager.Get().StartCoroutine
                MyBones.Clear();
                Bone MyBone = new Bone();
                bool IsReading = false;
                Debug.Log("Loading skeleton : " + transform.name + "\n " + FileUtil.ConvertToSingle(Data));

                #region SkeletonData
                for (int i = 0; i < Data.Count; i++)
                {
                    if (!IsReading)   // nothing
                    {
                        //SkeletonName = name;
                        if (Data[i].Contains("/BeginSkeleton"))
                        {
                            string NewSkeletonName = ScriptUtil.RemoveCommand(Data[i]);
                            if (NewSkeletonName != "/BeginSkeleton" && NewSkeletonName != "")
                            {
                                SkeletonName = NewSkeletonName;
                                IsReading = true;
                            }
                            else
                            {
                                Debug.LogError("Line: " + Data[i] + " contains /BeginSkeleton with no skeleton name");
                            }
                        }
                    }
                    else
                    {
                        if (Data[i] == "/EndSkeleton")
                        {
                            IsReading = false;
                            //Debug.LogError("Ending skeleton on line: " + i);
                            break;
                        }
                        else if (Data[i] == "/Name")
                        {
                            MyBone.Name = Data[i + 1];
                            i += 1;
                        }
                        else if (Data[i] == "/Bone")
                        {
                            int ParentIndex = int.Parse(Data[i + 1]);
                            MyBone = CreateBoneLoading(ParentIndex);
                            Debug.Log("Loading skeleton Bone: " + transform.name + ":" + ParentIndex);
                            //yield return new WaitForSeconds(SkeletonLoadDelay);
                            i += 1;
                        }
                        else if (Data[i] == "/Capsule")
                        {
                            float CapsuleHeight = float.Parse(Data[i + 1]);
                            float CapsuleRadius = float.Parse(Data[i + 2]);
                            Vector3 CapsuleCenter = Vector3.zero;
                            CapsuleCenter.x = float.Parse(Data[i + 3]);
                            CapsuleCenter.y = float.Parse(Data[i + 4]);
                            CapsuleCenter.z = float.Parse(Data[i + 5]);
                            GetCapsule();
                            if (CapsuleCollider)
                            {
                                CapsuleCollider.height = CapsuleHeight;
                                CapsuleCollider.radius = CapsuleRadius;
                                CapsuleCollider.center = CapsuleCenter;
                            }
                            i += 5;
                        }
                        else if (Data[i].Contains("/BeginSkeletonAnimator"))
                        {
                            //Debug.LogError("Loading Skeleton Animator " + i);
                            for (int j = i; j < Data.Count; j++)
                            {
                                if (Data[j].Contains("/EndSkeletonAnimator"))
                                {
                                    //Debug.LogError("End Skeleton Animator " + j);
                                    int Index1 = i + 1;
                                    int Index2 = j - 1;
                                    int ElementCount = (Index2 - Index1) + 1;
                                    List<string> MyScript = Data.GetRange(Index1, ElementCount);
                                    //Debug.LogError("Script:\n" + FileUtil.ConvertToSingle(MyScript));
                                    float TimeBegin = Time.realtimeSinceStartup;
                                    if (MyAnimator != null)
                                    {
                                        MyAnimator.RunScript(MyScript);
                                    }
                                    else
                                    {
                                        Debug.LogError(name + " does not have an animator.");
                                    }
                                    //Debug.Log("Time taken to load animation: " + (Time.realtimeSinceStartup - TimeBegin));
                                    i = j;
                                    //yield return new WaitForSeconds(SkeletonLoadDelay);
                                    break;
                                }
                            }
                        }
                        /*else if (Zeltex.Util.ScriptUtil.RemoveWhiteSpace(Data[i]) == "/VoxelMesh")
                        {
                            Debug.Log("Creating VoxelMesh");
                        }*/
                        else if (MyBone.MyTransform)
                        {
                            if (Data[i] == "/Tag")
                            {
                                MyBone.Tag = Data[i + 1];
                                i += 1;
                            }
                            else if (Data[i] == "/Position")
                            {
                                float PositionX = float.Parse(Data[i + 1]);
                                float PositionY = float.Parse(Data[i + 2]);
                                float PositionZ = float.Parse(Data[i + 3]);
                                MyBone.MyTransform.localPosition = new Vector3(PositionX, PositionY, PositionZ);
                                i += 3;
                                //Debug.LogError("Setting new position for bone " + (MyBones.Count-1) + " - " + MyBones[MyBones.Count - 1].position.ToString());
                            }   
                            else if (Data[i] == "/Rotation")
                            {
                                float RotationX = float.Parse(Data[i + 1]);
                                float RotationY = float.Parse(Data[i + 2]);
                                float RotationZ = float.Parse(Data[i + 3]);
                                float RotationW = float.Parse(Data[i + 4]);
                                MyBone.MyTransform.localRotation = new Quaternion(RotationX, RotationY, RotationZ, RotationW);
                                i += 4;
                            }
                            else if (Data[i] == "/Scale")
                            {
                                try
                                {
                                    MyBone.MyTransform.localScale.Set(
                                        float.Parse(Data[i + 1]),
                                        float.Parse(Data[i + 2]),
                                        float.Parse(Data[i + 3]));
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                                i += 3;
                            }
                            else if (ScriptUtil.RemoveWhiteSpace(Data[i]) == "/VoxelMesh")
                            {
                                Debug.Log("Loading VoxelMesh: " + transform.name + ":" + i);
                                for (int j = i + 1; j < Data.Count; j++)
                                {
                                    if (Data[j] == "/EndVoxelMesh")
                                    {
                                        int Index1 = i + 1;
                                        int Index2 = j - 1;
                                        int ElementCount = (Index2 - Index1) + 1; // from i to j-1
                                        List<string> MyMeshScript = Data.GetRange(Index1, ElementCount);
                                        if (MyBones.Count > 0)
                                        {
                                            Debug.Log("Loading skeleton Mesh: " + transform.name + ":" + MyMeshScript.Count + "\n " + FileUtil.ConvertToSingle(MyMeshScript));
                                            yield return CoroutineManager.StartCoroutine(CreateMeshRoutine(
                                                MyBone, 
                                                MyMeshScript, 
                                                false));    //CharacterManager.Get().StartCoroutine
                                        }
                                        i = j;  // skuo to endVoxelMesh line
                                        break;
                                    }
                                }
                                //yield return new WaitForSeconds(SkeletonLoadDelay);
                            }
                            else if (Data[i] == "/MeshPosition")
                            {
                                float PositionX = float.Parse(Data[i + 1]);
                                float PositionY = float.Parse(Data[i + 2]);
                                float PositionZ = float.Parse(Data[i + 3]);
                                if (MyBone.VoxelMesh != null)
                                {
                                    MyBone.VoxelMesh.transform.localPosition = new Vector3(PositionX, PositionY, PositionZ);
                                }
                                i += 3;
                            }
                            else if (Data[i] == "/MeshRotation")
                            {
                                float RotationX = float.Parse(Data[i + 1]);
                                float RotationY = float.Parse(Data[i + 2]);
                                float RotationZ = float.Parse(Data[i + 3]);
                                float RotationW = float.Parse(Data[i + 4]);
                                if (MyBone.VoxelMesh != null)
                                {
                                    MyBone.VoxelMesh.transform.localRotation =
                                        new Quaternion(RotationX, RotationY, RotationZ, RotationW);
                                }
                                i += 4;
                            }
                            else if (Data[i] == "/MeshScale")
                            {
                                float PositionX = float.Parse(Data[i + 1]);
                                float PositionY = float.Parse(Data[i + 2]);
                                float PositionZ = float.Parse(Data[i + 3]);
                                if (MyBone.VoxelMesh != null)
                                {
                                    MyBone.VoxelMesh.transform.localScale = new Vector3(PositionX, PositionY, PositionZ);
                                }
                                i += 3;
                            }
                            else if (Data[i] == "/EndBone")
                            {
                                MyBone = new Bone();    // empty is
                            }
                        }
                    }
                }
                #endregion
                SetDefaultPose();
                RefreshCapsule();
                if (MyBones.Count == 0)
                {
                    // Create default mesh or something? a transparent cube?
                    CreateBoneLoading(-1); // a default bone
                    // Create a default mesh too - a cube
                }
                SetMeshColliders(IsMeshColliders);
                //MyWorld.SetColliders(IsMeshColliders);
                //MyWorld.SetConvex(IsConvexMeshes);
                SetMeshVisibility(true);
                UpdateBounds();
                if (DefaultBody)
                {
                    DefaultBody.SetActive(MyBones.Count == 0);
                }
                IsLoading = false;
            }
            else
            {
                Debug.LogWarning("Trying to load skeleton while already loading.");
            }
            //Debug.Log("Finished Loading Skeleton: " + (Time.realtimeSinceStartup - LoadSkeletonBeginTime));
        }

        /// <summary>
        /// Creates a new bone for the loading skeleton
        /// </summary>
        private Bone CreateBoneLoading(int ParentIndex)
        {
            GameObject NewBone = new GameObject();
            SetLayerMask(NewBone);
            NewBone.name = "Bone " + MyBones.Count;
            NewBone.tag = "Bone";
            if (ParentIndex >= 0 && ParentIndex < MyBones.Count &&
                MyBones[ParentIndex] != null && MyBones[ParentIndex].MyTransform != null)
            {
                NewBone.transform.SetParent(MyBones[ParentIndex].MyTransform, false);
            }
            else
            {
                NewBone.transform.SetParent(transform, false);
            }
            NewBone.transform.position = NewBone.transform.parent.position;
            NewBone.transform.rotation = NewBone.transform.parent.rotation;
            NewBone.transform.localScale = new Vector3(1, 1, 1);
            // Lists
            Bone MyBone = CreateBoneData(NewBone.transform.parent, NewBone.transform, MyBones.Count - 1);
            MyBone.MyTransform = NewBone.transform;
            MyBone.ParentTransform = NewBone.transform.parent;
            MyBones.Add(MyBone);  //ParentBone
            return MyBone;
        }

        #endregion

        #region Mesh

        /// <summary>
        /// Used by skeleton manager
        /// </summary>
        public void CreateMesh(Bone SelectedBone, string MeshData)
        {
            StartCoroutine(CreateMeshRoutine(SelectedBone, FileUtil.ConvertToList(MeshData)));
        }

        /// <summary>
        /// Create a mesh in the timer
        /// </summary>
        private IEnumerator CreateMeshRoutine(Bone SelectedBone, List<string> MeshData, bool IsMeshVisible = true)
        {
            Debug.LogError(name + " CreateMeshRoutine " + MeshData.Count);
            if (SelectedBone != null)
            {
                if (SelectedBone.VoxelMesh != null)
                {
                    MonoBehaviourExtension.Kill(SelectedBone.VoxelMesh.gameObject);
                    SelectedBone.VoxelMesh = null;
                }
                // TODO - Make this pooled skeleton meshes
                GameObject NewMeshObject = new GameObject();
                SetLayerMask(NewMeshObject);
                NewMeshObject.name = "VoxelMesh [" + SelectedBone.Name + "]";
                NewMeshObject.tag = "BonePart";
                // World Stuff
                World MyWorld = NewMeshObject.GetComponent<World>();
                if (MyWorld == null)
                {
                    MyWorld = NewMeshObject.AddComponent<World>();
                    MyWorld.IsChunksCentred = false;
                }
                WorldUpdater WorldUpdater = WorldUpdater.Get();
                if (WorldUpdater)
                {
                    MyWorld.MyUpdater = WorldUpdater;
                }
                else
                {
                    Debug.LogError("WorldUpdater object not found.");
                }
                VoxelManager VoxelManager = VoxelManager.Get();
                if (VoxelManager)
                {
                    MyWorld.MyDataBase = VoxelManager;
                    MyWorld.MyMaterials = VoxelManager.MyMaterials;
                }
                else
                {
                    Debug.LogError("Voxel Manager is null");
                }

                MyWorld.SetColliders(false);
                MyWorld.SetConvex(IsConvexMeshes);
                MyWorld.SetMeshVisibility(IsMeshVisible);

                MyWorld.VoxelScale = VoxelScale;
                MyWorld.IsCentreWorld = true;
                MyWorld.IsDropParticles = true;

                NewMeshObject.transform.SetParent(SelectedBone.MyTransform);
                NewMeshObject.transform.position = SelectedBone.MyTransform.position;
                NewMeshObject.transform.rotation = SelectedBone.MyTransform.rotation;
                NewMeshObject.transform.localScale.Set(1, 1, 1);
                SelectedBone.VoxelMesh = NewMeshObject.transform;

                Debug.LogError("-----------------------------------");
                Debug.LogError(name + " Loading Mesh Script: " + MeshData.Count);
                //yield return null;
                yield return CoroutineManager.StartCoroutine(MyWorld.RunScriptRoutine(MeshData));
                Debug.LogError("-----------------------------------");
                /*bool IsWorldLoading = true;
                int WaitCount = 0;
                while (IsWorldLoading)
                {
                    WaitCount++;
                    IsWorldLoading = MyWorld.IsWorldLoading();
                    yield return null;
                }*/
                //Debug.LogError("Waited : " + WaitCount + " for mesh to load.");
            }
            else
            {
                Debug.LogError("Skeleton bone is null.");
            }
        }

        /// <summary>
        /// Returns the name of the mesh from the meshes gameobject name
        /// </summary>
        public static string GetMeshName(GameObject MyMeshObject)
        {
            if (MyMeshObject.name.Contains("MeshName"))
            {
                int MeshNameIndex = MyMeshObject.name.IndexOf("MeshName");
                string MyMeshName = MyMeshObject.name.Substring(MeshNameIndex);
                //Debug.LogError("MeshName: " + MyMeshName + " - Length: " + MyMeshName.Length);
                int MeshNameIndex2 = MyMeshName.IndexOf("(") + 1;
                int LengthOfName = MyMeshName.Length - MeshNameIndex2 - 1;
                //Debug.LogError("MeshNameStartIndex: " + MeshNameIndex2 + " - LengthOfName: " + LengthOfName);
                MyMeshName = MyMeshName.Substring(MeshNameIndex2, LengthOfName);
                return MyMeshName;
            }
            return "";
        }

        /// <summary>
        /// Gets a debug string for the voxels
        /// </summary>
        private string GetVoxelDebugInfo(List<string> MyData)
        {
            string MyDebugInfo = "";
            List<int> VoxelCount = new List<int>();
            for (int i = 0; i < 25; i++)
            {
                VoxelCount.Add(0);
            }
            for (int i = 0; i < MyData.Count; i++)
            {
                string[] MyInput = MyData[i].Split(' ');
                if (MyInput.Length == 1)
                {
                    VoxelCount[int.Parse(MyInput[0])] += 1;
                }
                else
                {
                    VoxelCount[22] += 1;
                }
            }
            for (int i = 0; i < VoxelCount.Count; i++)
            {
                MyDebugInfo += "Voxel type [" + i + "] with count of [" + VoxelCount[i] + "]\n";
            }
            return MyDebugInfo;
        }
        #endregion
    }
}