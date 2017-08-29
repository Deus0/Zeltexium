using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex.Skeletons
{
    /// <summary>
    /// Contains a curve for all the variables of a transform
    /// </summary>
    [System.Serializable]
    public class ZeltexKeyFrame
    {
        public Transform MyObject;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation;
        public Vector3 OriginalScale;
        public AnimationCurve AnimationCurvePositionX;
        public AnimationCurve AnimationCurvePositionY;
        public AnimationCurve AnimationCurvePositionZ;
        public AnimationCurve AnimationCurveRotationX;
        public AnimationCurve AnimationCurveRotationY;
        public AnimationCurve AnimationCurveRotationZ;
        public AnimationCurve AnimationCurveScaleX;
        public AnimationCurve AnimationCurveScaleY;
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
            OriginalPosition = MyTransform.localPosition;
            OriginalRotation = MyTransform.localRotation;
            OriginalScale = MyTransform.localScale;
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