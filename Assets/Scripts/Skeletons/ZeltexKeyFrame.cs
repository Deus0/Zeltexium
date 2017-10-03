using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex.Skeletons
{
    /// <summary>
    /// Contains a curve for all the variables of a transform
    /// </summary>
    [System.Serializable]
    public class ZeltexKeyFrame
    {
        [JsonProperty]
        public Vector3 OriginalPosition;
        [JsonProperty]
        public Quaternion OriginalRotation;
        [JsonProperty]
        public Vector3 OriginalScale = new Vector3(1,1,1);
        [Header("Unity Instantiated")]
        [JsonIgnore]
        public Transform MyObject;
        [JsonIgnore]
        public AnimationCurve AnimationCurvePositionX;
        [JsonIgnore]
        public AnimationCurve AnimationCurvePositionY;
        [JsonIgnore]
        public AnimationCurve AnimationCurvePositionZ;
        [JsonIgnore]
        public AnimationCurve AnimationCurveRotationX;
        [JsonIgnore]
        public AnimationCurve AnimationCurveRotationY;
        [JsonIgnore]
        public AnimationCurve AnimationCurveRotationZ;
        [JsonIgnore]
        public AnimationCurve AnimationCurveScaleX;
        [JsonIgnore]
        public AnimationCurve AnimationCurveScaleY;
        [JsonIgnore]
        public AnimationCurve AnimationCurveScaleZ;

        public ZeltexKeyFrame(Transform MyTransform)
        {
            AnimationCurvePositionX = new AnimationCurve();
            AnimationCurvePositionY = new AnimationCurve();
            AnimationCurvePositionZ = new AnimationCurve();
            AnimationCurveRotationX = new AnimationCurve();
            AnimationCurveRotationY = new AnimationCurve();
            AnimationCurveRotationZ = new AnimationCurve();
            AnimationCurveScaleX = new AnimationCurve();
            AnimationCurveScaleY = new AnimationCurve();
            AnimationCurveScaleZ = new AnimationCurve();
            MyObject = MyTransform;
            if (MyTransform)
            {
                OriginalPosition = MyTransform.localPosition;
                OriginalRotation = MyTransform.localRotation;
                OriginalScale = MyTransform.localScale;
            }
            AnimationCurveScaleX.AddKey(new Keyframe(0, OriginalScale.x));
            AnimationCurveScaleY.AddKey(new Keyframe(0, OriginalScale.y));
            AnimationCurveScaleZ.AddKey(new Keyframe(0, OriginalScale.z));
        }
        public void Restore()
        {
            MyObject.localPosition = OriginalPosition;
            MyObject.localRotation = OriginalRotation;
        }
        public void RemoveKeys(float MyTime)
        {
            RemoveKeys(AnimationCurvePositionX, MyTime);
            RemoveKeys(AnimationCurvePositionY, MyTime);
            RemoveKeys(AnimationCurvePositionZ, MyTime);
            RemoveKeys(AnimationCurveRotationX, MyTime);
            RemoveKeys(AnimationCurveRotationY, MyTime);
            RemoveKeys(AnimationCurveRotationZ, MyTime);
            RemoveKeys(AnimationCurveScaleX, MyTime);
            RemoveKeys(AnimationCurveScaleY, MyTime);
            RemoveKeys(AnimationCurveScaleZ, MyTime);
        }
        void RemoveKeys(AnimationCurve MyCurve, float MyTime)
        {
            for (var i = 0; i < MyCurve.keys.Length; i++)
            {
                if (MyCurve.keys[i].time == MyTime)
                {
                    MyCurve.RemoveKey(i);
                    return;
                }
            }
        }
    }
}