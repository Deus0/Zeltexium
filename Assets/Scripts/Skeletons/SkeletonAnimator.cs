using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Zeltex.Util;
using System.IO;

namespace Zeltex.Skeletons
{
    /// <summary>
    /// Animates the skeleton class
    /// </summary>
    [ExecuteInEditMode]
    public class SkeletonAnimator : MonoBehaviour
    {
        #region Variables
        public int SelectedIndex = 0;   // current animation playing
        public bool IsPlayOnStart;
        public string BeginAnimationName = "Idle";
        public float CurrentTime;
        public float TotalTime = 3;
        public bool IsAnimating;
        public bool IsForceLoad;
        public bool IsAnimationLoop = true;
        private float AnimationSpeed = 2;

        private Characters.Character MyCharacter;
        private SkeletonHandler MySkeleton;
        //public List<Zanimation> MyAnimations = new List<Zanimation>();
        public static float TicksPerSecond = 30;
        public static float TickSkipper = 2.5f;    // for drawing the time line
        public UnityEvent OnUpdateFrames = new UnityEvent();
        public UnityEvent OnUpdatedTimeEvent = new UnityEvent();

        public EditorAction PlayPause = new EditorAction();
        public EditorAction ActionGenerateAnimation = new EditorAction();
        [SerializeField]
        private float AnimationRandomness = 0.05f;
        #endregion

        #region Mono
        void Start()
        {
            // load in 1 second
            MySkeleton = GetComponent<SkeletonHandler>();
            if (MySkeleton && MySkeleton.GetSkeleton() != null)
            {
                MySkeleton.GetSkeleton().OnLoadSkeleton.AddEvent(LoadAnimation);
            }
            else
            {
                Debug.LogError("No skeleton or handler insie animator: " + name);
            }
            if (transform.parent)
            {
                MyCharacter = transform.parent.GetComponent<Characters.Character>();
            }
        }
        
        void Update()
        {
            if (IsAnimating && Application.isPlaying)
            {
                CurrentTime += Time.deltaTime * AnimationSpeed;
                OnUpdateTime();
            }
            if (IsRestoringPose)
            {
                UpdateRestoringPose();
            }
            if (ActionGenerateAnimation.IsTriggered())
            {
                GenerateAnimation();
            }
            if (PlayPause.IsTriggered())
            {
                UniversalCoroutine.CoroutineManager.StartCoroutine(PlayOnce());
            }
        }

        private void GenerateAnimation()
        {
            Zanimation NewAnimation = new Zanimation();
            NewAnimation.SetName(NameGenerator.GenerateVoxelName());
            for (int i = 0; i < MyCharacter.GetData().MySkeleton.MyBones.Count; i++)
            {
                ZeltexKeyFrame BoneFrames = new ZeltexKeyFrame(MyCharacter.GetData().MySkeleton.MyBones[i].MyTransform);
                BoneFrames.AnimationCurvePositionY = new AnimationCurve();
                float AnimationLength = 3f;
                for (float TimeIndex = 0; TimeIndex <= AnimationLength; TimeIndex += 0.2f)
                {
                    if (TimeIndex == 0 || TimeIndex == AnimationLength)
                    {
                        BoneFrames.AnimationCurvePositionY.AddKey(
                            new Keyframe(3f, MyCharacter.GetData().MySkeleton.MyBones[i].GetDefaultPosition().y));
                    }
                    else
                    {
                        BoneFrames.AnimationCurvePositionY.AddKey(
                            new Keyframe(TimeIndex,
                            MyCharacter.GetData().MySkeleton.MyBones[i].GetDefaultPosition().y +
                                //Mathf.Sin(TimeIndex) +
                                Random.Range(-AnimationRandomness, AnimationRandomness)));
                    }
                }
                BoneFrames.AnimationCurvePositionY.AddKey(
                    new Keyframe(3f, MyCharacter.GetData().MySkeleton.MyBones[i].GetDefaultPosition().y));
                NewAnimation.AddKeyFrame(MyCharacter.GetData().MySkeleton.MyBones[i].MyTransform, BoneFrames);
            }
            MyCharacter.GetData().MyAnimations.Clear();
            MyCharacter.GetData().MyAnimations.Add(NewAnimation);
        }

        private IEnumerator PlayOnce()
        {
            float TimeStarted = Time.realtimeSinceStartup;
            float TimeSince = (Time.realtimeSinceStartup - TimeStarted) * AnimationSpeed;
            while (TimeSince <= 3f)//GetAnimations()[0])
            {
                TimeSince = (Time.realtimeSinceStartup - TimeStarted) * AnimationSpeed;
                CurrentTime = TimeSince;
                OnUpdateTime();
                yield return null;
            }
            if (IsAnimationLoop)
            {
                UniversalCoroutine.CoroutineManager.StartCoroutine(PlayOnce());
            }
            else
            {
                CurrentTime = 0;
                OnUpdateTime();
            }
        }

