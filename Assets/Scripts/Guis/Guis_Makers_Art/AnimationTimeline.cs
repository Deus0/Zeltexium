using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Zeltex.Util;
using System.IO;
using MakerGuiSystem;
using Zeltex.Skeletons;

namespace Zeltex.Guis.Maker
{
    [System.Serializable]
    public enum MyAnimationMode
    {
        Selection,
        Position,
        Rotation,
        Scale
    }
    /// <summary>
    /// A timeline for our animation!
    /// </summary>
    public class AnimationTimeline : MonoBehaviour,
                                IPointerClickHandler
    {
        #region Variables
        [SerializeField]
        private MyAnimationMode MyMode;

        [Header("Tick")]
        [SerializeField]
        private Material MyTickMaterial;
        [SerializeField]
        private Color32 MyTickColor = Color.green;
        [SerializeField]
        private Color32 CurveTickColor = Color.red;
        [SerializeField]
        private float TicksPerSecond = 30;
        [SerializeField]
        private float TickSkipper = 2.5f;    // for drawing the time line

        [Header("References")]
        [SerializeField]
        private SkeletonPainter MySkeletonPainter;
        [SerializeField]
        private GameObject TickedTimeObject; // gui for slider
        [SerializeField]
        private IndexController MyIndexController;
        [SerializeField]
        private InputField NameInput;  // rename animation!
        [SerializeField]
        private InputField TimeInput;  // rename animation!

        // tick and animator references
        private SkeletonAnimator MyAnimator;
        private List<GameObject> GridTicks = new List<GameObject>();
        private List<GameObject> CurveTicks = new List<GameObject>();
        #endregion

        #region Behaviour

        /// <summary>
        /// When clicked, a new timeline position is selected which updates the animation position
        /// </summary>
        public void OnPointerClick(PointerEventData MyEventData)
        {
            if (MyAnimator)
            {
                //Debug.LogError("Clicked at: " + MyEventData.position.x);
                Vector2 PositionInRect;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    GetComponent<RectTransform>(),
                    MyEventData.position,
                    Camera.main,
                    out PositionInRect);
                PositionInRect.x += GetComponent<RectTransform>().GetWidth() / 2f;
                //Debug.LogError("Clicked at2: " + PositionInRect.x);
                // first get real time time
                MyAnimator.CurrentTime = MyAnimator.TotalTime * (PositionInRect.x / GetComponent<RectTransform>().GetWidth());
                // now correct the time
                MyAnimator.CurrentTime *= (TicksPerSecond / TickSkipper);
                MyAnimator.CurrentTime = Mathf.RoundToInt(MyAnimator.CurrentTime);
                MyAnimator.CurrentTime /= (TicksPerSecond / TickSkipper);
                if (MyAnimator.CurrentTime > MyAnimator.TotalTime)
                {
                    MyAnimator.CurrentTime = MyAnimator.TotalTime;
                }
                if (MyAnimator.CurrentTime < 0)
                {
                    MyAnimator.CurrentTime = 0;
                }
                MyAnimator.OnUpdateTime();
            }
        }

        /// <summary>
        /// When updating total time from the skeleton selected
        /// </summary>
        public void OnUpdatedTotalTime(float NewTotalTime)
        {
            if (MyAnimator && NewTotalTime != MyAnimator.TotalTime)
            {
                MyAnimator.TotalTime = NewTotalTime;
                ClearCurveTicks();
                ClearGrid();
                GenerateGrid();
            }
        }

        /// <summary>
        /// Sets the new skeleton animator
        /// </summary>
        public void SetAnimator(SkeletonAnimator NewAnimator)
        {
            MyAnimator = NewAnimator;
            if (MyAnimator == null)
            {
                // disable buttons
                OnDisabled();
            }
            else
            {
                // enable buttons
                MyAnimator.OnUpdateFrames.AddEvent(RefreshCurveTicks);
                MyAnimator.OnUpdatedTimeEvent.AddEvent(OnUpdatedTime);
                MyIndexController.SetMaxSelected(GetSize());
                GenerateGrid();
            }
        }

        /// <summary>
        /// Remove the skeleton animator
        /// </summary>
        public void RemoveAnimator(SkeletonAnimator NewAnimator)
        {
            if (MyAnimator == NewAnimator)
            {
                OnDisabled();
            }
        }

        /// <summary>
        /// Disable timeline, and ui
        /// </summary>
        private void OnDisabled()
        {
            NameInput.interactable = false;
            NameInput.text = "";
            TimeInput.interactable = false;
            TimeInput.text = "";
            ClearCurveTicks();
            if (MyIndexController.MaxSelected != 0)
            {
                MyIndexController.SetMaxSelected(0);
            }
            if (MyAnimator)
            {
                MyAnimator.OnUpdateFrames.RemoveListener(RefreshCurveTicks);
                MyAnimator.OnUpdatedTimeEvent.RemoveListener(OnUpdatedTime);
            }
        }
        #endregion

