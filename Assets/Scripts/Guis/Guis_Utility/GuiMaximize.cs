using UnityEngine;
using Zeltex.Util;
using UnityEngine.UI;

namespace Zeltex.Guis
{
    /// <summary>
    /// Maximizes a gui window to the entire screen
    /// Puts to front when doing so.
    /// </summary>
    public class GuiMaximize : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private RectTransform WindowTransform;

        [SerializeField]
        private RectTransform NameInput;
        private Orbitor MyOrbitor;
        private int SizeType = 1;   // start at normal size
        [SerializeField]
        private RectTransform MyHeader;
        [SerializeField]
        private Text HeaderText;
        private GuiDraggable HeaderDraggable;
        private Vector2 OriginalSize;
        private Vector2 OriginalPosition;
        private Vector2 MaximumSize = new Vector2(1920, 1080);
        private Vector2 MinimumSize = new Vector2(1080 / 2f, 1080 / 2f);    // 1080 if header not included
        private Vector2 ViewerSize = new Vector2(1080 / 2f, 1080 / 2f);
        private float HeaderHeight;
        [Header("Viewer")]
        [SerializeField]
        private RectTransform ViewerTransform;
        [SerializeField]
        private bool IsMaskedViewer;
        [SerializeField]
        private bool IsChildViewer;
        private RectTransform ActualViewerTransform;

        private void Start()
        {
            MyOrbitor = WindowTransform.GetComponent<Orbitor>();
            OriginalSize = WindowTransform.sizeDelta;
            if (HeaderText == null && MyHeader)
            {
                HeaderText = MyHeader.transform.GetChild(0).gameObject.GetComponent<Text>();
            }
            if (MyHeader)
            {
                HeaderHeight = MyHeader.GetComponent<RectTransform>().sizeDelta.y;
                HeaderDraggable = MyHeader.GetComponent<GuiDraggable>();
            }
            else
            {
                Debug.LogWarning(name + " does not have header attached.");
            }
            MinimumSize.y += HeaderHeight;
           // SizeType = 1;   // maximum
            SizeType = 2;   // smallest
            if (IsChildViewer)
            {
                if (ViewerTransform.childCount > 0)
                {
                    ActualViewerTransform = ViewerTransform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                }
            }
            else
            {
                if (ViewerTransform)
                {
                    if (ViewerTransform.childCount > 0)
                    {
                        ActualViewerTransform = ViewerTransform.GetChild(0).GetComponent<RectTransform>();
                    }
                }
            }
            Maximize();
        }

        public void Maximize()
        {
            SizeType++;
            if (SizeType == 3)
            {
                SizeType = 0;
            }
            if (HeaderDraggable)
            {
                HeaderDraggable.enabled = (SizeType != 2);    // enable only if not maximized
            }
            // MInimum size
            if (SizeType == 0)
            {
                WindowTransform.sizeDelta = MinimumSize;
                if (ViewerTransform)
                {
                    Maker.TextureEditor MyTextureEditor = ViewerTransform.GetComponent<Maker.TextureEditor>();
                    if (MyTextureEditor)
                    {
                        MyTextureEditor.OnMaximized();
                    }
                    ViewerTransform.sizeDelta = ViewerSize;
                    if (IsChildViewer)
                    {
                        ViewerTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta = ViewerTransform.sizeDelta;
                    }
                    if (IsMaskedViewer)
                    {
                        if (ActualViewerTransform)
                        {
                            ActualViewerTransform.sizeDelta = new Vector2(1024, 1024);
                            ActualViewerTransform.gameObject.GetComponent<ObjectViewer>().ResizeRenderTexture(new Vector2(1024, 1024));
                        }
                    }
                }
                MyOrbitor.SetScreenPosition(OriginalPosition);
                if (NameInput)
                {
                    NameInput.sizeDelta = new Vector2(160, NameInput.sizeDelta.y);
                    Vector2 HalfParent = new Vector2(NameInput.transform.parent.GetComponent<RectTransform>().GetWidth() / 2f, 0);
                    NameInput.localPosition = new Vector2(NameInput.GetWidth() / 2f, 0) - HalfParent;
                }
                if (HeaderText)
                {
                    HeaderText.fontSize = 40;
                }
            }
            // Large size
            else if (SizeType == 1)
            {
                WindowTransform.sizeDelta = OriginalSize;
                if (ViewerTransform)
                {
                    ViewerTransform.sizeDelta = ViewerSize * 2 - new Vector2(HeaderHeight, HeaderHeight);   // 1,000 x 1,080
                    if (IsChildViewer)
                    {
                        ViewerTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta = ViewerTransform.sizeDelta;
                    }
                    if (IsMaskedViewer)
                    {
                        if (ActualViewerTransform)
                        {
                            ActualViewerTransform.sizeDelta = new Vector2(2048, 2048);
                            ActualViewerTransform.gameObject.GetComponent<ObjectViewer>().ResizeRenderTexture(new Vector2(2048, 2048));
                        }
                    }
                }
                MyOrbitor.SetScreenPosition(new Vector2(MyOrbitor.GetScreenPosition().x, 0));
                if (NameInput)
                {
                    NameInput.sizeDelta = new Vector2(300, NameInput.sizeDelta.y);
                    Vector2 HalfParent = new Vector2(NameInput.transform.parent.GetComponent<RectTransform>().GetWidth() / 2f, 0);
                    NameInput.localPosition = new Vector2(NameInput.GetWidth() / 2f, 0) - HalfParent;
                }   
                if (HeaderText)
                {
                    HeaderText.fontSize = 80;
                }
            }
            // full screen?
            else
            {
                WindowTransform.sizeDelta = MaximumSize;
                OriginalPosition = MyOrbitor.GetScreenPosition();
                MyOrbitor.SetScreenPosition(Vector2.zero);
                if (IsMaskedViewer)
                {
                    ViewerTransform.sizeDelta = new Vector2(1920f, ViewerTransform.sizeDelta.y);
                }
                if (IsChildViewer)
                {
                    ViewerTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta = ViewerTransform.sizeDelta;
                }
            }
            RefreshTextureEditor();
        }

        private void RefreshTextureEditor()
        {
            if (ViewerTransform)
            {
                Maker.TextureEditor MyTextureEditor = ViewerTransform.GetComponent<Maker.TextureEditor>();
                if (MyTextureEditor)
                {
                    MyTextureEditor.OnMaximized();
                }
            }
        }
    }
}