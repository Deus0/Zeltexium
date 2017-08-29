using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*[System.Serializable]
public class MyAnimationData 
{
	public string ObjectPath;
	public string PropertyName;
	public GameObject MyBindedObject;
	public AnimationCurve MyCurve;

	public MyAnimationData(string NewPath, string NewPropertyName, GameObject NewObject, AnimationCurve NewCurve)
	{
		ObjectPath = NewPath;
		PropertyName = NewPropertyName;
		MyBindedObject = NewObject;
		MyCurve = NewCurve;
	}

	public string GetLabel()
	{
		string Label = "";
		Label += MyBindedObject.name + "-";
		Label += PropertyName + "-";
		//Label += MyCurve.ToString () + "-";
		return Label;
	}
}

[ExecuteInEditMode]
public class CustomAnimator : MonoBehaviour 
{
	[Header("Debug")]
	public bool IsDebugMode = false;
	public KeyCode BeginAnimationKey = KeyCode.O;
	public KeyCode PauseAnimationKey = KeyCode.P;
	private bool IsFixedUpdate = false;
	[Header("References")]
	public AnimationClip MyAnimation;
	public GameObject RootBone;
	[SerializeField] private List<MyAnimationData> MyBones = new List<MyAnimationData>();
	[SerializeField] private List<string> BoneNames = new List<string>();
	[SerializeField] private List<GameObject> BoneReferences = new List<GameObject>();
	[Header("Options")]
    bool IsAddNoise = true;
    public float RandomNoiseAmplitude = 0.01f;
    public bool IsMasking = true;
	public List<string> MyMasking;
	public bool IsImport = false;
	public float AnimationTimeBegin;
	public bool IsAnimating = true;
	Vector3 InitialLocalPosition;
    //public AnimationCurve MyCurve;
    //public List<GameObject> MyBones;
    float AnimationTimeLength;

#if UNITY_EDITOR
    public UnityEditor.EditorCurveBinding[] MyCurveBindings;
	#endif
	Quaternion DebugRotation;

	public float RoundTo2Dec(float Input) 
	{
		return Mathf.RoundToInt (100*Input)/100f;
	}
    public float GetAnimationCount()
    {
        AnimationTimeLength = MyAnimation.length;
        return AnimationTimeLength;
    }

    void Start() 
	{
		if (RootBone == null)
			RootBone = gameObject;
		//AddPositions ();
		ResetAnimation();
	}
	void Update() 
	{
		// actions
		if (IsImport) 
		{ 
			IsImport = false;
			Debug.LogError("Importing Animation!");
			ConvertAnimationToData();
		}
		FrameCount++;
		//if (!IsFixedUpdate && FrameCount % 4 == 0)
        {
			UpdateAnimation ();
		}
	}
	int FrameCount = 0;
	void FixedUpdate() 
	{
		if (IsFixedUpdate)
			UpdateAnimation ();
	}

	void AddToBoneNames(string NewName) 
	{
		for (int i = 0; i < BoneNames.Count; i++) 
		{
			if (BoneNames[i] == NewName)
				return;
		}
		BoneNames.Add (NewName);
	}
	public GameObject FindChildBone(GameObject ParentBone, string BoneName) 
	{
		if (BoneName == "")
			return RootBone;
		for (int i = 0; i < ParentBone.transform.childCount; i++) {
			GameObject ChildBone = ParentBone.transform.GetChild(i).gameObject;
			if (ChildBone.name == BoneName)
				return ChildBone;
			GameObject ChildBone2 = FindChildBone(ChildBone, BoneName);
			if (ChildBone2 != null)
				return ChildBone2;
		}
		return null;
	}
	public void ResetAnimation()
    {
		InitialLocalPosition = transform.localPosition;
		AnimationTimeBegin = Time.time;
		IsAnimating = true;
	}

	GameObject GetBone(string BoneName)
    {
		return gameObject;
	}

	void AddPositions()
    {
		MyBones.Add (new MyAnimationData("", "m_LocalPosition.x", gameObject, new AnimationCurve()));
		MyBones.Add (new MyAnimationData("", "m_LocalPosition.y", gameObject, new AnimationCurve()));
		MyBones.Add (new MyAnimationData("", "m_LocalPosition.z", gameObject, new AnimationCurve()));
	}

	float AnimationTime;

	public bool IsMask(string BoneName) 
	{
		if (!IsMasking)
			return false;
		for (int i = 0; i < MyMasking.Count; i++) 
	    {
			if (BoneName == MyMasking[i])
				return true;
		}
		return false;
	}

	public void UpdateAnimation() 
	{
		if (IsAnimating) 
		{
			AnimationTime = Time.time-AnimationTimeBegin;
            UpdateBonesAtTime(AnimationTime);
            AnimationTime = 0;
		}
	}