        #region IndexController

        /// <summary>
        /// When there is nothing left in the list
        /// </summary>
        private void OnListEmpty()
        {
            OnDisabled();
        }

        public void OnBegin()
        {
            MyIndexController.RemovedOldIndex();
            InitiateController();
        }

        private void InitiateController()
        {
            if (MyIndexController)
            {
                // Remove listeners
                MyIndexController.OnIndexUpdated.RemoveAllListeners();
                MyIndexController.OnAdd.RemoveAllListeners();
                MyIndexController.OnRemove.RemoveAllListeners();
                MyIndexController.OnListEmpty.RemoveAllListeners();
                // Add listeners
                MyIndexController.OnIndexUpdated.AddEvent(OnUpdatedIndex);
                MyIndexController.OnAdd.AddEvent(OnAdd);
                MyIndexController.OnRemove.AddEvent(OnRemove);
                MyIndexController.OnListEmpty.AddEvent(OnListEmpty);
                // Begin!
                MyIndexController.SetMaxSelected(GetSize());
                MyIndexController.OnBegin();
            }
        }

        private int GetSelected()
        {
            return MyIndexController.SelectedIndex;
        }

        private int GetSize()
        {
            if (MyAnimator)
            {
                return MyAnimator.MyAnimations.Count;
            }
            else
            {
                return 0;
            }
        }

        private void OnAdd(int Index)
        {
            if (MyAnimator)
            {
                MyAnimator.MyAnimations.Add(new ZeltexAnimation());
                MyIndexController.SetMaxSelected(GetSize());
                MyIndexController.SelectIndex(GetSize() - 1);
                MyAnimator.SelectedIndex = MyIndexController.SelectedIndex;
                MyAnimator.OnUpdateFrames.Invoke();
                NameInput.interactable = true;
                TimeInput.interactable = true;
                Debug.Log("Added new animation, AnimationsIndex: " + MyAnimator.SelectedIndex);
            }
        }

        private void OnRemove(int Index)
        {
            if (MyAnimator)
            {
                MyAnimator.MyAnimations.RemoveAt(Index);
                //MyAnimator.Delete();
                MyIndexController.SetMaxSelected(GetSize());
                MyIndexController.SelectIndex(MyIndexController.SelectedIndex - 1);
                MyAnimator.SelectedIndex = MyIndexController.SelectedIndex;
            }
        }

        public void ForceUpdated()
        {
            MyIndexController.RemovedOldIndex();
            MyIndexController.ForceSelect(MyIndexController.SelectedIndex);
        }

        private void OnUpdatedIndex(int Index)
        {
            NameInput.interactable = true;
            MyAnimator.Select(MyIndexController.SelectedIndex);
            ClearCurveTicks();  // clear ui ticks
                                //MyAnimator.Load(MyAnimator.SelectedIndex);
                                // Update gui
           // MyIndexText.text = "" + (1 + MyAnimator.SelectedIndex);// + "/" + MyAnimator.MyAnimations.Count;
            NameInput.text = MyAnimator.MyAnimations[MyAnimator.SelectedIndex].Name;
            MyAnimator.Reset();
            MyAnimator.OnUpdateFrames.Invoke();
        }
        #endregion

        #region Frames

        /// <summary>
        /// Add keyframes
        /// </summary>
        public void AddEndingKeyFrames()
        {
            if (MyAnimator)
            {
                Transform MySelectedTransform = MySkeletonPainter.GetSelectedBone();
                if (MySelectedTransform != null)
                {
                    MyAnimator.AddKeyFrame(
                        MySelectedTransform,
                        0,
                        MySelectedTransform.localPosition,
                        MySelectedTransform.localEulerAngles,
                        MySelectedTransform.localScale);
                    float MyTime = Mathf.RoundToInt(MyAnimator.TotalTime * TicksPerSecond) / TicksPerSecond;
                    MyAnimator.AddKeyFrame(
                        MySelectedTransform,
                        MyTime,
                        MySelectedTransform.localPosition,
                        MySelectedTransform.localEulerAngles,
                        MySelectedTransform.localScale);
                    RefreshCurveTicks();
                }
            }
        }

        /// <summary>
        /// Add a keyframe for just one frame
        /// </summary>
        public void AddKeyFrame()
        {
            Transform MySelectedTransform = MySkeletonPainter.GetSelectedBone();
            if (MySelectedTransform != null)
            {
                float MyTime = Mathf.RoundToInt(MyAnimator.CurrentTime * TicksPerSecond) / TicksPerSecond;
                MyAnimator.AddKeyFrame(
                    MySelectedTransform,
                    MyTime,
                    MySelectedTransform.localPosition,
                    MySelectedTransform.localEulerAngles,
                    MySelectedTransform.localScale);
                RefreshCurveTicks();
            }
        }

