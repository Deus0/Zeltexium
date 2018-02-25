using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex.Skeletons
{

    [System.Serializable()]
    public class FloatDictionary : SerializableDictionaryBase<float, float>
    {
        [JsonIgnore]
        public bool IsEditing;
    }
    /// <summary>
    /// Contains a curve for all the variables of a transform
    /// </summary>
    [System.Serializable]
    public class ZeltexTransformCurve : Element
    {
        [Header("Initial")]
        [JsonProperty]
        public string TransformLocation = "";  // '' is root
        [JsonProperty, SerializeField]
        public Vector3 OriginalPosition;
        [JsonProperty, SerializeField]
        public Vector3 OriginalRotation;
        [JsonProperty, SerializeField]
        public Vector3 OriginalScale = new Vector3(1, 1, 1);

        [Header("Curve Data")]
        [JsonProperty, SerializeField]
        protected FloatDictionary PositionsX = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary PositionsY = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary PositionsZ = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary ScalesX = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary ScalesY = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary ScalesZ = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary RotationsX = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary RotationsY = new FloatDictionary();
        [JsonProperty, SerializeField]
        protected FloatDictionary RotationsZ = new FloatDictionary();

        [Header("Unity Instantiated")]
        [JsonIgnore]
        public Transform MyObject;
        [JsonIgnore, SerializeField]
        protected bool IsActive = true;
        [JsonIgnore]
        public bool CouldFindParentInHeirarchy;
        [JsonIgnore, SerializeField]
        protected AnimationCurve AnimationCurvePositionX;
        [JsonIgnore, SerializeField]
        protected AnimationCurve AnimationCurvePositionY;
        [JsonIgnore, SerializeField]
        protected AnimationCurve AnimationCurvePositionZ;
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

        public override void OnLoad()
        {
            //DataToCurves();
        }

        /// <summary>
        /// Sets whether or not it updates
        /// </summary>
        /// <param name="NewActiveState"></param>
        public void SetActive(bool NewActiveState)
        {
            IsActive = NewActiveState;
        }

        /// <summary>
        /// Sets the current frame of the curve
        /// </summary>
        /// <param name="CurrentTime"></param>
        public void SetTime(float CurrentTime)
        {
            if (MyObject != null && IsActive)
            {
                MyObject.localPosition = GetPosition(CurrentTime);
                MyObject.localEulerAngles = GetRotation(CurrentTime);
                MyObject.localScale = GetScale(CurrentTime);
            }
        }

        private void DataToCurves()
        {
            InitiateCurves();
            FillCurve(PositionsX, AnimationCurvePositionX);
            FillCurve(PositionsY, AnimationCurvePositionY);
            FillCurve(PositionsZ, AnimationCurvePositionZ);
            FillCurve(ScalesX, AnimationCurveScaleX);
            FillCurve(ScalesY, AnimationCurveScaleY);
            FillCurve(ScalesZ, AnimationCurveScaleZ);
            FillCurve(RotationsX, AnimationCurveRotationX);
            FillCurve(RotationsY, AnimationCurveRotationY);
            FillCurve(RotationsZ, AnimationCurveRotationZ);
        }

        private void FillCurve(FloatDictionary Data, AnimationCurve MyCurve)
        {
            if (Data.Count > 0)
            {
                List<float> Keys = new List<float>();
                List<float> Values = new List<float>();
                MyCurve.keys = new Keyframe[0];
                foreach (KeyValuePair<float, float> MyPair in Data)
                {
                    Keys.Add(MyPair.Key);
                    Values.Add(MyPair.Value);
                }
                for (int i = 0; i < Keys.Count; i++)
                {
                    MyCurve.AddKey(new Keyframe(Keys[i], Values[i]));
                }
            }
        }

        public void KeyCurrentFrame(string PropertyName, float CurrentTime)
        {
            if (MyObject != null)
            {
                if (PropertyName == "Position")
                {
                    SetKey("PositionX", CurrentTime, MyObject.localPosition.x);
                    SetKey("PositionY", CurrentTime, MyObject.localPosition.y);
                    SetKey("PositionZ", CurrentTime, MyObject.localPosition.z);
                }
                else if (PropertyName == "PositionX")
                {
                    SetKey("PositionX", CurrentTime, MyObject.localPosition.x);
                }
                else if (PropertyName == "PositionY")
                {
                    SetKey("PositionY", CurrentTime, MyObject.localPosition.y);
                }
                else if (PropertyName == "PositionZ")
                {
                    SetKey("PositionZ", CurrentTime, MyObject.localPosition.z);
                }
                if (PropertyName == "Scale")
                {
                    SetKey("ScaleX", CurrentTime, MyObject.localScale.x);
                    SetKey("ScaleY", CurrentTime, MyObject.localScale.y);
                    SetKey("ScaleZ", CurrentTime, MyObject.localScale.z);
                }
                else if (PropertyName == "ScaleX")
                {
                    SetKey("ScaleX", CurrentTime, MyObject.localScale.x);
                }
                else if (PropertyName == "ScaleY")
                {
                    SetKey("ScaleY", CurrentTime, MyObject.localScale.y);
                }
                else if (PropertyName == "ScaleZ")
                {
                    SetKey("ScaleZ", CurrentTime, MyObject.localScale.z);
                }
                if (PropertyName == "Rotation")
                {
                    SetKey("RotationX", CurrentTime, MyObject.localEulerAngles.x);
                    SetKey("RotationY", CurrentTime, MyObject.localEulerAngles.y);
                    SetKey("RotationZ", CurrentTime, MyObject.localEulerAngles.z);
                }
                else if (PropertyName == "RotationX")
                {
                    SetKey("RotationX", CurrentTime, MyObject.localEulerAngles.x);
                }
                else if (PropertyName == "RotationY")
                {
                    SetKey("RotationY", CurrentTime, MyObject.localEulerAngles.y);
                }
                else if (PropertyName == "RotationZ")
                {
                    SetKey("RotationZ", CurrentTime, MyObject.localEulerAngles.z);
                }
            }
        }
        
        public Vector3 GetPosition(float CurrentTime)
        {
            Vector3 NewPosition = new Vector3(OriginalPosition.x, OriginalPosition.y, OriginalPosition.z);
            if (AnimationCurvePositionX != null && AnimationCurvePositionX.keys.Length > 0)
            {
                NewPosition.x = AnimationCurvePositionX.Evaluate(CurrentTime);
            }
            if (AnimationCurvePositionY != null && AnimationCurvePositionY.keys.Length > 0)
            {
                NewPosition.y = AnimationCurvePositionY.Evaluate(CurrentTime);
            }
            if (AnimationCurvePositionZ != null && AnimationCurvePositionZ.keys.Length > 0)
            {
                NewPosition.z = AnimationCurvePositionZ.Evaluate(CurrentTime);
            }
            return NewPosition;
        }

        public Vector3 GetRotation(float CurrentTime)
        {
            Vector3 NewRotation = new Vector3(OriginalRotation.x, OriginalRotation.y, OriginalRotation.z);
            if (AnimationCurveRotationX != null && AnimationCurveRotationX.keys.Length > 0)
            {
                NewRotation.x = AnimationCurveRotationX.Evaluate(CurrentTime);
            }
            if (AnimationCurveRotationY != null && AnimationCurveRotationY.keys.Length > 0)
            {
                NewRotation.y = AnimationCurveRotationY.Evaluate(CurrentTime);
            }
            if (AnimationCurveRotationZ != null && AnimationCurveRotationZ.keys.Length > 0)
            {
                NewRotation.z = AnimationCurveRotationZ.Evaluate(CurrentTime);
            }
            return NewRotation;
        }

        public Vector3 GetScale(float CurrentTime)
        {
            Vector3 NewScale = new Vector3(OriginalScale.x, OriginalScale.y, OriginalScale.z);
            if (AnimationCurveScaleX != null && AnimationCurveScaleX.keys.Length > 0)
            {
                NewScale.x = AnimationCurveScaleX.Evaluate(CurrentTime);
            }
            if (AnimationCurveScaleY != null && AnimationCurveScaleY.keys.Length > 0)
            {
                NewScale.y = AnimationCurveScaleY.Evaluate(CurrentTime);
            }
            if (AnimationCurveScaleZ != null && AnimationCurveScaleZ.keys.Length > 0)
            {
                NewScale.z = AnimationCurveScaleZ.Evaluate(CurrentTime);
            }
            return NewScale;
        }

        public ZeltexTransformCurve()
        {
            InitiateCurves();
        }

        public ZeltexTransformCurve(Transform MyTransform)
        {
            InitiateCurves();
            SetTransform(MyTransform);
        }

        private void InitiateCurves()
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
        }
        /// <summary>
        /// Input is the string for the transform location
        /// </summary>
        public void SetTransformLocation(string NewTransformLocation)
        {
            TransformLocation = NewTransformLocation;
        }

        public void SetTransform(Transform MyTransform)
        {
            MyObject = MyTransform;
            if (MyObject)
            {
                OriginalPosition = MyObject.localPosition;
                OriginalRotation = MyObject.localEulerAngles;
                OriginalScale = MyObject.localScale;
            }
        }

        public void Restore()
        {
            if (MyObject)
            {
                MyObject.localPosition = OriginalPosition;
                MyObject.localEulerAngles = OriginalRotation;
                MyObject.localScale = OriginalScale;
            }
        }

        public void RemoveKeys(float MyTime)
        {
            RemoveKeys(PositionsX, AnimationCurvePositionX, MyTime);
            RemoveKeys(PositionsY, AnimationCurvePositionY, MyTime);
            RemoveKeys(PositionsZ, AnimationCurvePositionZ, MyTime);
            RemoveKeys(RotationsX, AnimationCurveRotationX, MyTime);
            RemoveKeys(RotationsY, AnimationCurveRotationY, MyTime);
            RemoveKeys(RotationsZ, AnimationCurveRotationZ, MyTime);
            RemoveKeys(ScalesX, AnimationCurveScaleX, MyTime);
            RemoveKeys(ScalesY, AnimationCurveScaleY, MyTime);
            RemoveKeys(ScalesZ, AnimationCurveScaleZ, MyTime);
        }

        void RemoveKeys(FloatDictionary Data, AnimationCurve MyCurve, float MyTime)
        {
            if (Data.ContainsKey(MyTime))
            {
                Data.Remove(MyTime);
            }
            for (var i = 0; i < MyCurve.keys.Length; i++)
            {
                if (MyCurve.keys[i].time == MyTime)
                {
                    MyCurve.RemoveKey(i);
                    return;
                }
            }
        }

        public AnimationCurve GetCurve(string PropertyName)
        {
            if (PropertyName == "PositionX")
            {
                return AnimationCurvePositionX;
            }
            else if (PropertyName == "PositionY")
            {
                return AnimationCurvePositionY;
            }
            else if (PropertyName == "PositionZ")
            {
                return AnimationCurvePositionZ;
            }
            else if (PropertyName == "ScaleX")
            {
                return AnimationCurveScaleX;
            }
            else if (PropertyName == "ScaleY")
            {
                return AnimationCurveScaleY;
            }
            else if (PropertyName == "ScaleZ")
            {
                return AnimationCurveScaleZ;
            }
            else if(PropertyName == "RotationX")
            {
                return AnimationCurveRotationX;
            }
            else if (PropertyName == "RotationY")
            {
                return AnimationCurveRotationY;
            }
            else if (PropertyName == "RotationZ")
            {
                return AnimationCurveRotationZ;
            }
            else
            {
                return null;
            }
        }

        public void SetKey(string PropertyName, float Time, float Value)
        {
            if (PropertyName == "PositionX")
            {
                AnimationCurvePositionX.AddKey(new Keyframe(Time, Value));
                if (!PositionsX.ContainsKey(Time))
                {
                    PositionsX.Add(Time, 0);
                }
                PositionsX[Time] = Value;
            }
            else if (PropertyName == "PositionY")
            {
                AnimationCurvePositionY.AddKey(new Keyframe(Time, Value));
                if (!PositionsY.ContainsKey(Time))
                {
                    PositionsY.Add(Time, 0);
                }
                PositionsY[Time] = Value;
            }
            else if (PropertyName == "PositionZ")
            {
                AnimationCurvePositionZ.AddKey(new Keyframe(Time, Value));
                if (!PositionsZ.ContainsKey(Time))
                {
                    PositionsZ.Add(Time, 0);
                }
                PositionsZ[Time] = Value;
            }
            if (PropertyName == "ScaleX")
            {
                AnimationCurveScaleX.AddKey(new Keyframe(Time, Value));
                if (!ScalesX.ContainsKey(Time))
                {
                    ScalesX.Add(Time, 0);
                }
                ScalesX[Time] = Value;
            }
            else if (PropertyName == "ScaleY")
            {
                AnimationCurveScaleY.AddKey(new Keyframe(Time, Value));
                if (!ScalesY.ContainsKey(Time))
                {
                    ScalesY.Add(Time, 0);
                }
                ScalesY[Time] = Value;
            }
            else if (PropertyName == "ScaleZ")
            {
                AnimationCurveScaleZ.AddKey(new Keyframe(Time, Value));
                if (!ScalesZ.ContainsKey(Time))
                {
                    ScalesZ.Add(Time, 0);
                }
                ScalesZ[Time] = Value;
            }

            if (PropertyName == "RotationX")
            {
                AnimationCurveRotationX.AddKey(new Keyframe(Time, Value));
                if (!RotationsX.ContainsKey(Time))
                {
                    RotationsX.Add(Time, 0);
                }
                RotationsX[Time] = Value;
            }
            else if (PropertyName == "RotationY")
            {
                AnimationCurveRotationY.AddKey(new Keyframe(Time, Value));
                if (!RotationsY.ContainsKey(Time))
                {
                    RotationsY.Add(Time, 0);
                }
                RotationsY[Time] = Value;
            }
            else if (PropertyName == "RotationZ")
            {
                AnimationCurveRotationZ.AddKey(new Keyframe(Time, Value));
                if (!RotationsZ.ContainsKey(Time))
                {
                    RotationsZ.Add(Time, 0);
                }
                RotationsZ[Time] = Value;
            }
        }

        public void ClearKeys(string PropertyName)
        {
            if (PropertyName == "Position")
            {
                PositionsX.Clear();
                AnimationCurvePositionX = new AnimationCurve();
                PositionsY.Clear();
                AnimationCurvePositionY = new AnimationCurve();
                PositionsZ.Clear();
                AnimationCurvePositionZ = new AnimationCurve();
            }
            else if (PropertyName == "Scale")
            {
                ScalesX.Clear();
                AnimationCurveScaleX = new AnimationCurve();
                ScalesY.Clear();
                AnimationCurveScaleY = new AnimationCurve();
                ScalesZ.Clear();
                AnimationCurveScaleZ = new AnimationCurve();
            }
            else if (PropertyName == "Rotation")
            {
                RotationsX.Clear();
                AnimationCurveRotationX = new AnimationCurve();
                RotationsY.Clear();
                AnimationCurveRotationY = new AnimationCurve();
                RotationsZ.Clear();
                AnimationCurveRotationZ = new AnimationCurve();
            }
        }

        public void Detach()
        {
            MyObject = null;
        }

        public void Attach(Transform ParentTransform)
        {
            DataToCurves();
            SetTransform(ParentTransform);
        }

        public void Spawn(Transform ParentTransform = null)
        {
            if (MyObject == null)
            {
                GameObject NewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);// new GameObject();
                NewObject.name = Name;// + "_Handle";
                NewObject.transform.SetParent(ParentTransform);
                NewObject.transform.localPosition = OriginalPosition;
                NewObject.transform.localEulerAngles = OriginalRotation;
                NewObject.transform.localScale = OriginalScale;
                SetTransform(NewObject.transform);
                // Add in test mesh
                MeshFilter MyMeshFilter = NewObject.GetComponent<MeshFilter>();
                for (int i = 0; i < MyMeshFilter.sharedMesh.vertices.Length; i++)
                {
                    MyMeshFilter.sharedMesh.vertices[i] *= 0.25f;
                }
                MyMeshFilter.sharedMesh.RecalculateBounds();
                MeshRenderer MyMeshRenderer = NewObject.GetComponent<MeshRenderer>();

                MyMeshRenderer.sharedMaterial.SetColor("_Color", 
                    new Color32( (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)255));
            }
        }

        public override void DeSpawn()
        {
            if (MyObject != null)
            {
                MyObject.gameObject.Die();
            }
        }

        public static float RandomnessScale = 100f;
        public void GenerateRandomNoise(float AnimationLength = 10f, float AnimationRandomness = 0.1f)
        {
            ClearKeys("Position");
            ClearKeys("Rotation");
            Vector3 LastRandom = OriginalPosition;
            Vector3 ThisRandom;
            Vector3 LastRandom2 = OriginalRotation;
            Vector3 ThisRandom2;
            for (float TimeIndex = 0; TimeIndex <= AnimationLength; TimeIndex += 0.1f)
            {
                if (TimeIndex == 0 || TimeIndex == AnimationLength)
                {
                    SetKey("PositionX", TimeIndex, OriginalPosition.x);
                    SetKey("PositionY", TimeIndex, OriginalPosition.y);
                    SetKey("PositionZ", TimeIndex, OriginalPosition.z);
                    SetKey("RotationX", TimeIndex, OriginalRotation.x);
                    SetKey("RotationY", TimeIndex, OriginalRotation.y);
                    SetKey("RotationZ", TimeIndex, OriginalRotation.z);
                }
                else
                {
                    ThisRandom = LastRandom + new Vector3(Random.Range(-AnimationRandomness, AnimationRandomness), Random.Range(-AnimationRandomness, AnimationRandomness), Random.Range(-AnimationRandomness, AnimationRandomness)) / RandomnessScale;
                    SetKey("PositionX", TimeIndex, ThisRandom.x);
                    SetKey("PositionY", TimeIndex, ThisRandom.y);
                    SetKey("PositionZ", TimeIndex, ThisRandom.z);
                    ThisRandom2 = LastRandom2 + new Vector3(Random.Range(-AnimationRandomness, AnimationRandomness), Random.Range(-AnimationRandomness, AnimationRandomness), Random.Range(-AnimationRandomness, AnimationRandomness)) / RandomnessScale;

                    Vector3 MagicRandom = (new Vector3(ThisRandom2.x, ThisRandom2.y, ThisRandom2.z) * 360f);
                    MagicRandom = MagicRandom.ApplyRotationMagic();
                    SetKey("RotationX", TimeIndex, MagicRandom.x);
                    SetKey("RotationY", TimeIndex, MagicRandom.y);
                    SetKey("RotationZ", TimeIndex, MagicRandom.z);
                    /*AnimationCurvePositionX.AddKey(
                        new Keyframe(TimeIndex,
                            //Mathf.Sin(TimeIndex) +
                            Random.Range(-AnimationRandomness, AnimationRandomness)));
                    AnimationCurvePositionY.AddKey(
                        new Keyframe(TimeIndex,
                            //Mathf.Sin(TimeIndex) +
                            Random.Range(-AnimationRandomness, AnimationRandomness)));
                    AnimationCurvePositionZ.AddKey(
                        new Keyframe(TimeIndex,
                            //Mathf.Sin(TimeIndex) +
                            Random.Range(-AnimationRandomness, AnimationRandomness)));*/
                }
            }
            SetKey("PositionX", AnimationLength, OriginalPosition.x);
            SetKey("PositionY", AnimationLength, OriginalPosition.y);
            SetKey("PositionZ", AnimationLength, OriginalPosition.z);
            SetKey("RotationX", AnimationLength, OriginalRotation.x);
            SetKey("RotationY", AnimationLength, OriginalRotation.y);
            SetKey("RotationZ", AnimationLength, OriginalRotation.z);
        }

    }
}