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
    public class Zanimator : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private Zanimation Data;
        private List<Zanimation> MyAnimations = new List<Zanimation>();
        public int SelectedIndex = 0;   // current animation playing
        public bool IsPlayOnStart;
        public string BeginAnimationName = "Idle";
        public float CurrentTime;
        //public float TotalTime = 3;
        public bool IsAnimating;
        public bool IsForceLoad;
        private float AnimationSpeed = 2;
        //public List<Zanimation> MyAnimations = new List<Zanimation>();
        public static float TicksPerSecond = 30;
        public static float TickSkipper = 2.5f;    // for drawing the time line
        public UnityEvent OnUpdateFrames = new UnityEvent();
        public UnityEvent OnUpdatedTimeEvent = new UnityEvent();

        [Header("Actions")]
        public EditorAction ActionAttach = new EditorAction();
        public EditorAction ActionDetach = new EditorAction();
        public EditorAction ActionApplyTransformMask = new EditorAction();
        public EditorAction ActionRemoveTransformMask = new EditorAction();

        [Header("Animating")]
        public EditorAction ActionPlay = new EditorAction();
        public EditorAction ActionStop = new EditorAction();

        [Header("KeyFraming")]
        public EditorAction ActionKeyCurrentFrame = new EditorAction();
        public EditorAction ActionClearKeys = new EditorAction();
        public EditorAction ActionCreateTransformCurveOnTransform = new EditorAction();

        [Header("Proecural")]
        public EditorAction ActionGenerate = new EditorAction();

        [Header("ActionsData")]
        public float GenerateTimeLength = 10f;
        public float GenerateNoiseMax = 1f;
        public string ActionPropertyName = "Position";
        public Transform TargetTransform;
        public List<Transform> TransformMask;
        #endregion

        #region Mono
        void Start()
        {
            if (IsPlayOnStart && Application.isPlaying)
            {
                Debug.Log(name + " is playing on start.");
                Play();
            }
            // load in 1 second
            /*MySkeleton = GetComponent<SkeletonHandler>();
            if (MySkeleton && MySkeleton.GetSkeleton() != null)
            {
                MySkeleton.GetSkeleton().OnLoadSkeleton.AddEvent(LoadAnimation);
            }
            else
            {
                Debug.LogError("No skeleton or handler insie animator: " + name);
            }*/
            /*if (transform.parent)
            {
                MyCharacter = transform.parent.GetComponent<Characters.Character>();
            }*/
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
            if (ActionPlay.IsTriggered())
            {
                Play();
            }
            if (ActionStop.IsTriggered())
            {
                Stop();
            }
            if (ActionGenerate.IsTriggered())
            {
                GenerateAnimation();
            }
            // Based on Data
            if (Data != null)
            {
                if (ActionAttach.IsTriggered())
                {
                    Data.Activate(transform);
                }
                if (ActionDetach.IsTriggered())
                {
                    Data.Deactivate();
                }
                if (ActionKeyCurrentFrame.IsTriggered())
                {
                    Data.KeyCurrentFrame(ActionPropertyName, CurrentTime);
                }
                if (ActionClearKeys.IsTriggered())
                {
                    if (TargetTransform)
                    {
                        Data.ClearKeys(ActionPropertyName, TargetTransform);
                    }
                    else
                    {
                        Data.ClearKeys(ActionPropertyName);
                    }
                }
                if (ActionCreateTransformCurveOnTransform.IsTriggered())
                {
                    if (TargetTransform != null)
                    {
                        Transform MyTransform = TargetTransform;
                        string TransformLocation = "/" + MyTransform.name;
                        while (MyTransform != transform)
                        {
                            MyTransform = MyTransform.parent;
                            if (MyTransform != transform)
                            {
                                TransformLocation = "/" + MyTransform.name + TransformLocation;
                            }
                        }
                        Debug.Log("Found Transform Location: " + TransformLocation);
                        ZeltexTransformCurve NewCurve = new ZeltexTransformCurve();
                        NewCurve.TransformLocation = TransformLocation;
                        NewCurve.Name = TargetTransform.name;
                        Data.AddCurve(NewCurve);
                        NewCurve.Attach(TargetTransform);
                    }
                    else
                    {
                        Debug.LogError("You must select a TargetTransform");
                    }
                }

                if (ActionApplyTransformMask.IsTriggered())
                {
                    Data.ApplyTransformMask(TransformMask);
                }
                if (ActionRemoveTransformMask.IsTriggered())
                {
                    Data.RemoveTransformMask();
                }
            }
        }

        private void GenerateAnimation()
        {
            if (Data == null)
            {
                Data = new Zanimation();
            }
            Data.GenerateNewData(GenerateNoiseMax);
        }

        public List<Zanimation> GetAnimations()
        {
            /*if (MyCharacter && MyCharacter.GetData() != null)
            {
                return MyCharacter.GetData().MyAnimations;
            }
            else
            {
                //GetComponent<SkeletonHandler>().GetSkeleton().
                Debug.LogError("Still have not loaded animations in editor.");
                return new List<Zanimation>();
            }*/
            return MyAnimations;
        }
        #endregion

        #region Data

        /// <summary>
        /// Sets the current Zanimation to play - if single mode
        /// </summary>
        public void SetData(Zanimation NewData)
        {
            if (Data != NewData)
            {
                Data = NewData;
            }
        }

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
            ZeltexTransformCurve MyFrame = null;
            for (int i = 0; i < GetAnimations()[SelectedIndex].GetSize(); i++)
            {
                if (GetAnimations()[SelectedIndex].GetTransform(i).MyObject == MyObject)
                {
                    MyFrame = GetAnimations()[SelectedIndex].GetTransform(i);
                    MyFrame.RemoveKeys(MyTime);
                    break;
                }
            }
            if (MyFrame == null)
            {
                MyFrame = new ZeltexTransformCurve(MyObject);
                GetAnimations()[SelectedIndex].AddKeyFrame(MyFrame);
            }
            MyFrame.SetKey("PositionX", MyTime, MyInput.x);
            MyFrame.SetKey("PositionY", MyTime, MyInput.y);
            MyFrame.SetKey("PositionZ", MyTime, MyInput.z);
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
            for (int i = 0; i < GetAnimations()[MyIndex].GetSize(); i++)
            {
                if (GetAnimations()[MyIndex].GetTransform(i).MyObject == MySelectedTransform)
                {
                    GetAnimations()[MyIndex].RemoveTransform(i);
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
            ZeltexTransformCurve MyFrame = null;
            for (int i = 0; i < GetAnimations()[SelectedIndex].GetSize(); i++)
            {
                if (GetAnimations()[SelectedIndex].GetTransform(i).MyObject == MySelectedTransform)
                {
                    MyFrame = GetAnimations()[SelectedIndex].GetTransform(i);
                    MyFrame.RemoveKeys(CurrentTime);
                    break;
                }
            }
            OnUpdateFrames.Invoke();
        }
        #endregion

        #region ChangeAnimationStates
        private Zeltine AnimationRoutine = null;
        /// <summary>
        /// Plays selected animation
        /// </summary>
        public void Play()
        {
            IsAnimating = !IsAnimating;
            if (Application.isPlaying)
            {
                if (!IsAnimating)
                {
                    if (CurrentTime >= Data.GetTimeLength())
                    {
                        CurrentTime = 0;
                        OnUpdateTime();
                    }
                }
            }
            else
            {
                if (AnimationRoutine == null || AnimationRoutine.UniversalRoutine == null)
                {
                    AnimationRoutine = RoutineManager.Get().StartCoroutine(PlayInEditor());
                }
                else
                {
                    Debug.Log("Animator already running!");
                }
            }
        }

        private IEnumerator PlayInEditor()
        {
            yield return null;
            float TimeStarted = Time.realtimeSinceStartup;
            float TimeSince = (Time.realtimeSinceStartup - TimeStarted) * AnimationSpeed;
            float LastTime = Time.realtimeSinceStartup;
            while (TimeSince <= Data.GetTimeLength())
            {
                if (IsAnimating)
                {
                    TimeSince = (Time.realtimeSinceStartup - TimeStarted) * AnimationSpeed;
                    CurrentTime = TimeSince;
                    OnUpdateTime();
                }
                else
                {
                    TimeStarted += (Time.realtimeSinceStartup - LastTime);  // delay the time
                    TimeSince = (Time.realtimeSinceStartup - TimeStarted) * AnimationSpeed;
                }
                LastTime = Time.realtimeSinceStartup;
                yield return null;
            }
            CurrentTime = 0;
            OnUpdateTime();
            if (Data.IsAnimationLoop)
            {
                AnimationRoutine = RoutineManager.Get().StartCoroutine(PlayInEditor());
            }
            else
            {
                AnimationRoutine = null;
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
            if (IsAnimating || CurrentTime != 0)
            {
                IsAnimating = false;
                CurrentTime = 0;
                BeginPoseRestoration();
            }
            AnimationRoutine = null;
        }

        /// <summary>
        /// Resets current animation
        /// </summary>
        public void Reset()
        {
            RestoreDefaultPose();
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
                RestoreDefaultPose();
            }
        }

        /// <summary>
        /// Updates the animations in time
        /// </summary>
        public void OnUpdateTime()
        {
            if (Data != null)
            {
                Data.PlayFrame(CurrentTime);
            }
            if (CurrentTime >= GetTimeLength())
            {
                if (Data != null && Data.IsAnimationLoop && IsAnimating)
                {
                    CurrentTime = GetTimeLength() - CurrentTime;
                }
                else
                {
                    IsAnimating = false;
                    CurrentTime = GetTimeLength();
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

        private void RestoreDefaultPose()
        {
            //Debug.Log("TODO: Implement Restore Default Pose as an generated animation.");
            if (Data != null)
            {
                Data.Restore();
            }
        }

        private void BeginPoseRestoration()
        {
            IsRestoringPose = true;
            //TimeBeginRestoring = Time.time;
            RestoreDefaultPose();    // for now
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

        public float GetTimeLength()
        {
            if (Data != null)
            {
                return Data.GetTimeLength();
            }
            return 0;
        }

        public void SetTimeLength(float NewTimeLength)
        {
            if (Data != null)
            {
                Data.SetTimeLength(NewTimeLength);
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