        /// <summary>
        /// Deletes the selected keyframe - at current time position
        /// </summary>
        public void DeleteKeyFrame()
        {
            if (MyAnimator)
            {
                MyAnimator.DeleteKeyFrame(MySkeletonPainter.GetSelectedBone());
                RefreshCurveTicks();
            }
        }

        /// <summary>
        /// Delets all the keys of a skeleton animation, based on a selected bone
        /// </summary>
        public void DeleteAllKeys()
        {
            if (MyAnimator)
            {
                MyAnimator.DeleteAllKeys(MySkeletonPainter.GetSelectedBone());
                RefreshCurveTicks();
            }
        }
        #endregion

        #region Animator

        /// <summary>
        /// When time updates
        /// </summary>
        public void OnUpdatedTime()
        {
            TickedTimeObject.GetComponent<RectTransform>().anchoredPosition = GetTimePosition(
                                MyAnimator.CurrentTime * TicksPerSecond,
                                TickedTimeObject.GetComponent<RectTransform>().GetSize()
                                );
            //MySkeletonViewer.OnUpdateTransform();
        }

        /// <summary>
        /// Updates the skeleton animation name
        /// </summary>
        public void UpdateName(string NewName)
        {
            if (MyAnimator && MyAnimator.MyAnimations.Count != 0)
            {
                MyAnimator.MyAnimations[MyAnimator.SelectedIndex].Name = NewName;
            }
        }

        /// <summary>
        /// Set the animator loops
        /// </summary>
        public void SetLoop(bool IsLoop)
        {
            MyAnimator.IsAnimationLoop = IsLoop;
        }

        /// <summary>
        /// Stop the animation
        /// </summary>
        public void Stop()
        {
            MyAnimator.Stop();
        }

        /// <summary>
        /// Begin the animation
        /// </summary>
        public void Play()
        {
            MyAnimator.Play();
        }
        #endregion

        #region CurveTicks

        /// <summary>
        /// Clears and regenerates the AnimationCurve ticks
        /// </summary>
        public void RefreshCurveTicks()
        {
            ClearCurveTicks();
            GenerateCurveTicks();
        }

