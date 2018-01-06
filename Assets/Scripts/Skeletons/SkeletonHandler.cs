﻿using System.Collections;
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
		public Skeleton MySkeleton;
		[Header("Skeleton Actions")]
		public EditorAction ActionSaveElement = new EditorAction();
		public EditorAction ActionAddBone = new EditorAction();
		public EditorAction ActionAddBoneVox = new EditorAction();
		public Transform ActionBone = null;

		[Header("Handler Actions")]
		public EditorAction ActionActivate = new EditorAction();
		public EditorAction ActionDeactivate = new EditorAction();
		[Header("Pose Actions")]
        public EditorAction ActionSetDefaultPose = new EditorAction();
        public EditorAction ActionRestoreDefaultPose = new EditorAction();

		[Header("Misc")]
		public Character Data;
		private SkeletonAnimator MyAnimator;
		[SerializeField]
		private bool HasInit;

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
					if (GetSkeleton() != null)
					{
						GetSkeleton().SetSkeletonHandler(this);
					}
					else
					{
						Debug.LogError("Skeleton null ins handler: " + name);
					}
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

				if (ActionSaveElement.IsTriggered())
				{
					// First check if positions have changed in transforms
					GetSkeleton().CheckTransforms();
					GetSkeleton().Save();
				}
				if (ActionAddBone.IsTriggered())
				{
					AddBone();
				}

				if (ActionAddBoneVox.IsTriggered())
				{
					RoutineManager.Get().StartCoroutine(ImportVoxesAsBones());
				}

				if (ActionSetDefaultPose.IsTriggered())
				{
					GetSkeleton().SetDefaultPose();
				}
				if (ActionRestoreDefaultPose.IsTriggered())
				{
					GetSkeleton().RestoreDefaultPose();
				}
				if (ActionActivate.IsTriggered())
				{
					GetSkeleton().Activate();
				}
				if (ActionDeactivate.IsTriggered())
				{
					GetSkeleton().Deactivate();
				}
            }
        }

		public IEnumerator ImportVoxesAsBones() 
		{
			Debug.Log("Imported voxes as bones in " + name);

			yield return DataManager.Get().LoadVoxFiles();
			List<VoxelModel> MyModels = DataManager.Get().GetImportedVoxelModels();

			yield return null;
			if (MyModels[0].Name != "Empty" && MyModels[0].Name != "") 
			{
				for (int i = 0; i < MyModels.Count; i++)
				{
					if ( MyModels[i].Name != "Empty" &&  MyModels[i].Name != "") 
					{
						Debug.Log("Spawning Bone for model: " + MyModels[i].Name);
						Bone NewBone = GetSkeleton().CreateBone(ActionBone);
						NewBone.SetMesh(MyModels[i]);
						NewBone.SetName(MyModels[i].Name + "-Bone");
						yield return NewBone.ActivateRoutine();
					}
				}
				Debug.Log("Imported " + MyModels.Count + " models.");
			}
			else
			{
				Debug.Log("Did not import any vox files.");
			}
		}

		private Bone AddBone()
		{
			Bone NewBone = GetSkeleton().CreateBone(ActionBone);
			if (NewBone == null)
			{
				Debug.LogError("BOne failed to be added to " + name);
			}
			else
			{
				NewBone.ActivateSingle();
				Debug.Log("Successfully added new bone " + NewBone.Name + " to " + name);
				#if UNITY_EDITOR
				UnityEditor.Selection.objects = new Object[] { NewBone.MyTransform };
				#endif
			}
			return NewBone;
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
				MyAnimator = GetComponent<SkeletonAnimator>();
				HasInit = true;
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