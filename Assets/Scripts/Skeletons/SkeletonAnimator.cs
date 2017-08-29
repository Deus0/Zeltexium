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
    public class SkeletonAnimator : MonoBehaviour
    {
        #region Variables
        public int SelectedIndex = 0;   // current animation playing
        public bool IsPlayOnStart;
        public string BeginAnimationName = "Idle";
        public float CurrentTime;
        public float TotalTime = 3;
        public bool IsAnimating;
        public UnityEvent OnUpdateFrames = new UnityEvent();
        public UnityEvent OnUpdatedTimeEvent = new UnityEvent();
        public bool IsForceLoad;
        public bool IsAnimationLoop = true;
        float AnimationSpeed = 2;
        public List<ZeltexAnimation> MyAnimations = new List<ZeltexAnimation>();
        public static float TicksPerSecond = 30;
        public static float TickSkipper = 2.5f;    // for drawing the time line
        private Skeleton MySkeleton;
        #endregion

        #region Mono
        void Start()
        {
            // load in 1 second
            MySkeleton = GetComponent<Skeleton>();
            MySkeleton.OnLoadSkeleton.AddEvent(LoadAnimation);
        }
        
        void Update()
        {
            if (IsAnimating)
            {
                CurrentTime += Time.deltaTime * AnimationSpeed;
                OnUpdateTime();
            }
            if (IsRestoringPose)
            {
                UpdateRestoringPose();
            }
            if (IsForceLoad)
            {
                IsForceLoad = false;
                LoadAnimation();
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
            MyAnimations[SelectedIndex].Clear();
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
            for (int i = 0; i < MyAnimations[SelectedIndex].MyKeyFrames.Count; i++)
            {
                if (MyAnimations[SelectedIndex].MyKeyFrames[i].MyObject == MyObject)
                {
                    MyFrame = MyAnimations[SelectedIndex].MyKeyFrames[i];
                    MyFrame.RemoveKeys(MyTime);
                    break;
                }
            }
            if (MyFrame == null)
            {
                MyFrame = new ZeltexKeyFrame(MyObject);
                MyAnimations[SelectedIndex].MyKeyFrames.Add(MyFrame);
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
            for (int i = 0; i <  MyAnimations.Count; i++)
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
            for (int i = 0; i < MyAnimations[MyIndex].MyKeyFrames.Count; i++)
            {
                if (MyAnimations[MyIndex].MyKeyFrames[i].MyObject == MySelectedTransform)
                {
                    MyAnimations[MyIndex].MyKeyFrames.RemoveAt(i);
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
            for (int i = 0; i < MyAnimations[SelectedIndex].MyKeyFrames.Count; i++)
            {
                if (MyAnimations[SelectedIndex].MyKeyFrames[i].MyObject == MySelectedTransform)
                {
                    MyFrame = MyAnimations[SelectedIndex].MyKeyFrames[i];
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
            MySkeleton.RestoreDefaultPose();
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
                MySkeleton.RestoreDefaultPose();
            }
        }
        /// <summary>
        /// Updates the animations in time
        /// </summary>
        public void OnUpdateTime()
        {
            if (SelectedIndex < 0 || SelectedIndex >= MyAnimations.Count) 
            {
                return;
            }
            for (int i = MyAnimations[SelectedIndex].MyKeyFrames.Count-1; i >= 0; i--)
            {
                if (MyAnimations[SelectedIndex].MyKeyFrames[i].MyObject == null)
                {
                    MyAnimations[SelectedIndex].MyKeyFrames.RemoveAt(i);
                }
                else
                {
                    float ValueX = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurvePositionX.Evaluate(CurrentTime);
                    float ValueY = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurvePositionY.Evaluate(CurrentTime);
                    float ValueZ = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurvePositionZ.Evaluate(CurrentTime);
                    MyAnimations[SelectedIndex].MyKeyFrames[i].MyObject.localPosition = new Vector3(ValueX, ValueY, ValueZ);
                    float RotateX = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurveRotationX.Evaluate(CurrentTime);
                    float RotateY = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurveRotationY.Evaluate(CurrentTime);
                    float RotateZ = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurveRotationZ.Evaluate(CurrentTime);
                    MyAnimations[SelectedIndex].MyKeyFrames[i].MyObject.localEulerAngles = new Vector3(RotateX, RotateY, RotateZ);
                    float ScaleX = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurveScaleX.Evaluate(CurrentTime);
                    float ScaleY = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurveScaleY.Evaluate(CurrentTime);
                    float ScaleZ = MyAnimations[SelectedIndex].MyKeyFrames[i].AnimationCurveScaleZ.Evaluate(CurrentTime);
                    MyAnimations[SelectedIndex].MyKeyFrames[i].MyObject.localScale = new Vector3(ScaleX, ScaleY, ScaleZ);
                }
            }
            if (CurrentTime >= TotalTime)
            {
                if (IsAnimationLoop && IsAnimating)
                {
                    CurrentTime = 0;
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
        int BlendedIndex;
        float BlendTimeStarted;
        float BlendTimeLength = 1;  // not sure here

        private void BeginBlending()
        {
            BlendTimeStarted = Time.time;
            BlendedIndex = SelectedIndex;   // this will be called in select
        }
        #endregion

        #region RestoringPose
        private bool IsRestoringPose;       // Pose Lerping back to default
        float TimeBeginRestoring;

        private void BeginPoseRestoration()
        {
            IsRestoringPose = true;
            TimeBeginRestoring = Time.time;
            if (MySkeleton == null)
            {
                MySkeleton = GetComponent<Skeleton>();
            }
            MySkeleton.RestoreDefaultPose();    // for now
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
                for (int i = 0; i < MyAnimations.Count; i++)
                {
                    if (MyAnimations[i].Name == BeginAnimationName)
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
        public List<string> GetScript()
        {
            List<string> Data = new List<string>();
            Data.Add("/BeginSkeletonAnimator");
            for (int i = 0; i < MyAnimations.Count; i++)
            {
                Data.Add("/BeginAnimation " + MyAnimations[i].Name);
                Data.AddRange(MyAnimations[i].GetScript());
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
            MyAnimations.Clear();
            //Debug.LogError("Loading Animation for " + MySkeleton.SkeletonName + "\n" + FileUtil.ConvertToSingle(Data));
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Contains("/BeginAnimation"))
                {
                    ZeltexAnimation MyAnimation = new ZeltexAnimation();
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
                            MyAnimations.Add(MyAnimation);
                            i = j;
                            break;
                        }
                    }
                }
            }
        }
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
            MyAnimations.Add(new ZeltexAnimation());
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