        public List<Zanimation> GetAnimations()
        {
            if (MyCharacter && MyCharacter.GetData() != null)
            {
                return MyCharacter.GetData().MyAnimations;
            }
            else
            {
                //GetComponent<SkeletonHandler>().GetSkeleton().
                Debug.LogError("Still have not loaded animations in editor.");
                return new List<Zanimation>();
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// Fixes time to 3 decimal places
        /// </summary>
        public static float FixTime(float MyTime)
        {
            float ActualTicksPerSecond = TicksPerSecond / TickSkipper;
            //return MyTime;
            return (Mathf.RoundToInt(MyTime * ActualTicksPerSecond)) / ActualTicksPerSecond;
        }

        /// <summary>
        /// Clears the selected Animation
        /// </summary>
        public void Clear()
        {
            GetAnimations()[SelectedIndex].Clear();
        }

        /// <summary>
        /// Adds a keyframe at a current frame
        /// </summary>
        public void AddKeyFrame(Transform MyObject, float MyTime, Vector3 MyInput, Vector3 MyRotation, Vector3 InputScale)
        {
            if (MyObject == null)
            {
                Debug.LogError("Trying to add null MyObject");
                return;
            }
            ZeltexKeyFrame MyFrame = null;
            for (int i = 0; i < GetAnimations()[SelectedIndex].MyKeyFrames.Count; i++)
            {
                if (GetAnimations()[SelectedIndex].MyKeyFrames[i].MyObject == MyObject)
                {
                    MyFrame = GetAnimations()[SelectedIndex].MyKeyFrames[i];
                    MyFrame.RemoveKeys(MyTime);
                    break;
                }
            }
            if (MyFrame == null)
            {
                MyFrame = new ZeltexKeyFrame(MyObject);
                GetAnimations()[SelectedIndex].MyKeyFrames.Add(MyFrame);
            }
            MyFrame.AnimationCurvePositionX.AddKey(new Keyframe(MyTime, MyInput.x));
            MyFrame.AnimationCurvePositionY.AddKey(new Keyframe(MyTime, MyInput.y));
            MyFrame.AnimationCurvePositionZ.AddKey(new Keyframe(MyTime, MyInput.z));
            // special magic
            MyRotation = ApplyMagicToRotation(MyRotation);
            MyFrame.AnimationCurveRotationX.AddKey(new Keyframe(MyTime, MyRotation.x));
            MyFrame.AnimationCurveRotationY.AddKey(new Keyframe(MyTime, MyRotation.y));
            MyFrame.AnimationCurveRotationZ.AddKey(new Keyframe(MyTime, MyRotation.z));
            //MyFrame.AnimationCurveRotationW.AddKey(new Keyframe(MyTime, MyRotation.w));
            MyFrame.AnimationCurveScaleX.AddKey(new Keyframe(MyTime, InputScale.x));
            MyFrame.AnimationCurveScaleY.AddKey(new Keyframe(MyTime, InputScale.y));
            MyFrame.AnimationCurveScaleZ.AddKey(new Keyframe(MyTime, InputScale.z));
            OnUpdateFrames.Invoke();   // refresh curve ticks
            //CreateCurveTick(MyTime);
        }
        /// <summary>
        /// Keeps floats between -180 and 180
        /// </summary>
        public static Vector3 ApplyMagicToRotation(Vector3 InputRotation)
        {
            if (InputRotation.x > 180) // if 210, 180-210 = -30! 210 - 360, -150!
            {
                InputRotation.x = InputRotation.x - 360;
            }
            if (InputRotation.y > 180)
            {
                InputRotation.y = InputRotation.y - 360;
            }
            if (InputRotation.z > 180)
            {
                InputRotation.z = InputRotation.z - 360;
            }
            return InputRotation;
        }
        public void DeleteAllKeysFromAllAnimations(Transform MyBone)
        {
            for (int i = 0; i < GetAnimations().Count; i++)
            {
                DeleteAllKeys(MyBone, i);
            }
        }
        public void DeleteAllKeys(Transform MySelectedTransform)
        {
            DeleteAllKeys(MySelectedTransform, SelectedIndex);
        }
        /// <summary>
        /// Deletes all the keyframes in current animation
        /// </summary>
        public void DeleteAllKeys(Transform MySelectedTransform, int MyIndex)
        {
            for (int i = 0; i < GetAnimations()[MyIndex].MyKeyFrames.Count; i++)
            {
                if (GetAnimations()[MyIndex].MyKeyFrames[i].MyObject == MySelectedTransform)
                {
                    GetAnimations()[MyIndex].MyKeyFrames.RemoveAt(i);
                    if (MyIndex == SelectedIndex)
                    {
                        OnUpdateFrames.Invoke();
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// Deletes the keyframe of the selected transform
        /// </summary>
        public void DeleteKeyFrame(Transform MySelectedTransform)
        {
            ZeltexKeyFrame MyFrame = null;
            for (int i = 0; i < GetAnimations()[SelectedIndex].MyKeyFrames.Count; i++)
            {
                if (GetAnimations()[SelectedIndex].MyKeyFrames[i].MyObject == MySelectedTransform)
                {
                    MyFrame = GetAnimations()[SelectedIndex].MyKeyFrames[i];
                    MyFrame.RemoveKeys(CurrentTime);
                    break;
                }
            }
            OnUpdateFrames.Invoke();
        }
        #endregion

        #region ChangeAnimationStates
        /// <summary>
        /// Plays selected animation
        /// </summary>
        public void Play()
        {
            IsAnimating = !IsAnimating;
            if (!IsAnimating)
            {
                if (CurrentTime >= TotalTime)
                {
                    CurrentTime = 0;
                    OnUpdateTime();
                }
            }
        }

        /// <summary>
        /// Selects a new animation and begins playing it
        /// </summary>
        public void Play(int AnimationIndex)
        {
            if (SelectedIndex != AnimationIndex)
            {
                SelectedIndex = AnimationIndex;
                Reset();
                IsAnimating = true;
            }
        }

        /// <summary>
        /// Is the animator playing that animation?
        /// </summary>
        public bool IsPlaying(int AnimationIndex)
        {
            return (IsAnimating && SelectedIndex == AnimationIndex);
        }

        /// <summary>
        /// stops any animation
        /// </summary>
        public void Stop()
        {
            if (IsAnimating)
            {
                IsAnimating = false;
                Debug.Log("Stopping animation: " + SelectedIndex + " on " + transform.parent.name);
                CurrentTime = 0;
                OnUpdateTime();
                BeginPoseRestoration();
            }
        }

        /// <summary>
        /// Resets current animation
        /// </summary>
        public void Reset()
        {
            MySkeleton.GetSkeleton().RestoreDefaultPose();
            CurrentTime = 0;
            OnUpdateTime();
            IsAnimating = false;
        }
        #endregion

        #region Animate
        /// <summary>
        /// sets the new speed of animating
        /// </summary>
        public void SetSpeed(float NewSpeed)
        {
            AnimationSpeed = NewSpeed;
        }

        /// <summary>
        /// blend to new state
        /// </summary>
        public void Select(int NewIndex)
        {
            if (SelectedIndex != NewIndex)
            {
                // turn on blending
                BeginBlending();
                // Switch Animation
                SelectedIndex = NewIndex;
                MySkeleton.GetSkeleton().RestoreDefaultPose();
            }
        }
        /// <summary>
        /// Updates the animations in time
        /// </summary>
        public void OnUpdateTime()
        {
            if (SelectedIndex < 0 || SelectedIndex >= GetAnimations().Count) 
            {
                return;
            }
            for (int i = GetAnimations()[SelectedIndex].MyKeyFrames.Count-1; i >= 0; i--)
            {
                if (GetAnimations()[SelectedIndex].MyKeyFrames[i].MyObject == null)
                {
                    GetAnimations()[SelectedIndex].MyKeyFrames.RemoveAt(i);
                }
                else
                {
                    float ValueX = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurvePositionX.Evaluate(CurrentTime);
                    float ValueY = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurvePositionY.Evaluate(CurrentTime);
                    float ValueZ = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurvePositionZ.Evaluate(CurrentTime);
                    GetAnimations()[SelectedIndex].MyKeyFrames[i].MyObject.localPosition = new Vector3(ValueX, ValueY, ValueZ);
                    float RotateX = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurveRotationX.Evaluate(CurrentTime);
                    float RotateY = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurveRotationY.Evaluate(CurrentTime);
                    float RotateZ = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurveRotationZ.Evaluate(CurrentTime);
                    GetAnimations()[SelectedIndex].MyKeyFrames[i].MyObject.localEulerAngles = new Vector3(RotateX, RotateY, RotateZ);
                    float ScaleX = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurveScaleX.Evaluate(CurrentTime);
                    float ScaleY = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurveScaleY.Evaluate(CurrentTime);
                    float ScaleZ = GetAnimations()[SelectedIndex].MyKeyFrames[i].AnimationCurveScaleZ.Evaluate(CurrentTime);
                    GetAnimations()[SelectedIndex].MyKeyFrames[i].MyObject.localScale = new Vector3(ScaleX, ScaleY, ScaleZ);
                }
            }
            if (CurrentTime >= TotalTime)
            {
                if (IsAnimationLoop && IsAnimating)
                {
                    CurrentTime = TotalTime - CurrentTime;
                }
                else
                {
                    IsAnimating = false;
                    CurrentTime = TotalTime;
                    //CurrentTime = 0;
                }
            }
            OnUpdatedTimeEvent.Invoke();
        }
        #endregion

        #region Blending
        // if BlendedIndex != -1, add the positions together instead of just using one, 
        //  lerp their percentage using the time values
        //int BlendedIndex;
        //float BlendTimeStarted;
        //float BlendTimeLength = 1;  // not sure here

        private void BeginBlending()
        {
           // BlendTimeStarted = Time.time;
            //BlendedIndex = SelectedIndex;   // this will be called in select
        }
        #endregion

        #region RestoringPose
        private bool IsRestoringPose;       // Pose Lerping back to default
        //float TimeBeginRestoring;

        private void BeginPoseRestoration()
        {
            IsRestoringPose = true;
            //TimeBeginRestoring = Time.time;
            MySkeleton.GetSkeleton().RestoreDefaultPose();    // for now
        }
        /// <summary>
        /// Over time, restore the default pose
        /// Lerps between 2 poses
        /// </summary>
        void UpdateRestoringPose()
        {

        }
        #endregion

        #region File

        public void LoadAnimation()
        {
            //Load(SelectedIndex);
            if (IsPlayOnStart)
            {
                // look for 'Idle'
                for (int i = 0; i < GetAnimations().Count; i++)
                {
                    if (GetAnimations()[i].Name == BeginAnimationName)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
                Play();
                IsAnimating = true;
            }
        }

        /// <summary>
        /// Get the data for the skeleton animator
        /// </summary>
        /*public List<string> GetScript()
        {
            List<string> Data = new List<string>();
            Data.Add("/BeginSkeletonAnimator");
            for (int i = 0; i < GetAnimations().Count; i++)
            {
                Data.Add("/BeginAnimation " + GetAnimations()[i].Name);
                Data.AddRange(GetAnimations()[i].GetScript());
                Data.Add("/EndAnimation");
            }
            Data.Add("/EndSkeletonAnimator");
            return Data;
        }
        /// <summary>
        /// Load all the animations script
        /// </summary>
        public void RunScript(List<string> Data)
        {
            GetAnimations().Clear();
            //Debug.LogError("Loading Animation for " + MySkeleton.SkeletonName + "\n" + FileUtil.ConvertToSingle(Data));
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Contains("/BeginAnimation"))
                {
                    Zanimation MyAnimation = new Zanimation();
                    string MyAnimationName = Data[i].Split(' ')[1];
                    MyAnimation.Name = MyAnimationName;
                    for (int j = i; j < Data.Count; j++)
                    {
                        if (Data[j].Contains("/EndAnimation"))
                        {
                            int Index1 = i + 1;
                            int Index2 = j - 1;
                            int ElementCount = (Index2 - Index1) + 1;
                            List<string> MyAnimationScript = Data.GetRange(Index1, ElementCount);
                            MyAnimation.RunScript(transform, MyAnimationScript);
                            GetAnimations().Add(MyAnimation);
                            i = j;
                            break;
                        }
                    }
                }
            }
        }*/
        #endregion
    }
}


/*
if (MyAnimations.Count == 0)
{
    LoadAll();
}
 * public void LoadAll()
{
    // Need to get animations off Skeleton Manager instead
    MyAnimations.Clear();
    SkeletonAnimation MyAnimationData = SkeletonMaker.Get().GetAnimation(MySkeleton.SkeletonName);
    if (MyAnimationData != null)
    {
        for (int i = 0; i < MyAnimationData.MyData.Count; i++)
        {
            MyAnimations.Add(new Zanimation());
            MyAnimations[MyAnimations.Count - 1].Name = MyAnimationData.MyNames[i];
            MyAnimations[MyAnimations.Count - 1].RunScript(transform, FileUtil.ConvertToList(MyAnimationData.MyData[i]));
        }
    }
    OnUpdateFrames.Invoke();
}*/

/*public void Delete()
{
    //List<string> Data = MyAnimations[SelectedIndex].GetScript();
    string FilePath = AnimationTimeline.GetFullFileName(MySkeleton.SkeletonName, MyAnimations[SelectedIndex].Name);
    if (File.Exists(FilePath))
    {
        File.Delete(FilePath);
    }
    MyAnimations.RemoveAt(SelectedIndex);
    //Debug.LogError("Save to path [" + FilePath + "] With [" + Data.Count + "] Files.");
}*/