        /// <summary>
        /// Generates ticks on the time line for the selected bones keyframes
        /// </summary>
        private void GenerateCurveTicks()
        {
            if (MyAnimator && MyAnimator.MyAnimations.Count != 0)
            {
                MyAnimator.SelectedIndex = Mathf.Clamp(MyAnimator.SelectedIndex, 0, MyAnimator.MyAnimations.Count);
                for (int i = 0; i < MyAnimator.MyAnimations[MyAnimator.SelectedIndex].MyKeyFrames.Count; i++)
                {
                    if (MyAnimator.MyAnimations[MyAnimator.SelectedIndex].MyKeyFrames[i].MyObject == MySkeletonPainter.GetSelectedBone())
                    {
                        CreateTicksForCurve(MyAnimator.MyAnimations[MyAnimator.SelectedIndex].MyKeyFrames[i].AnimationCurvePositionX);  // for the selected bone, create the curve positions
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Clears all the ticks
        /// </summary>
        private void ClearCurveTicks()
        {
            for (int i = 0; i < CurveTicks.Count; i++)
            {
                Destroy(CurveTicks[i]);
            }
            CurveTicks.Clear();
        }

        /// <summary>
        /// Creates ticks for a curve
        /// </summary>
        private void CreateTicksForCurve(AnimationCurve MyCurve)
        {
            if (MyCurve == null)
            {
                //Debug.LogError("No Curve Found");
                return;
            }
            //Debug.LogError("Curve Found! wow..");
            for (int i = 0; i < MyCurve.keys.Length; i++)
            {
                CreateCurveTick(MyCurve.keys[i].time);
            }
        }

        public GameObject CreateCurveTick(float TimePosition)
        {
            //Debug.LogError("Creating Curve Tick at: " + TimePosition);
            float Height = gameObject.GetComponent<RectTransform>().GetSize().y;
            Vector2 TickSize = new Vector2(2.5f, Height * (9 / 10f));
            GameObject MyCurveTick = CreateTick(Mathf.RoundToInt(TimePosition * TicksPerSecond), CurveTickColor, TickSize);
            CurveTicks.Add(MyCurveTick);
            MyCurveTick.name = "CurveTick " + TimePosition;
            return MyCurveTick;
        }

        /// <summary>
        /// Get the position based on the time and size of the grid
        /// </summary>
        public Vector2 GetTimePosition(float MyTime, Vector2 MySize)
        {
            float Height = gameObject.GetComponent<RectTransform>().GetSize().y;
            RectTransform MyRectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 MyPosition = MyRectTransform.position;
            float TickWidth = MyRectTransform.GetSize().x / ((float) MyAnimator.TotalTime * TicksPerSecond);
            Vector2 SpawnPosition = new Vector2(MyTime * TickWidth + MySize.x / 4f - 1, MySize.y / 2f); //
            return SpawnPosition;
        }

        /// <summary>
        /// Create a tick along the timeline
        /// </summary>
        private GameObject CreateTick(float MyTime, Color32 TickColor, Vector2 TickSize)
        {
            RectTransform MyRectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 MyPosition = MyRectTransform.position;
            float Width = MyRectTransform.GetSize().x;
            float Height = MyRectTransform.GetSize().y;
            float TickWidth = Width / ((float)MyAnimator.TotalTime * TicksPerSecond);
            //GameObject NewTick = (GameObject)Instantiate(GridLinePrefab, SpawnPosition, Quaternion.identity);
            GameObject NewTick = new GameObject();
            NewTick.name = "Tick " + MyTime;
            RawImage TickImage = NewTick.AddComponent<RawImage>();
            RectTransform MyTickTransform = NewTick.GetComponent<RectTransform>();
            MyTickTransform.SetParent(MyRectTransform, false);
            MyTickTransform.SetAnchors(new Vector2());
            MyTickTransform.SetPivot(new Vector2(0.5f, 0.5f));
            MyTickTransform.SetSize(TickSize);
            Vector2 SpawnPosition = new Vector2(MyTime * TickWidth + MyTickTransform.GetWidth() / 2f, MyTickTransform.GetHeight() / 2f);
            MyTickTransform.anchoredPosition = SpawnPosition;
            TickImage.color = TickColor;
            TickImage.material = MyTickMaterial;
            return NewTick;
        }

        #endregion

        #region GridTicks

        /// <summary>
        /// Create a tick along the grid, for the grid
        /// </summary>
        GameObject CreateGridTick(float MyTime, Color32 TickColor)
        {
            float Height = gameObject.GetComponent<RectTransform>().GetSize().y;
            Vector2 TickSize;
            if (MyTime % 30 == 0)
                TickSize = new Vector2(2, Height);
            else if (MyTime % 15 == 0)
                TickSize = new Vector2(2, Height * (4 / 5f));
            else if (MyTime % 5 == 0)
                TickSize = new Vector2(2, Height * (2 / 3f));
            else
                TickSize = new Vector2(2, Height * (1 / 3f));
            return CreateTick(MyTime, TickColor, TickSize);
        }

        /// <summary>
        /// Clear the grid ticks
        /// </summary>
        private void ClearGrid()
        {
            for (int i = 0; i < GridTicks.Count; i++)
            {
                Destroy(GridTicks[i]);
            }
            GridTicks.Clear();
        }

        /// <summary>
        /// Create a timeline points based off total time
        /// </summary>
        private void GenerateGrid()
        {
            if (MyAnimator)
            {
                for (float i = 0; i <= MyAnimator.TotalTime * TicksPerSecond; i += TickSkipper)
                {
                    GridTicks.Add(CreateGridTick(i, MyTickColor));
                }
            }
        }
#endregion
    }
}

//if (MyKeyFrames[i].MyType == "PositionX")
//if (MyKeyFrames[i].MyTime < CurrentTime)
//{
/*float NewX = MyKeyFrames[i].MyValue;
float EndX = MyKeyFrames[i].MyValue;
float BeginTime = (CurrentTime - MyKeyFrames[i].MyTime);
float EndTime = TotalTime - MyKeyFrames[i].MyTime;
if (i != MyKeyFrames.Count - 1)
{
    EndTime = MyKeyFrames[i+1].MyTime - MyKeyFrames[i].MyTime;
}
float TimePercent = BeginTime / EndTime;
if (i != MyKeyFrames.Count-1)
{
    EndX = MyKeyFrames[i + 1].MyValue;
}
float LerpedX = Mathf.Lerp(NewX, EndX, TimePercent);
//Debug.LogError("TimePercent: " + TimePercent);
//Debug.LogError("LerpedX: " + LerpedX);
MyKeyFrames[i].MyObject.localPosition = new Vector3(LerpedX,
    MyKeyFrames[i].MyObject.localPosition.y, 
    MyKeyFrames[i].MyObject.localPosition.z
    );*/
/*if (MyAnimator.SelectedIndex < MaxSelected)
{
    MyAnimator.Save(MyAnimator.SelectedIndex); // save first
    MyAnimator.Clear();
    ClearCurveTicks();
    MyAnimator.SelectedIndex++;
    MyAnimator.Load(MyAnimator.SelectedIndex);
    // Update gui
    MyIndexText.text = (1 + MyAnimator.SelectedIndex) + "";
}*/
