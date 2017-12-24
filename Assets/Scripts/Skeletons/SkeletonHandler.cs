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
    [ExecuteInEditMode]
    public class SkeletonHandler : MonoBehaviour
    {
        public Character Data;
        private SkeletonAnimator MyAnimator;
        [SerializeField]
        private bool HasInit;
        public EditorAction ActionSetDefaultPose = new EditorAction();
        public EditorAction ActionRestoreDefaultPose = new EditorAction();
        public EditorAction ActionGenerateSkeleton = new EditorAction();
        public EditorAction ActionConvertMeshes = new EditorAction();
        protected Skeleton MySkeleton;

        private void Awake()
        {
            Init();
        }

        void Init()
        {
            if (!HasInit)
            {
                HasInit = true;
                MyAnimator = GetComponent<SkeletonAnimator>();
                if (transform.parent)
                {
                    Debug.LogError("Attacking Skeletonhandler");
                    Character MyCharacter = transform.parent.GetComponent<Character>();
                    if (MyCharacter)
                    {
                        Data = MyCharacter;
                        //.GetData().MySkeleton;
                        GetSkeleton().SetSkeletonHandler(this);
                    }
                    else
                    {
                        Debug.LogError("Skeleton Handler has no Character");
                    }
                }
                else
                {
                    Debug.LogError("Skeleton Handler has no parent");
                }
            }
        }

        private void Update()
        {
            Init();
            if (GetSkeleton() != null)// && Application.isPlaying)
            {
                GetSkeleton().SetSkeletonHandler(this);
                GetSkeleton().Update();
            }
            if (ActionSetDefaultPose.IsTriggered())
            {
                GetSkeleton().SetDefaultPose();
            }
            if (ActionRestoreDefaultPose.IsTriggered())
            {
                GetSkeleton().RestoreDefaultPose();
            }
            if (ActionGenerateSkeleton.IsTriggered())
            {
                GetSkeleton().ActionGenerateSkeleton.Trigger();
            }
            if (ActionConvertMeshes.IsTriggered())
            {
                GetSkeleton().SetMeshColliders(true);
            }
        }

        public void SetSkeletonData(Skeleton NewSkeleton)
        {
            if (MySkeleton != null)
            {
                MySkeleton.SetSkeletonHandler(null);
            }
            MySkeleton = NewSkeleton;
            if (MySkeleton != null)
            {
                MySkeleton.SetSkeletonHandler(this);
            }
        }

        public SkeletonAnimator GetAnimator()
        {
            return MyAnimator;
        }

        public Skeleton GetSkeleton()
        {
            if (Data != null && Data.GetData() != null)
            {
                return Data.GetData().MySkeleton;
            }
            else
            {
                return MySkeleton;
            }
        }
        public List<Bone> GetBones()
        {
            if (GetSkeleton() != null)
            {
                return GetSkeleton().MyBones;
            }
            return new List<Bone>();
        }

        public void OnDeath()
        {
            if (GetSkeleton() != null)
            {
                GetSkeleton().DestroyBodyCubes();
            }
            else
            {
                Debug.LogError("No Skeleton cannot destroy cubes.");
            }
        }
    }
}