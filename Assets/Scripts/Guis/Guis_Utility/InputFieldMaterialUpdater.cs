using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis
{
    /// <summary>
    /// A quick hack for the input field
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class InputFieldMaterialUpdater : MonoBehaviour
    {
        public Material MyCaretMaterial;

        void Start()
        {
            StopAllCoroutines();
            StartCoroutine(ChangeInputMaterial());
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(ChangeInputMaterial());
        }

        IEnumerator ChangeInputMaterial()
        {
            while (gameObject.activeSelf == false)
            {
                Debug.Log(name + " is deactivated.");
                yield return null;
            }
            float TimeBegun = Time.realtimeSinceStartup;
            while (true)
            {
                if (transform.childCount == 0)
                {
                    break;
                }
                if (transform.GetChild(0).name.Contains("Caret"))
                {
                    if (transform.childCount >= 3)
                    {
                        transform.GetChild(2).gameObject.SetActive(false);
                        yield return null;
                        yield return null;
                        yield return null;
                        transform.GetChild(2).gameObject.SetActive(true);
                        yield return null;
                        yield return null;
                        yield return null;
                    }
                    Transform MyCaret = transform.GetChild(0);
                    if (MyCaret)
                    {
                        MyCaret.GetComponent<CanvasRenderer>().SetMaterial(MyCaretMaterial, Texture2D.whiteTexture);
                    }
                    break;
                }
                else if (TimeBegun - Time.realtimeSinceStartup >= 10f)
                {
                    break;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
