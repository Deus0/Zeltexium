using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.AnimationUtilities
{
	public class SkeletonModifier : MonoBehaviour
    {
		[SerializeField] protected List<GameObject> MyBodyParts = new List<GameObject> ();
		protected List<int> MyParentIndexes = new List<int>();

		public Transform GetBodyPart(string BodyPartName) 
		{
			for (int i = 0; i < MyBodyParts.Count; i++) 
			{
				if (MyBodyParts [i]) {
					if (MyBodyParts [i].name == BodyPartName) {
						return MyBodyParts [i].transform;
					}
				}
			}
			return null;
		}
		protected GameObject GetParentBodyPart(int Index, List<GameObject> MySpawnedBodyParts)
		{
			if (Index >= 0 && Index < MyParentIndexes.Count) 
			{
				int ParentIndex = MyParentIndexes[Index];
				if (ParentIndex != Index) 
				{
					if (ParentIndex >= 0 && ParentIndex < MySpawnedBodyParts.Count) 
					{
						return MySpawnedBodyParts [MyParentIndexes [Index]];
					}
				}
			}
			return null;
		}

		protected void ClearData() 
		{
			MyBodyParts.Clear ();
		}
        protected void SetBodyMaterial(int MyIndex, Material NewMaterial)
        {
            MyBodyParts[MyIndex].GetComponent<MeshRenderer>().material = NewMaterial;
        }
		protected Material GetBodyMaterial(int MyIndex)
		{
			return MyBodyParts[MyIndex].GetComponent<MeshRenderer>().material;
		}
		protected void SetBodyColor(int Index, Color32 NewColor) 
		{
			if (Index < 0 || Index >= MyBodyParts.Count)
				return;
			MeshRenderer MyMeshPart = MyBodyParts [Index].GetComponent<MeshRenderer> ();
			if (MyMeshPart == null)
				return;
			MyMeshPart.material.color = NewColor;
		}

		protected void GatherBodyParts()
		{
			ClearData ();
			MyBodyParts = FindChildren (gameObject, false);
			//CheckChildMeshes (gameObject, -1);	//transform.GetChild (0).
		}

		protected List<GameObject> FindChildren(GameObject Parent) {
			return FindChildren (Parent, false);
		}

		protected List<GameObject> FindChildren(GameObject Parent, bool IsBone) {
			MyParentIndexes.Clear ();
			List<GameObject> MyList = new List<GameObject> ();
			if (Parent.GetComponent<MeshFilter>() != null) {
				Parent = Parent.transform.parent.gameObject;
			}
			//Debug.LogError ("Finding children of: " + Parent.name);
			CheckChildMeshes (Parent, 0, MyList, IsBone);
			return MyList;
		}

		// assuming the mesh parts have no children
		// assuming the actual bos are just transforms
		protected void CheckChildMeshes(GameObject Parent, int ParentIndex, List<GameObject> MyList, bool IsAddBone) 
		{
			for (int i = 0; i < Parent.transform.childCount; i++) {
				GameObject MyChild = Parent.transform.GetChild(i).gameObject;	// grab the bone
				bool IsMeshObject = (MyChild.GetComponent<MeshFilter>() != null);
					{
						// adds either the bone object or the mesh
						if (IsAddBone && !IsMeshObject) {
							MyList.Add (MyChild);
							MyParentIndexes.Add (ParentIndex);
							ParentIndex = MyList.Count-1;	// new parent is the one i just added
						}
						else if (IsMeshObject && !IsAddBone) {
							MyList.Add (MyChild);
							MyParentIndexes.Add (ParentIndex);
							ParentIndex = MyList.Count-1;	// new parent is the one i just added
						}
						if (!IsMeshObject)
							CheckChildMeshes (MyChild, ParentIndex, MyList, IsAddBone);
					}
			}
		}
	}
}