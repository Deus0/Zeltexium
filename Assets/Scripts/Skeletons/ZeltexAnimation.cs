using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex.Skeletons
{

    /// <summary>
    /// Each Zanimation contains a bunch of keyframes, connected to transforms
    /// Uses transform names for bone positions
    /// </summary>
    [System.Serializable]
    public class Zanimation : Element
    {
        #region Variables
        public List<ZeltexKeyFrame> MyKeyFrames = new List<ZeltexKeyFrame>();
        string LineType = "";   // used for reading script
        #endregion

        #region Utility
        /// <summary>
        /// Clear the animation
        /// </summary>
        public void Clear()
        {
            // restore Original Positions
            for (int i = 0; i < MyKeyFrames.Count; i++)
            {
                MyKeyFrames[i].Restore();
            }
            // Clear curves
            MyKeyFrames.Clear();
        }

        /// <summary>
        /// Add a keyframe to the animation
        /// </summary>
        public void AddKeyFrame(Transform MyObject, ZeltexKeyFrame MyFrames)
        {
            if (MyObject == null)
            {
                Debug.LogError("Trying to add null MyObject");
                return;
            }
            for (int i = 0; i < MyKeyFrames.Count; i++)
            {
                if (MyKeyFrames[i].MyObject == MyObject)
                {
                    MyKeyFrames[i] = MyFrames;
                    return;
                }
            }
            MyKeyFrames.Add(MyFrames);
        }

        /// <summary>
        /// Add a keyframe to the animation
        /// </summary>
        public ZeltexKeyFrame AddKeyFrame(Transform MyObject)
        {
            if (MyObject == null)
            {
                Debug.LogError("Trying to add null MyObject");
                return null;
            }
            ZeltexKeyFrame MyFrame = null;
            for (int i = 0; i < MyKeyFrames.Count; i++)
            {
                if (MyKeyFrames[i].MyObject == MyObject)
                {
                    MyFrame = MyKeyFrames[i];
                    break;
                }
            }
            if (MyFrame == null)
            {
                MyFrame = new ZeltexKeyFrame(MyObject);
                MyKeyFrames.Add(MyFrame);
            }
            return MyFrame;
        }
        /// <summary>
        /// Finds a bone using a unique bone name identifier
        /// </summary>
        public Transform FindBone(Transform MyTransform, string BoneName)
        {
            Skeleton MySkeleton = MyTransform.GetComponent<Skeleton>();
            for (int i = 0; i < MySkeleton.MyBones.Count; i++)
            {
                if (MySkeleton.MyBones[i].GetUniqueName() == BoneName)
                {
                    return MySkeleton.MyBones[i].MyTransform;
                }
            }
            return null;
        }
        #endregion

        #region File
        /// <summary>
        /// Gets a script from the animation curves
        /// </summary>
        public List<string> GetScript()
        {
            float MicroLimit = 0.01f;
            List<string> Data = new List<string>();
            for (int i = 0; i < MyKeyFrames.Count; i++)
            {
                if (MyKeyFrames[i].MyObject)
                {
                    Data.Add("/KeysBegin " + MyKeyFrames[i].MyObject.name);
                    if (MyKeyFrames[i].AnimationCurvePositionX.keys.Length > 0)
                    {
                        Data.Add("/PositionX");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurvePositionX.keys.Length; j++)
                        {
                            if (MyKeyFrames[i].AnimationCurvePositionX.keys[j].value > -MicroLimit && MyKeyFrames[i].AnimationCurvePositionX.keys[j].value < MicroLimit)
                            {
                                MyKeyFrames[i].AnimationCurvePositionX.keys[j].value = 0;
                            }
                            Data.Add(MyKeyFrames[i].AnimationCurvePositionX.keys[j].time + " " + MyKeyFrames[i].AnimationCurvePositionX.keys[j].value);
                        }
                        Data.Add("/EndPositionX");
                    }
                    if (MyKeyFrames[i].AnimationCurvePositionY.keys.Length > 0)
                    {
                        Data.Add("/PositionY");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurvePositionY.keys.Length; j++)
                        {
                            if (MyKeyFrames[i].AnimationCurvePositionY.keys[j].value > -MicroLimit && MyKeyFrames[i].AnimationCurvePositionY.keys[j].value < MicroLimit)
                            {
                                MyKeyFrames[i].AnimationCurvePositionY.keys[j].value = 0;
                            }
                            Data.Add(MyKeyFrames[i].AnimationCurvePositionY.keys[j].time + " " + MyKeyFrames[i].AnimationCurvePositionY.keys[j].value);
                        }
                        Data.Add("/EndPositionY");
                    }
                    if (MyKeyFrames[i].AnimationCurvePositionZ.keys.Length > 0)
                    {
                        Data.Add("/PositionZ");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurvePositionZ.keys.Length; j++)
                        {
                            if (MyKeyFrames[i].AnimationCurvePositionZ.keys[j].value > -MicroLimit && MyKeyFrames[i].AnimationCurvePositionZ.keys[j].value < MicroLimit)
                            {
                                MyKeyFrames[i].AnimationCurvePositionZ.keys[j].value = 0;
                            }
                            Data.Add(MyKeyFrames[i].AnimationCurvePositionZ.keys[j].time + " " + MyKeyFrames[i].AnimationCurvePositionZ.keys[j].value);
                        }
                        Data.Add("/EndPositionZ");
                    }

                    // Rotation
                    if (MyKeyFrames[i].AnimationCurveRotationX.keys.Length > 0)
                    {
                        Data.Add("/RotationX");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveRotationX.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveRotationX.keys[j].time + " " + MyKeyFrames[i].AnimationCurveRotationX.keys[j].value);
                        }
                        Data.Add("/EndRotationX");
                    }
                    if (MyKeyFrames[i].AnimationCurveRotationY.keys.Length > 0)
                    {
                        Data.Add("/RotationY");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveRotationY.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveRotationY.keys[j].time + " " + MyKeyFrames[i].AnimationCurveRotationY.keys[j].value);
                        }
                        Data.Add("/EndRotationY");
                    }
                    if (MyKeyFrames[i].AnimationCurveRotationZ.keys.Length > 0)
                    {
                        Data.Add("/RotationZ");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveRotationZ.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveRotationZ.keys[j].time + " " + MyKeyFrames[i].AnimationCurveRotationZ.keys[j].value);
                        }
                        Data.Add("/EndRotationZ");
                    }
                    /*if (MyKeyFrames[i].AnimationCurveRotationW.keys.Length > 0)
                    {
                        Data.Add("/RotationW");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveRotationW.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveRotationW.keys[j].time + " " + MyKeyFrames[i].AnimationCurveRotationW.keys[j].value);
                        }
                        Data.Add("/EndRotationW");
                    }*/
                    //Scale
                    if (MyKeyFrames[i].AnimationCurveScaleX.keys.Length > 0)
                    {
                        Data.Add("/ScaleX");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveScaleX.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveScaleX.keys[j].time + " " + MyKeyFrames[i].AnimationCurveScaleX.keys[j].value);
                        }
                        Data.Add("/EndScaleX");
                    }
                    if (MyKeyFrames[i].AnimationCurveScaleY.keys.Length > 0)
                    {
                        Data.Add("/ScaleY");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveScaleY.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveScaleY.keys[j].time + " " + MyKeyFrames[i].AnimationCurveScaleY.keys[j].value);
                        }
                        Data.Add("/EndScaleY");
                    }
                    if (MyKeyFrames[i].AnimationCurveScaleZ.keys.Length > 0)
                    {
                        Data.Add("/ScaleZ");
                        for (int j = 0; j < MyKeyFrames[i].AnimationCurveScaleZ.keys.Length; j++)
                        {
                            Data.Add(MyKeyFrames[i].AnimationCurveScaleZ.keys[j].time + " " + MyKeyFrames[i].AnimationCurveScaleZ.keys[j].value);
                        }
                        Data.Add("/EndScaleZ");
                    }


                    Data.Add("/KeysEnd");

                }
            }
            return Data;
        }

        /// <summary>
        /// Loads the animation from the string data
        /// </summary>
        public void RunScript(Transform MyTransform, List<string> Data)
        {
            bool IsReadKeys = false;
            Transform MyAnimatedObject = null;
            ZeltexKeyFrame MyFrame = null;
            //Debug.LogError("Loading Animation for " + Name + ":\n" + FileUtil.ConvertToSingle(Data));
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].Contains("/KeysBegin"))
                {
                    string ObjectName = ScriptUtil.RemoveCommand(Data[i]);
                    MyAnimatedObject = FindBone(MyTransform, ObjectName);
                    if (MyAnimatedObject != null)
                    {
                        IsReadKeys = true;
                        MyFrame = AddKeyFrame(MyAnimatedObject);
                    }
                    else
                    {
                        Debug.LogError("Bone [" + ObjectName + "] Not Found!");
                    }
                }
                else if (IsReadKeys)
                {
                    if (Data[i] == "/KeysEnd")
                    {
                        IsReadKeys = false;
                        MyAnimatedObject = null;
                        MyFrame = null;
                    }
                    else
                    {
                        ReadLine(Data[i], "PositionX", MyFrame.AnimationCurvePositionX);    //MyFrame, 
                        ReadLine(Data[i], "PositionY", MyFrame.AnimationCurvePositionY);
                        ReadLine(Data[i], "PositionZ", MyFrame.AnimationCurvePositionZ);
                        ReadLine(Data[i], "ScaleX", MyFrame.AnimationCurveScaleX);
                        ReadLine(Data[i], "ScaleY", MyFrame.AnimationCurveScaleY);
                        ReadLine(Data[i], "ScaleZ", MyFrame.AnimationCurveScaleZ);
                        ReadLine(Data[i], "RotationX", MyFrame.AnimationCurveRotationX);
                        ReadLine(Data[i], "RotationY", MyFrame.AnimationCurveRotationY);
                        ReadLine(Data[i], "RotationZ", MyFrame.AnimationCurveRotationZ);
                    }
                }
            }
        }

        /// <summary>
        /// Reads a line of the animation
        /// note: myframe is passed in using reference
        /// </summary>
        private void ReadLine(string MyLine, string VariableType, AnimationCurve MyAnimationCurve)
        {
            if (MyLine == "/" + VariableType)
            {
                LineType = VariableType;
            }
            else if (LineType == VariableType)
            {
                if (MyLine == "/End" + VariableType)
                {
                    LineType = "";
                }
                else
                {
                    string[] MyStrings = MyLine.Split(' ');
                    float TimeValue = float.Parse(MyStrings[0]);
                    float Value = float.Parse(MyStrings[1]);
                    MyAnimationCurve.AddKey(new Keyframe(SkeletonAnimator.FixTime(TimeValue), Value));
                }
            }
        }
        #endregion
    }
}