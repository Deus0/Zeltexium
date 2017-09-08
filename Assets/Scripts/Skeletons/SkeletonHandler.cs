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
            if (Data != null)
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
        }

        public SkeletonAnimator GetAnimator()
        {
            return MyAnimator;
        }

        public Skeleton GetSkeleton()
        {
            return Data.GetData().MySkeleton;
        }
        public List<Bone> GetBones()
        {
            if (Data != null)
            {
                return GetSkeleton().MyBones;
            }
            return new List<Bone>();
        }
    }
}