    public void UpdateBonesAtTime(float AnimationTime)
    {
        for (int z = 0; z < BoneReferences.Count; z++)
        {
            if (BoneReferences[z] == null)
            {
                ResetBoneReferences();
                return;
            }
            if (!IsMask(BoneReferences[z].name))
            {
                GameObject MyBone = BoneReferences[z];
                Vector3 NewLocalPosition = MyBone.transform.localPosition - InitialLocalPosition;
                Vector3 NewScale = MyBone.transform.localScale;
                Quaternion NewRotation = MyBone.transform.rotation;
                for (int i = 0; i < MyBones.Count; i++)
                {
                    if (BoneNames[z] == MyBones[i].ObjectPath)
                    {
                        try
                        {
                            if (MyBones[i].PropertyName == "m_LocalPosition.x")
                                NewLocalPosition.x = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            else if (MyBones[i].PropertyName == "m_LocalPosition.y")
                                NewLocalPosition.y = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            else if (MyBones[i].PropertyName == "m_LocalPosition.z")
                                NewLocalPosition.z = MyBones[i].MyCurve.Evaluate(AnimationTime);

                            if (MyBones[i].PropertyName == "m_LocalScale.x")
                                NewScale.x = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            else if (MyBones[i].PropertyName == "m_LocalScale.y")
                                NewScale.y = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            else if (MyBones[i].PropertyName == "m_LocalScale.z")
                                NewScale.z = MyBones[i].MyCurve.Evaluate(AnimationTime);

                            if (MyBones[i].PropertyName == "m_LocalRotation.x")
                            {
                                NewRotation.x = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            }
                            else if (MyBones[i].PropertyName == "m_LocalRotation.y")
                            {
                                NewRotation.y = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            }
                            else if (MyBones[i].PropertyName == "m_LocalRotation.z")
                            {
                                NewRotation.z = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            }
                            else if (MyBones[i].PropertyName == "m_LocalRotation.w")
                            {
                                NewRotation.w = MyBones[i].MyCurve.Evaluate(AnimationTime);
                            }
                        }
                        catch (System.MissingMethodException e)
                        {


                        }
                    }
                }
                if (NewLocalPosition != MyBone.transform.localPosition)
                {
                    //Debug.LogError(NewLocalPosition.ToString());
                    MyBone.transform.localPosition = InitialLocalPosition + NewLocalPosition;
                }
                if (NewScale != MyBone.transform.localScale)
                {
                    //Debug.LogError(NewLocalPosition.ToString());
                    MyBone.transform.localScale = NewScale;
                }
                if (NewRotation != MyBone.transform.rotation)
                {
                    if (IsAddNoise)
                    {
                        NewRotation = Quaternion.Euler(
                            NewRotation.eulerAngles.x + RandomNoiseAmplitude * SimplexNoise.Noise.Generate(Time.time),
                            NewRotation.eulerAngles.y + RandomNoiseAmplitude * SimplexNoise.Noise.Generate(Time.time/2f),
                            NewRotation.eulerAngles.z + RandomNoiseAmplitude * SimplexNoise.Noise.Generate(-Time.time));
                    }
                    //Debug.LogError (NewRotation.ToString ());
                    MyBone.transform.localRotation = NewRotation;
                    DebugRotation = NewRotation;
                }
            }
        }
    }
    public AnimationCurve GetCurve(Transform MyBone, string MyPropertyName)
    {
        for (int i = 0; i < MyBones.Count; i++)
        {
            if (MyBone.name == MyBones[i].ObjectPath)
            {
                if (MyBones[i].PropertyName == MyPropertyName)
                    return MyBones[i].MyCurve;
            }
        }
        return null;
    }
    public AnimationCurve GetCurvePositionX(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalPosition.x");
    }
    public AnimationCurve GetCurvePositionY(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalPosition.y");
    }
    public AnimationCurve GetCurvePositionZ(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalPosition.z");
    }
    public AnimationCurve GetCurveRotationX(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalRotation.x");
    }
    public AnimationCurve GetCurveRotationY(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalRotation.y");
    }
    public AnimationCurve GetCurveRotationZ(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalRotation.z");
    }
    public AnimationCurve GetCurveRotationW(Transform MyBone)
    {
        return GetCurve(MyBone, "m_LocalRotation.w");
    }



    public void AnimateBone(GameObject MyBone) 
	{
		
	}
    public void UpdateBones(List<Transform> MyBones)
    {
        BoneReferences.Clear();
        BoneNames.Clear();
        for (int i = 0; i < MyBones.Count; i++)
        {
            BoneReferences.Add(MyBones[i].gameObject);
            BoneNames.Add(MyBones[i].name);
        }
    }
    // Import file from animation
	private void ConvertAnimationToData() 
	{
		#if UNITY_EDITOR
		MyBones.Clear();
		BoneNames.Clear();
		BoneReferences.Clear();
		//MyCurves = UnityEditor.AnimationUtility.GetAllCurves (MyAnimation);
		MyCurveBindings = UnityEditor.AnimationUtility.GetCurveBindings (MyAnimation);
		for (int i = 0; i < MyCurveBindings.Length; i++)
        {
			//MyCurves.Add();
			string BoneName = MyCurveBindings[i].path;
			while (true) {
				BoneName = BoneName.Substring(BoneName.IndexOf("/")+1);
				if (!BoneName.Contains("/"))
					break;
			}
			GameObject MyBone = FindChildBone(gameObject, BoneName);
			AddToBoneNames(BoneName);
			MyBones.Add (new MyAnimationData(BoneName,
				MyCurveBindings[i].propertyName,
				MyBone,
				UnityEditor.AnimationUtility.GetEditorCurve(MyAnimation, MyCurveBindings[i])));
			if (MyAnimation.isLooping) 
			{
				MyBones [i].MyCurve.postWrapMode = WrapMode.Loop;
			}
		}
        ResetBoneReferences();
        #endif
    }
    public void ResetBoneReferences()
    {
        BoneReferences.Clear();
        for (int i = 0; i < BoneNames.Count; i++)
        {
            BoneReferences.Add(FindChildBone(gameObject, BoneNames[i]));
        }
    }
}
*/
                    /*if (IsAddNoise)
                    {
                        NewLocalPosition += new Vector3(
                            SimplexNoise.Noise.Generate(Time.time, 0), 
                        SimplexNoise.Noise.Generate(Time.time, Time.time), 
                        SimplexNoise.Noise.Generate(0,Time.time));
                    }*/