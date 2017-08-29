using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zeltex.AnimationUtilities 
{

	public class BodyColours : SkeletonModifier 
	{
        public Material MyMaterial;
        public bool IsSingleColor = false;
        public Color32 MyColor;

		public bool IsRainbowColours = false;
		public bool IsDarkColours = true;
		public bool IsRandomSizes = false;
		public float MinSizeVariation = 0.9f;
		public float MaxSizeVariation = 1.1f;
		public float BodyPartMinSizeVariation = 0.94f;
		public float BodyPartMaxSizeVariation = 1.06f;

		public bool DebugMode = false;
		public bool IsDebugSizes = false;
		// fade out animation
		[Header("Fade Out Animation")]
		private float FadeTime = 10f;
		private bool IsFadingOut = false;
		private List<Color32> BeginColors = new List<Color32>();
		private float BeginFadingTime;

		List<GameObject> MyBones = null;
		public List<GameObject> MyChildren;
		float IncreaseRate = 0.075f;

		/*void OnGUI()
		{
			if (DebugMode) 
			{
				GUILayout.Label("BodyParts Count [" + MyBodyParts.Count + "]");
				if (MyBodyParts.Count > 0) {
					Transform MyHead = GetBodyPart ("1_HeadMesh Part");
					if (MyHead) 
					{
						GUILayout.Label("Spawning Items from [" + MyHead.name + "]");
					}
				}
			}
			if (IsDebugSizes) 
			{
				if (MyBones == null)
                {
					MyBones = FindChildren (gameObject, true);
				}
				int PositionY = 0;
				GUI.Label (new Rect (0, (++PositionY) * 20f, 100, 20), "Scales: ");
				GUI.Label (new Rect (0, (++PositionY) * 20f, 750, 20), "\tNames\t: LocalScale \t Lossy Scale");
				for (int i = 0; i < MyBones.Count; i++)
                {
					//BodyPart MyBodyPart = MyBones[i].transform.GetChild(0)
					GUI.Label (new Rect (0, (++PositionY) * 20f, 750, 20), "\t" + MyBones [i].name + "\t: " + MyBones [i].transform.localScale 
						+ "\t: " + MyBones [i].transform.lossyScale
					);
				}
			}
		}*/

		// Use this for initialization
		void Start () 
		{
			//ChangeBodyColours ();	// should be in another script lol
			//MutateBodySizes ();
			//MutateBodyScale ();
			//ClearData ();
		}

		void Update() 
		{
			AnimateFade ();
		}

		public void MutateBodyScale() 
		{
			float UniformScaler = Random.Range (MinSizeVariation, MaxSizeVariation);
			transform.localScale = new Vector3 (transform.localScale.x * UniformScaler,
				transform.localScale.y * UniformScaler,
				transform.localScale.z * UniformScaler);
		}
		// Fade OnDeath
		public void OnDeath() 
		{
			StartCoroutine(DelayedFade (15));
		}

		IEnumerator DelayedFade(int Time)
		{
			yield return new WaitForSeconds (Time);
			FadeOut ();
		}

		void FadeOut()
		{
			if (!IsFadingOut)
			{
				Debug.Log ("Begun Fading " + BeginFadingTime);
				GatherBodyParts ();
				//transform.parent.GetComponent<Zeltex.Items.Inventory> ().DropAllItems (GetBodyPart("1_HeadMesh Part"));
				BeginFadingTime = Time.time;
				IsFadingOut = true;
				for (int i = 0; i < MyBodyParts.Count; i++)
                {
                    Material MyMaterial = GetBodyMaterial(i);
                    BeginColors.Add(MyMaterial.color);
                    MyMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);
                    MyMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                    //MyMaterial.renderQueue = 3;
                    MyMaterial.SetFloat("_Mode", 3);
                    MyMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    MyMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    MyMaterial.SetInt("_ZWrite", 0);
                    MyMaterial.DisableKeyword("_ALPHATEST_ON");
                    MyMaterial.EnableKeyword("_ALPHABLEND_ON");
                    MyMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    MyMaterial.renderQueue = 3000;
                }
			}
		}
		void AnimateFade() 
		{
			if (IsFadingOut) 
			{
				for (int i = 0; i < MyBodyParts.Count; i++) 
				{
					Color32 MyColor = Color32.Lerp(BeginColors[i],
						new Color32(BeginColors[i].r, BeginColors[i].g, BeginColors[i].b, 0),
						(Time.time-BeginFadingTime)/FadeTime);
					SetBodyColor (i, MyColor);
				}
				if (Time.time - BeginFadingTime > FadeTime) 
				{
					Destroy (transform.parent.gameObject);
				}
			}
		}
		// should be in its own script
		public void ChangeBodyColours() 
		{
			GatherBodyParts ();
			int Max = 255;
			if (IsDarkColours)
				Max = 125;
			byte red = (byte)Random.Range(0,Max);
			byte green = (byte)Random.Range(0,Max);
			byte blue = (byte)Random.Range(0,Max);
			
			for (int i = 0; i < MyBodyParts.Count; i++) 
			{
                SetBodyMaterial(i, new Material(MyMaterial));
				if (IsRainbowColours)
				{
					red = (byte)Random.Range(0,255);
					green = (byte)Random.Range(0,255);
					blue = (byte)Random.Range(0,255);
				}
				if (MyBodyParts[i].tag != null)
				if (MyBodyParts[i].tag != "Item")
				{
                    if (IsSingleColor)
                            GetBodyMaterial(i).color = MyColor;
                     else
					    GetBodyMaterial(i).color = new Color32(red, green, blue,255);
				}
			}
		}

		/*public void MutateBodySizes() 
		{
			if (IsRandomSizes)
				for (int i = 0; i < MyBodyParts.Count; i++) 
				{
					if (MyBodyParts [i].tag != "Item") {
						SetBoneRandomScale (MyBodyParts [i]);
					}
				}
		}*/
		/*public void IncreaseBonScale(GameObject MyTargetBone) 
		{
			BodyPart MyBodyPart = MyTargetBone.GetComponent<BodyPart> ();
			if (MyBodyPart && !MyBodyPart.IsRigidBody) 
			{
				if (MyBodyPart.gameObject.tag != "Item") 
				{
					MultiplyBoneScale (MyTargetBone, 1f + IncreaseRate);
				}
			}
		}
		public void DecreaseBoneScale(GameObject MyTargetBone) 
		{
			BodyPart MyBodyPart = MyTargetBone.GetComponent<BodyPart> ();
			if (MyBodyPart && !MyBodyPart.IsRigidBody) {
				MultiplyBoneScale (MyTargetBone, 1f - IncreaseRate);
			}
		}*/

		/*public void MultiplyBoneScale(GameObject MyTargetBone, float Multiplier) 
		{
			MyTargetBone = MyTargetBone.transform.parent.gameObject;
			Vector3 MyScaleMultiplier = MyTargetBone.transform.localScale*Multiplier;
			SetBoneScale (MyScaleMultiplier, MyTargetBone);
		}

		public void SetBoneRandomScale(GameObject MyParentBone) 
		{
			MyParentBone = MyParentBone.transform.parent.gameObject;
			Vector3 MyScaleMultiplier;// = MyBodyParts[i].transform.localScale;
			MyScaleMultiplier.x = Random.Range (BodyPartMinSizeVariation, BodyPartMaxSizeVariation);
			MyScaleMultiplier.y = Random.Range (BodyPartMinSizeVariation, BodyPartMaxSizeVariation);
			MyScaleMultiplier.z = Random.Range (BodyPartMinSizeVariation, BodyPartMaxSizeVariation);
			SetBoneScale (MyScaleMultiplier, MyParentBone);
		}

		// sets the scale of a gameobject while not affect its children bones
		public void SetBoneScale(Vector3 NewScale, GameObject MyParentBone) 
		{
			MyChildren = FindChildren (MyParentBone, true);
			List<Vector3> BeforeLossyScale = new List<Vector3> ();
			List<Vector3> BeforeLocalScale = new List<Vector3> ();
			for (int i = 0; i < MyChildren.Count; i++) 
			{
				BeforeLossyScale.Add (MyChildren [i].transform.lossyScale);
				//BeforeLocalScale.Add (MyChildren [i].transform.localScale);
			}
			List<Transform> MyDirectChildren = new List<Transform> ();
			for (int i = 0; i < MyParentBone.transform.childCount; i++) 
			{
				if (MyParentBone.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>() == null)
				{
					MyDirectChildren.Add (MyParentBone.transform.GetChild(i));
				}
			}
			Vector3 ScaleDifference = NewScale - MyParentBone.transform.localScale;
			MyParentBone.transform.localScale = NewScale;
			// I just need to reset the scales since their positions are good

			for (int i = 0; i < MyDirectChildren.Count; i++) 
			{
				//SetLossyScale(MyChildren[i], BeforeLossyScale[i]);
				//Vector3 ParentScale = MyDirectChildren[i].transform.parent.localScale;	// it needs to be multiplied to get lossy scale as before
				//NewChildScale = new Vector3(NewChildScale.x/(NewScale.x),
				//                            NewChildScale.y/(NewScale.y),
				//                            NewChildScale.z/(NewScale.z));
				//MyDirectChildren [i].transform.localScale = NewChildScale;
			}
		}

		public void SetLossyScale(GameObject MyObject, Vector3 NewScale) 
		{
			// first detatch and children
			List<Transform> MyDirectChildren = new List<Transform> ();
			for (int i = 0; i < MyObject.transform.childCount; i++) {
				//if (MyObject.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>() == null) {
					MyDirectChildren.Add (MyObject.transform.GetChild(i));
				//}
			}
			Transform OldParent = MyObject.transform.parent;
			MyObject.transform.parent = null;
			MyObject.transform.DetachChildren ();
			MyObject.transform.SetParent(OldParent);
			// then scale it
			MyObject.transform.localScale = NewScale;
		}

		public void MultiplyScale(GameObject MyObject, Vector3 ScaleMultiplier)
		{
			Vector3 MyScale = MyObject.transform.localScale;
			MyScale.x *= ScaleMultiplier.x;
			MyScale.y *= ScaleMultiplier.y;
			MyScale.z *= ScaleMultiplier.z;
			MyObject.transform.localScale = MyScale;
		}*/
	}
}
