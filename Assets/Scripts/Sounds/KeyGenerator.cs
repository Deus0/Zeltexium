using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZeltexTools
{
    /// <summary>
    /// Simple script to generate keys
    /// </summary>
    [ExecuteInEditMode]
    public class KeyGenerator : MonoBehaviour
    {
        public bool IsGenerateKeys = false;
        public bool IsDestroyKeys = false;
        public int KeysCount = 12;
        public Vector2 RectSize = new Vector2(60, 26);
        public Vector2 Margin = new Vector2(2, 2);
        public Material MyMaterial;
        public bool IsInput = true;
        public List<AudioSource> MyAudios;
        public List<KeyCode> MyKeys;
        
        void Start()
        {
            MyKeys = new List<KeyCode>();
            MyKeys.Add(KeyCode.Alpha1);
            MyKeys.Add(KeyCode.Alpha2);
            MyKeys.Add(KeyCode.Alpha3);
            MyKeys.Add(KeyCode.Alpha4);
            MyKeys.Add(KeyCode.Alpha5);
            MyKeys.Add(KeyCode.Alpha6);
            MyKeys.Add(KeyCode.Alpha7);
            MyKeys.Add(KeyCode.Alpha8);
            MyKeys.Add(KeyCode.Alpha9);
            MyKeys.Add(KeyCode.Alpha0);
            MyKeys.Add(KeyCode.Minus);
            MyKeys.Add(KeyCode.Equals);
            MyKeys.Add(KeyCode.Backspace);
            MyAudios = new List<AudioSource>();
            for (int i = 0; i < MyKeys.Count; i++)
            {
                MyAudios.Add(transform.GetChild(i).GetComponent<AudioSource>());
            }
        }
        void Update()
        {
            if (IsGenerateKeys)
            {
                IsGenerateKeys = false;
                GenerateKeys();
            }
            if (IsDestroyKeys)
            {
                IsDestroyKeys = false;
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            if (IsInput)
            {
                for (int i = 0; i < MyKeys.Count; i++)
                {
                    if (Input.GetKey(MyKeys[i]))    //Down
                    {
                        if (!MyAudios[i].isPlaying)
                        {
                            MyAudios[i].loop = true;
                            MyAudios[i].Play();
                        }
                    }
                    if (Input.GetKeyUp(MyKeys[i]))    //Down
                    {
                        MyAudios[i].loop = false;
                    }
                }
            }
        }
        private void GenerateKeys()
        {
            for (int i = 0; i < KeysCount; i++)
            {
                GameObject NewKey = new GameObject();
                NewKey.transform.SetParent(transform);
                NewKey.transform.localScale = new Vector3(1, 1, 1);
                NewKey.name = "Key_" + (i + 1);
                RectTransform MyRectTransform = NewKey.AddComponent<RectTransform>();
                RawImage MyRawImage = NewKey.AddComponent<RawImage>();
                MyRawImage.material = MyMaterial;
                AlterAudio MyAlterAudio = NewKey.AddComponent<AlterAudio>();
                MyAlterAudio.SemitoneOffset = -12 + i * 2;
                Button MyButton = NewKey.AddComponent<Button>();
                MyButton.targetGraphic = MyRawImage;
                AudioSource MyAudioSource = NewKey.AddComponent<AudioSource>();
                MyAudioSource.playOnAwake = false;
                //MyButton.onClick.AddEvent(MyAudioSource.Play);
                /*UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    MyButton.onClick,
                    MyAudioSource.Play
                );*/
                MyRectTransform.sizeDelta = RectSize;
                MyRectTransform.anchoredPosition = new Vector2(
                    Margin.x, 
                    RectSize.y / 2f - i * RectSize.y - i * Margin.y + KeysCount * RectSize.y / 2f);
            }
        }
    }
}