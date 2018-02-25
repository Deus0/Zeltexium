using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;
using Newtonsoft.Json;

namespace Zeltex.Skeletons
{

    /// <summary>
    /// Each Zanimation contains a bunch of keyframes, connected to transforms
    /// Uses transform names for bone positions
    /// </summary>
    [System.Serializable]
    public class Zanimation : Element
    {
        #region Variables
        // Each animation holds a list of keyframes - Storing a point in our timeline curve
        [JsonProperty]
        public bool IsAnimationLoop = true;
        [JsonProperty, SerializeField]
        protected float TimeLength = 10;
        [JsonProperty]
        public List<ZeltexTransformCurve> AnimatedTransforms = new List<ZeltexTransformCurve>();
        [JsonIgnore]
        private List<Transform> TransformMask = new List<Transform>();

        public void ApplyTransformMask(List<Transform> NewTransformMask)
        {
            if (NewTransformMask == null)
            {
                NewTransformMask = new List<Transform>();
            }
            RemoveTransformMask();
            TransformMask.AddRange(NewTransformMask);
            Debug.Log("Applying mask for: " + AnimatedTransforms.Count + " Animated Bones with: " + TransformMask.Count + " UnMasked Bones.");
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].SetActive(TransformMask.Contains(AnimatedTransforms[i].MyObject));
            }
        }

        public void RemoveTransformMask()
        {
            Restore();
            // First set all as active
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].SetActive(true);
            }
            TransformMask.Clear();
        }

        public float GetTimeLength()
        {
            return TimeLength;
        }

        public void SetTimeLength(float NewTimeLength)
        {
            TimeLength = NewTimeLength;
        }

        public void AddCurve(ZeltexTransformCurve NewCurve)
        {
            NewCurve.ParentElement = this;
            AnimatedTransforms.Add(NewCurve);
            OnModified();
        }

        public override void OnLoad()
        {
            base.OnLoad();
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].ParentElement = this;
                AnimatedTransforms[i].OnLoad();
            }
        }
        #endregion

        public void GenerateNewData(float GenerateNoiseMax = 0.05f, bool IsNewName = false)
        {
            if (IsNewName)
            {
                SetName(NameGenerator.GenerateVoxelName());
            }
            if (AnimatedTransforms.Count > 0)
            {
                for (int i = 0; i < AnimatedTransforms.Count; i++)
                {
                    ZeltexTransformCurve BoneFrames = AnimatedTransforms[i];
                    BoneFrames.GenerateRandomNoise(TimeLength, GenerateNoiseMax);
                }
            }
            else
            {
                for (int i = 0; i < 1; i++)
                {
                    ZeltexTransformCurve BoneFrames = new ZeltexTransformCurve();
                    BoneFrames.GenerateRandomNoise(TimeLength, GenerateNoiseMax);
                    AddKeyFrame(BoneFrames);
                }
            }
            OnModified();
        }

        public int GetSize()
        {
            return AnimatedTransforms.Count;
        }

        public void RemoveTransform(int Index)
        {
            AnimatedTransforms.RemoveAt(Index);
        }

        public ZeltexTransformCurve GetTransform(int Index)
        {
            return AnimatedTransforms[Index];
        }

        public void PlayFrame(float CurrentTime)
        {
            Vector3 CurrentPosition = Vector3.zero;
            Vector3 CurrentRotation = Vector3.zero;
            Vector3 CurrentScale = Vector3.zero;
            ZeltexTransformCurve CurrentAnimatedTransform;
            for (int i = AnimatedTransforms.Count - 1; i >= 0; i--)
            {
                CurrentAnimatedTransform = AnimatedTransforms[i];
                if (CurrentAnimatedTransform == null)
                {
                    AnimatedTransforms.RemoveAt(i);
                }
                else
                {
                    CurrentAnimatedTransform.SetTime(CurrentTime);
                }
            }
        }

        #region Utility

        public void ClearKeys(string PropertyName, Transform TargetTransform)
        {
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                if (AnimatedTransforms[i].MyObject == TargetTransform)
                {
                    AnimatedTransforms[i].ClearKeys(PropertyName);
                }
            }
        }
        public void ClearKeys(string PropertyName)
        {
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].ClearKeys(PropertyName);
            }
        }

        public void KeyCurrentFrame(string PropertyName, float CurrentTime)
        {
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].KeyCurrentFrame(PropertyName, CurrentTime);
            }
        }
        /// <summary>
        /// Clear the animation
        /// </summary>
        public void Clear()
        {
            // restore Original Positions
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].Restore();
            }
            // Clear curves
            AnimatedTransforms.Clear();
        }

        public void AddKeyFrame(ZeltexTransformCurve NewFrames)
        {
            if (AnimatedTransforms.Contains(NewFrames) == false)
            {
                AnimatedTransforms.Add(NewFrames);
            }
        }

        /// <summary>
        /// Add a keyframe to the animation
        /// </summary>
        public void AddKeyFrame(Transform MyObject, ZeltexTransformCurve MyFrames)
        {
            if (MyObject == null)
            {
                Debug.LogError("Trying to add null MyObject");
                return;
            }
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                if (AnimatedTransforms[i].MyObject == MyObject)
                {
                    AnimatedTransforms[i] = MyFrames;
                    return;
                }
            }
            AnimatedTransforms.Add(MyFrames);
        }

        /// <summary>
        /// Add a keyframe to the animation
        /// </summary>
        public ZeltexTransformCurve AddKeyFrame(Transform MyObject)
        {
            if (MyObject == null)
            {
                Debug.LogError("Trying to add null MyObject");
                return null;
            }
            ZeltexTransformCurve MyFrame = null;
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                if (AnimatedTransforms[i].MyObject == MyObject)
                {
                    MyFrame = AnimatedTransforms[i];
                    break;
                }
            }
            if (MyFrame == null)
            {
                MyFrame = new ZeltexTransformCurve(MyObject);
                AnimatedTransforms.Add(MyFrame);
            }
            return MyFrame;
        }
        /// <summary>
        /// Finds a bone using a unique bone name identifier
        /// </summary>
        public Transform FindBone(Transform MyTransform, string BoneName)
        {
            Skeleton MySkeleton = MyTransform.GetComponent<Skeleton>();
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            {
                if (MySkeleton.MyBones[i].GetUniqueName() == BoneName)
                {
                    return MySkeleton.MyBones[i].MyTransform;
                }
            }
            return null;
        }
        #endregion

        #region EditorSpawning
        private Zanimator MyZanimator;

        public override void Spawn()
        {
            GameObject NewZanimator = new GameObject();
            NewZanimator.name = Name;
            MyZanimator = NewZanimator.AddComponent<Zanimator>();
            MyZanimator.SetData(this);
        }

        public void SpawnWithSkeleton(string SkeletonName)
        {
            Spawn();
            SkeletonHandler MySkeletonHandler = MyZanimator.gameObject.AddComponent<SkeletonHandler>();
            Skeleton DataSkeleton = DataManager.Get().GetElement(DataFolderNames.Skeletons, SkeletonName) as Skeleton;
            //DataSkeleton = DataSkeleton.Clone<Skeleton>();
            MySkeletonHandler.SetSkeletonData(DataSkeleton);
            DataSkeleton.Activate(() => { Activate(MyZanimator.transform); });
        }

        public override void DeSpawn()
        {
            if (MyZanimator)
            {
                MyZanimator.gameObject.Die();
            }
        }

        public override bool HasSpawned()
        {
            return (MyZanimator != null);
        }
        #endregion

        public void Activate(Transform RootTransform)
        {
            Debug.Log("Activating Zanimation: " + Name + " for size of " + AnimatedTransforms.Count);
            Deactivate();   // make sure none are attached
            AttemptAttach(RootTransform);
            // Spawn leftovers
            bool DidAnySpawn = false;
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                if (!AnimatedTransforms[i].CouldFindParentInHeirarchy)
                {
                    AnimatedTransforms[i].Spawn(RootTransform);   // for now just spawn here
                    DidAnySpawn = true; // flag for later to attach again!
                }
            }
            // if any spawned, attempt again
            if (DidAnySpawn)
            {
                AttemptAttach(RootTransform);
            }
        }

        private void AttemptAttach(Transform RootTransform)
        {
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                if (AnimatedTransforms[i].MyObject == null)
                {
                    string TransformLocation = AnimatedTransforms[i].TransformLocation;
                    if (TransformLocation.Length > 0)
                    {
                        TransformLocation = TransformLocation.Substring(1, TransformLocation.Length - 1);
                    }
                    string[] TransformParts = TransformLocation.Split('/');
                    if (TransformLocation != "" && TransformParts.Length > 0)
                    {
                        // Debug.Log("TODO: Creating Transform at location: " + TransformLocation + " : " + TransformParts.Length);
                        // First seek Parent Transform
                        Transform ParentTransform = RootTransform;
                        for (int j = 0; j < TransformParts.Length; j++)
                        {
                            ParentTransform = ParentTransform.Find(TransformParts[j]);
                            if (ParentTransform == null)
                            {
                                break;
                            }
                        }
                        if (ParentTransform)
                        {
                            AnimatedTransforms[i].Attach(ParentTransform);
                        }
                        AnimatedTransforms[i].CouldFindParentInHeirarchy = (ParentTransform != null);
                    }
                    else
                    {
                        AnimatedTransforms[i].CouldFindParentInHeirarchy = false;
                    }
                }
            }
        }

        public void Deactivate()
        {
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].Detach();
            }
        }

        public void Restore()
        {
            for (int i = 0; i < AnimatedTransforms.Count; i++)
            {
                AnimatedTransforms[i].Restore();
            }
        }
    }
}