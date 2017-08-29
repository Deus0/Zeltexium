using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis
{
    /// <summary>
    /// Handles transform input
    /// </summary>
    public class TransformHandler : MonoBehaviour
    {
        private Transform MyTransform;
        private bool IsLocal;
        [Header("UI")]
        public InputField Name;
        public InputField PositionX, PositionY, PositionZ;
        public InputField RotationX, RotationY, RotationZ;
        public InputField ScaleX, ScaleY, ScaleZ;

        private void SetInteractable(bool NewState)
        {
            Name.interactable = NewState;
            PositionX.interactable = NewState;
            PositionY.interactable = NewState;
            PositionZ.interactable = NewState;
            RotationX.interactable = NewState;
            RotationY.interactable = NewState;
            RotationZ.interactable = NewState;
            ScaleX.interactable = NewState;
            ScaleY.interactable = NewState;
            ScaleZ.interactable = NewState;
        }
        private void ReleaseTransform()
        {
            if (MyTransform)
            {
                Name.text = "";
                PositionX.text = "";
                PositionY.text = "";
                PositionZ.text = "";
                RotationX.text = "";
                RotationY.text = "";
                RotationZ.text = "";
                ScaleX.text = "";
                ScaleY.text = "";
                ScaleZ.text = "";
                SetInteractable(false);
                MyTransform = null;
            }
        }

        private void SelectedTransform()
        {
            if (MyTransform)
            {
                PositionX.text = MyTransform.transform.localPosition.x + "";
                PositionY.text = MyTransform.transform.localPosition.y + "";
                PositionZ.text = MyTransform.transform.localPosition.z + "";
                RotationX.text = MyTransform.transform.localEulerAngles.x + "";
                RotationY.text = MyTransform.transform.localEulerAngles.y + "";
                RotationZ.text = MyTransform.transform.localEulerAngles.z + "";
                ScaleX.text = MyTransform.transform.localScale.x + "";
                ScaleY.text = MyTransform.transform.localScale.y + "";
                ScaleZ.text = MyTransform.transform.localScale.z + "";
                SetInteractable(true);
            }
        }

        public void Set(Transform NewTransform)
        {
            if (NewTransform != MyTransform)
            {
                ReleaseTransform();
                MyTransform = NewTransform;
                SelectedTransform();
            }
        }

        public void UseInput(InputField MyInput)
        {
            if (MyInput == PositionX)
            {
                Vector3 MyPosition = MyTransform.transform.localPosition;
                MyPosition.x = float.Parse(PositionX.text);
                MyTransform.transform.localPosition = MyPosition;
            }
            else if (MyInput == PositionY)
            {
                Vector3 MyPosition = MyTransform.transform.localPosition;
                MyPosition.y = float.Parse(PositionY.text);
                MyTransform.transform.localPosition = MyPosition;
            }
            else if (MyInput == PositionZ)
            {
                Vector3 MyPosition = MyTransform.transform.localPosition;
                MyPosition.z = float.Parse(PositionZ.text);
                MyTransform.transform.localPosition = MyPosition;
            }

            else if (MyInput == ScaleX)
            {
                Vector3 MyPosition = MyTransform.transform.localScale;
                MyPosition.x = float.Parse(ScaleX.text);
                MyTransform.transform.localScale = MyPosition;
            }
            else if (MyInput == ScaleY)
            {
                Vector3 MyPosition = MyTransform.transform.localScale;
                MyPosition.y = float.Parse(ScaleY.text);
                MyTransform.transform.localScale = MyPosition;
            }
            else if (MyInput == ScaleZ)
            {
                Vector3 MyPosition = MyTransform.transform.localScale;
                MyPosition.z = float.Parse(ScaleZ.text);
                MyTransform.transform.localScale = MyPosition;
            }


            else if (MyInput == RotationX)
            {
                Vector3 MyPosition = MyTransform.transform.eulerAngles;
                MyPosition.x = float.Parse(RotationX.text);
                MyTransform.transform.eulerAngles = MyPosition;
            }
            else if (MyInput == RotationY)
            {
                Vector3 MyPosition = MyTransform.transform.eulerAngles;
                MyPosition.y = float.Parse(RotationY.text);
                MyTransform.transform.eulerAngles = MyPosition;
            }
            else if (MyInput == RotationZ)
            {
                Vector3 MyPosition = MyTransform.transform.eulerAngles;
                MyPosition.z = float.Parse(RotationZ.text);
                MyTransform.transform.eulerAngles = MyPosition;
            }

            else if (MyInput == Name)
            {
                MyTransform.name = MyInput.name;
            }
        }
    }

}