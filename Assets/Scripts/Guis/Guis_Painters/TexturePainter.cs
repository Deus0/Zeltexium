using UnityEngine;
using System.Collections.Generic;
using ZeltexTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zeltex.Generators;

namespace Zeltex.Guis.Maker
{
    /// <summary>
    /// The type of action used when painting textures
    /// </summary>
    [System.Serializable]
    public enum TexturePaintType
    {
        None,
        Brush,
        Eraser,
        Line,
        Fill,
        Picker,
        Link
        //Select,
    }
    /// <summary>
    /// Paint the voxels
    /// The user will select a world, then edit it
    /// Upon selecting a new world, it will update the dropdown menu for its voxel types it uses\
    /// To Do:
    ///     Select Tool - including operations for them
    ///     Select Outer Edges Function - Selects all voxels on the edge, touching air
    ///     Move Tool - Move voxels in a direction using a gizmo type thing - just buttons will do for now
    ///     
    /// </summary>
    public class TexturePainter : GuiBasic
    {
        #region Variables
        private RawImage MyImage;    // use the texture selected
        [Header("References")]
        public TextureGenerator MyTextureGenerator;
        //[Header("Objects")]
        //public GameObject LineBeginObject;
        [Header("Painter")]
        public TexturePaintType BrushType;
        private bool IsSpray = true;
        private bool IsSpraying = false;
        // positioning
        private Vector2 PaintPosition;  // position mouse is at
        private Vector2 PaintWorldPosition;  // position mouse is at
        private bool IsInRect = false;
        public Material SelectedMaterial;
        public Material HighlightedMaterial;
        public Material PainterMaterial;
        public ColourPicker PrimaryColorPicker;
        public ColourPicker SecondaryColorPicker;
        private Color32 PrimaryColor = Color.red;
        private Color32 SecondaryColor = Color.white;
        private RawImage HighlightedImage;
        private bool IsTextureHit = false;
        private Vector2 PaintBrushSize;
        public bool IsLineBegin;
        private Vector2 LineBeginPosition;
        private Vector2 LineBeginWorldPosition;
        private bool IsPainting2 = false;   // Is painting currently?
        // Undo System
        private int InstructionIndex = 0;   // set to current instruction
        private int MaxInstructions = 10;
        private List<PaintInstruction> MyInstructions = new List<PaintInstruction>();   // list of instructions connected to various textures
        #endregion

        #region Mono
        public override void OnBegin()
        {
            PrimaryColorPicker.OnBegin();
            SecondaryColorPicker.OnBegin();
        }

        void Update()
        {
            if (MyImage != null && MyImage.gameObject.activeInHierarchy == false)
            {
                MyImage = null;
            }
            /*if (MyImage == null)
            {
                SetBrushTypeInternal(TexturePaintType.Link);
            }*/
            IsTextureHit = false;   // reset
            if (BrushType == TexturePaintType.Link)
            {
                RaycastViewer();
            }
            else if (IsPainting())
            {
                // Perform raycasts on gui
                RaycastImage();
                HandleInput();
            }
            AlterBrush();
            HandleHotkeys();
        }
        private void HandleHotkeys()
        {
            if (MyImage != null && IsInRect)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SetBrushTypeInternal(TexturePaintType.None);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SetBrushTypeInternal(TexturePaintType.Brush);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SetBrushTypeInternal(TexturePaintType.Picker);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    SetBrushTypeInternal(TexturePaintType.Eraser);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    SetBrushTypeInternal(TexturePaintType.Fill);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    SetBrushTypeInternal(TexturePaintType.Line);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    SetBrushTypeInternal(TexturePaintType.Link);
                }
            }
        }
        /// <summary>
        /// THe main input function
        /// </summary>
        private void HandleInput()
        {
            if (IsInRect)
            {
                bool IsMouseDown = false;
                IsMouseDown = Input.GetMouseButtonDown(0);    // only on first mouse press
                if (IsSpray && Input.GetMouseButtonDown(0))
                {
                    IsSpraying = true;
                }
                if (IsSpraying && Input.GetMouseButtonUp(0))
                {
                    IsSpraying = false;
                    IsPainting2 = false;
                }
                bool CanPaint = (IsSpray && IsSpraying) || (!IsSpray && IsMouseDown);
                // if click and ray is inside the image, do the rest
                // if click - select new raw image
                if (BrushType == TexturePaintType.Brush)
                {
                    if (CanPaint)
                    {
                        if (IsPainting2 == false)
                        {
                            IsPainting2 = true;
                            NewInstruction("Paint");
                        }
                        //Debug.Log("Painting colour in texture: " + PaintPosition.ToString());
                        Paint(PaintPosition, PrimaryColor);
                    }
                }
                else if (BrushType == TexturePaintType.Eraser)
                {
                    if (CanPaint)
                    {
                        Paint(PaintPosition, SecondaryColor);
                    }
                }
                else if (BrushType == TexturePaintType.Picker)
                {
                    if (IsMouseDown)
                    {
                        PickColor(PaintPosition);
                    }
                }
                else if (BrushType == TexturePaintType.Fill)
                {
                    if (IsMouseDown)
                    {
                        NewInstruction("Fill");
                        Fill(PaintPosition);
                    }
                }
                else if (BrushType == TexturePaintType.Line)
                {
                    if (IsMouseDown)
                    {
                        Debug.Log("Painting line in texture: " + PaintPosition.ToString());
                        NewInstruction("Line");
                        DrawLine(PaintPosition);
                    }
                }
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// Set the brush type internally
        /// </summary>
        private void SetBrushTypeInternal(TexturePaintType NewType)
        {
            GetDropdown("PaintModeDropdown").value = (int)NewType;
            SetBrushType(NewType);
        }

        private void SetBrushType(TexturePaintType NewType)
        {
            BrushType = NewType;
            IsLineBegin = false;
        }

        private void AlterBrush()
        {
            if (BrushType != TexturePaintType.None && (IsTextureHit || IsInRect))
            {
                if (BrushType == TexturePaintType.Brush)
                {
                    MouseCursor.Get().SetMouseIcon("TextureBrush");
                }
                else if (BrushType == TexturePaintType.Eraser)
                {
                    MouseCursor.Get().SetMouseIcon("TextureEraser");
                }
                else if (BrushType == TexturePaintType.Picker)
                {
                    MouseCursor.Get().SetMouseIcon("TexturePicker");
                }
                else if (BrushType == TexturePaintType.Line)
                {
                    MouseCursor.Get().SetMouseIcon("TextureLine");
                }
                else if (BrushType == TexturePaintType.Fill)
                {
                    MouseCursor.Get().SetMouseIcon("TextureFill");
                }
                else if (BrushType == TexturePaintType.Link)
                {
                    MouseCursor.Get().SetMouseIcon("TextureLink");
                }
            }
            else
            {
                MouseCursor.Get().SetMouseIcon("DefaultMouse");
            }
        }

        /// <summary>
        /// Sets the Primary Colour
        /// </summary>
        private void SetPrimaryColorInternal(Color32 NewColor)
        {
            Debug.Log("Setting new primary Color: " + NewColor.ToString());
            PrimaryColor = NewColor;
            PrimaryColorPicker.SetColor(NewColor);
        }
        /// <summary>
        /// Is the player painting
        /// </summary>
        bool IsPainting()
        {
            if (MyImage != null && MyImage.texture == null)
            {
                MyImage = null;
            }
            return (BrushType != TexturePaintType.None && MyImage != null && MyImage.gameObject.activeInHierarchy);
        }

        /// <summary>
        /// Set Texture Pixels
        /// </summary>
        public void SetPixels(Color32[] PaintColors)
        {
            Texture2D MyTexture = MyImage.texture as Texture2D;
            MyTexture.SetPixels32(PaintColors, 0);
            MyTexture.Apply(true);
            MyImage.texture = MyTexture;
        }

        /// <summary>
        /// Gets the pixels of the current texture
        /// </summary>
        public Color32[] GetPixels()
        {
            Texture2D MyTexture = MyImage.texture as Texture2D;
            return MyTexture.GetPixels32(0);
        }

        /// <summary>
        /// Paint using a size and position
        ///     - should use another texture for the brush! (16x16)
        /// </summary>
        private void Paint(Vector2 Position, Color32 PaintColor)
        {
            bool IsBlend = false;
            Vector2 Size = new Vector2(1, 1);
            Vector2 PositionMin = new Vector2(Position.x - Mathf.FloorToInt(Size.x / 2f), Position.y - Mathf.FloorToInt(Size.y / 2f));
            Vector2 PositionMax = new Vector2(Position.x + Mathf.CeilToInt(Size.x / 2f), Position.y + Mathf.CeilToInt(Size.y / 2f));
            int TextureWidth = MyImage.texture.width;
            int TextureHeight = MyImage.texture.height;
            Color32[] ColorData = GetPixels();
            for (float i = PositionMin.x; i < PositionMax.x; i++)
                for (float j = PositionMin.y; j < PositionMax.y; j++)
                {
                    if (i >= 0 && i < TextureWidth && j >= 0 && j <= TextureHeight)
                    {
                        float DistanceToMid = Vector2.Distance(new Vector2(i, j), Position);
                        if (DistanceToMid <= Size.x / 2f) // if is within bounds of texture size
                        {
                            //PaintColor.a = (byte)(DistanceToMid/(Size.x/2f));
                            //PaintColor.a = (byte)(255);
                            int Index = TextureEditor.GetPixelIndex(i, j, TextureWidth);
                            if (Index < ColorData.Length && Index >= 0)
                            {
                                Color32 NewColor = ColorData[Index];
                                if (IsBlend)
                                {
                                    NewColor.r = (byte)((NewColor.r + PaintColor.r * PaintColor.a) / 2f);
                                    NewColor.g = (byte)((NewColor.g + PaintColor.g * PaintColor.a) / 2f);
                                    NewColor.b = (byte)((NewColor.b + PaintColor.b * PaintColor.a) / 2f);
                                }
                                else
                                {
                                    NewColor = PaintColor;
                                }
                                ColorData[Index] = NewColor;
                            }
                        }
                    }
                }
            SetPixels(ColorData);
        }
        /// <summary>
        /// initializes texture on the raw image
        /// </summary>
        private void CreateTexture(Vector2 MySize)
        {
            Debug.Log("Creating new texture");
            if (MyImage != null)
            {
                MyImage.texture = new Texture2D(Mathf.FloorToInt(MySize.x), Mathf.FloorToInt(MySize.y), TextureFormat.RGBA32, false);
                MyImage.texture.filterMode = FilterMode.Point;
                MyImage.texture.wrapMode = TextureWrapMode.Clamp;
                Color32[] ColorData = GetPixels();
                for (int i = 0; i < ColorData.Length; i++)
                {
                    ColorData[i] = SecondaryColor;
                }
                SetPixels(ColorData);
            }
        }
        /// <summary>
        /// Use old texture as input to new one
        /// </summary>
        private void ResizeTexture(Vector2 MySize)
        {
            if (MyImage != null)
            {
                if (MyImage.texture != null)
                {
                    Debug.Log("Resizing texture from: " + MyImage.texture.texelSize.ToString() + ":" + MySize.ToString());
                    MyImage.texture = ResizeTexture(MySize, MyImage.texture as Texture2D);
                }
                else
                {
                    Debug.Log("Creating new texture");
                    CreateTexture(MySize);
                }
                OnResize();
            }
         }
        /// <summary>
        /// Use old texture as input to new one
        /// </summary>
        public static Texture2D ResizeTexture(Vector2 MySize, Texture2D MyTexture)
        {
            Vector2 OldSize = new Vector2(MyTexture.width, MyTexture.height);
            Color32[] OldTextureColours = MyTexture.GetPixels32(0);// GetPixels();
            MyTexture = new Texture2D(Mathf.FloorToInt(MySize.x), Mathf.FloorToInt(MySize.y), TextureFormat.RGBA32, false);
            MyTexture.filterMode = FilterMode.Point;
            MyTexture.wrapMode = TextureWrapMode.Clamp;
            Color32[] ColorData = MyTexture.GetPixels32(0);
            for (int i = 0; i < MyTexture.width; i++)
            {
                for (int j = 0; j < MyTexture.height; j++)
                {
                    int TileIndex1 = TextureEditor.GetPixelIndex(i, j, MyTexture.width);
                    // Find closest position on old texture
                    int NewPositionX = Mathf.FloorToInt(OldSize.x * (((float)i) / ((float)MyTexture.width))); // percentage through x
                    int NewPositionY = Mathf.FloorToInt(OldSize.y * (((float)j) / ((float)MyTexture.height)));
                    int TileIndex2 = TextureEditor.GetPixelIndex(
                        Mathf.Clamp(NewPositionX, 0, OldSize.x-1), 
                        Mathf.Clamp(NewPositionY, 0, OldSize.y - 1), 
                        (int)OldSize.x);
                    // set to old colour
                    //Debug.Log("TileIndex1: " + TileIndex1 + ": TileIndex2: " + TileIndex2 + "-" + ColorData.Length + ":" + OldTextureColours.Length);
                    ColorData[TileIndex1] = OldTextureColours[TileIndex2];
                }
            }
            MyTexture.SetPixels32(ColorData, 0);
            MyTexture.Apply(true);
            // as new texture is in place
            return MyTexture;
        }
        #endregion

        #region PaintActions

        /// <summary>
        /// Picks a colour
        /// </summary>
        public void PickColor(Vector2 PaintPosition)
        {
            //NewInstruction("PickColor");
            Color32[] MyColorData = GetPixels();
            int Index = TextureEditor.GetPixelIndex(PaintPosition.x, PaintPosition.y, MyImage.texture.width);
            SetPrimaryColorInternal(MyColorData[Index]);
        }

        /// <summary>
        /// Draws a line
        /// </summary>
        public void DrawLine(Vector2 PaintPosition)
        {
            if (!IsLineBegin)
            {
                IsLineBegin = true;
                LineBeginPosition = PaintPosition;
                LineBeginWorldPosition = PaintToWorldPosition(LineBeginPosition);
            }
            else
            {
                IsLineBegin = false;
                Color32[] MyColorData = GetPixels();
                Vector2 Direction = (LineBeginPosition - PaintPosition).normalized;
                for (int i = 0; i <= Vector2.Distance(PaintPosition, LineBeginPosition); i++)
                {
                    int NewIndex = TextureEditor.GetPixelIndex(Mathf.RoundToInt(PaintPosition.x + Direction.x * i),
                                                     Mathf.RoundToInt(PaintPosition.y + Direction.y * i),
                                                     MyImage.texture.width);
                    if (NewIndex >= 0 && NewIndex < MyColorData.Length)
                    {
                        MyColorData[NewIndex] = PrimaryColor;
                    }
                }
                int BeginIndex = TextureEditor.GetPixelIndex(LineBeginPosition.x, LineBeginPosition.y,MyImage.texture.width);
                int EndIndex = TextureEditor.GetPixelIndex(PaintPosition.x, PaintPosition.y, MyImage.texture.width);
                MyColorData[BeginIndex] = PrimaryColor;
                MyColorData[EndIndex] = PrimaryColor;
                SetPixels(MyColorData);
            }
        }

        /// <summary>
        /// Creates a new instruction to fill a selected pixel
        /// </summary>
        public void Fill(Vector2 PaintPosition)
        {
            NewInstruction("Fill");
            //Debug.LogError("Filling position: " + PaintPosition.ToString());
            //PaintPosition = FixPosition(PaintPosition);
            Color32[] MyColorData = GetPixels();
            int Index = TextureEditor.GetPixelIndex(PaintPosition.x, PaintPosition.y, MyImage.texture.width);
            Color32 ColorToReplace = MyColorData[Index];
            MyColorData = FloodFillPixels(MyColorData, PaintPosition, ColorToReplace, PrimaryColor);
            SetPixels(MyColorData);
        }

        /// <summary>
        /// Fills the colour
        /// </summary>
        private Color32[] FloodFillPixels(Color32[] MyColorData, Vector2 Position, Color32 ColorToReplace, Color32 NewColor)
        {
            int TextureWidth = MyImage.texture.width;
            int Index = TextureEditor.GetPixelIndex(Position.x, Position.y, TextureWidth);
            if (Index >= 0 && Index < MyColorData.Length)
            {
                if (MyColorData[Index].r == ColorToReplace.r && MyColorData[Index].g == ColorToReplace.g && MyColorData[Index].b == ColorToReplace.b)
                {
                    MyColorData[Index] = NewColor;
                    Vector2 PositionAbove = new Vector2(Position.x, Position.y + 1);
                    Vector2 PositionBelow = new Vector2(Position.x, Position.y - 1);
                    Vector2 PositionLeft = new Vector2(Position.x - 1, Position.y);
                    Vector2 PositionRight = new Vector2(Position.x + 1, Position.y);
                    int IndexAbove = TextureEditor.GetPixelIndex(PositionAbove.x, PositionAbove.y, TextureWidth);
                    int IndexBelow = TextureEditor.GetPixelIndex(PositionBelow.x, PositionBelow.y, TextureWidth);
                    int IndexLeft = TextureEditor.GetPixelIndex(PositionLeft.x, PositionLeft.y, TextureWidth);
                    int IndexRight = TextureEditor.GetPixelIndex(PositionRight.x, PositionRight.y, TextureWidth);
                    MyColorData = FloodFillPixels(MyColorData, PositionAbove, ColorToReplace, NewColor);
                    MyColorData = FloodFillPixels(MyColorData, PositionBelow, ColorToReplace, NewColor);
                    MyColorData = FloodFillPixels(MyColorData, PositionLeft, ColorToReplace, NewColor);
                    MyColorData = FloodFillPixels(MyColorData, PositionRight, ColorToReplace, NewColor);
                }
            }
            return MyColorData;
        }
        #endregion

        #region UI
        public override void UseInput(Button MyButton)
        {
            if (MyButton.name == "NoiseButton")
            {
                //MyTextureGenerator.GenerateNoise();
                MyTextureGenerator.Noise();
            }
            else if (MyButton.name == "VoroniButton")
            {
                MyTextureGenerator.Voroni();
            }
            else if (MyButton.name == "BricksButton")
            {
                MyTextureGenerator.Bricks();
            }
            else if (MyButton.name == "ClearButton")
            {
                CreateTexture(new Vector2(float.Parse(GetInput("SizeXInput").text), float.Parse(GetInput("SizeYInput").text)));
            }
            else if (MyButton.name == "ResizeButton")
            {
                ResizeTexture(new Vector2(float.Parse(GetInput("SizeXInput").text), float.Parse(GetInput("SizeYInput").text)));
            }
            else if (MyButton.name == "NoneButton")
            {
                SetBrushTypeInternal(TexturePaintType.None);
            }
            else if (MyButton.name == "PaintBrushButton")
            {
                SetBrushTypeInternal(TexturePaintType.Brush);
            }
            else if (MyButton.name == "FillButton")
            {
                SetBrushTypeInternal(TexturePaintType.Fill);
            }
            else if (MyButton.name == "LineButton")
            {
                SetBrushTypeInternal(TexturePaintType.Line);
            }
            else if (MyButton.name == "PickerButton")
            {
                SetBrushTypeInternal(TexturePaintType.Picker);
            }
            else if (MyButton.name == "LinkButton")
            {
                SetBrushTypeInternal(TexturePaintType.Link);
            }
            else if (MyButton.name == "EraserButton")
            {
                SetBrushTypeInternal(TexturePaintType.Eraser);
            }
            else if (MyButton.name == "UndoButton")
            {
                Undo();
            }
            else if (MyButton.name == "RedoButton")
            {
                Redo();
            }
            else if (MyButton.name == "TileMap")
            {
                Debug.LogError("Generating Tilemap>");
                Voxels.VoxelManager.Get().GenerateTileMap();
            }
        }
        /// <summary>
        /// Adds an instruction!
        /// </summary>
        public void NewInstruction(string InstructionType)
        {
            if (MyInstructions.Count >= InstructionIndex)
            {
                MyInstructions.RemoveRange(InstructionIndex, MyInstructions.Count - InstructionIndex);
                GetButton("RedoButton").interactable = false;
            }
            InstructionIndex++;
            PaintInstruction NewInstruction = new PaintInstruction();
            NewInstruction.MyTexture = MyImage.texture as Texture2D;
            NewInstruction.SetColors(NewInstruction.MyTexture.GetPixels32(0));
            NewInstruction.InstructionType = InstructionType;
            MyInstructions.Add(NewInstruction);
            if (MyInstructions.Count > MaxInstructions)
            {
                MyInstructions.RemoveAt(0);
            }
            GetButton("UndoButton").interactable = true;
        }
        /// <summary>
        /// Undos' previous action
        /// </summary>
        public void Undo()
        {
            if (MyInstructions.Count >= 1 && InstructionIndex >= 1)
            {
                if (MyInstructions.Count == InstructionIndex && MyInstructions[MyInstructions.Count-1].InstructionType != "Undo")
                {
                    PaintInstruction NewInstruction = new PaintInstruction();
                    NewInstruction.MyTexture = MyImage.texture as Texture2D;
                    NewInstruction.SetColors(NewInstruction.MyTexture.GetPixels32(0));
                    NewInstruction.InstructionType = "Undo";
                    MyInstructions.Add(NewInstruction);
                }
                InstructionIndex--;
                //Debug.Log("Undoing Instruction: " + InstructionIndex + " // " + MyInstructions.Count);
                // grab previous stuff
                PaintInstruction MyInstruction = MyInstructions[InstructionIndex];
                //SetPixels(MyInstruction.PreviousColors);
                //Texture2D MyTexture = MyImage.texture as Texture2D;
                MyInstruction.MyTexture.SetPixels32(MyInstruction.PreviousColors, 0);
                MyInstruction.MyTexture.Apply(true);
                if (InstructionIndex == 0)
                {
                    GetButton("UndoButton").interactable = false;
                }
                //MyImage.texture = MyTexture;
                //MyInstructions.RemoveAt(MyInstructions.Count - 1);
                GetButton("RedoButton").interactable = true;
            }
            Debug.Log("Undoing Instruction2");
        }

        public void Redo()
        {
            if (InstructionIndex + 1 < MyInstructions.Count)
            {
                InstructionIndex++;
                //Debug.Log("Redoing Instruction: " + InstructionIndex + " // " + MyInstructions.Count);
                PaintInstruction MyInstruction = MyInstructions[InstructionIndex];
                MyInstruction.MyTexture.SetPixels32(MyInstruction.PreviousColors, 0);
                MyInstruction.MyTexture.Apply(true);
                GetButton("UndoButton").interactable = true;
                if (InstructionIndex == MyInstructions.Count-1)
                {
                    GetButton("RedoButton").interactable = false;
                }
            }
        }

        public override void UseInput(Dropdown MyDropdown)
        {
            if (MyDropdown.name == "PaintModeDropdown")
            {
                SetBrushType((TexturePaintType)MyDropdown.value);
            }
        }

        public override void UseInput(Toggle MyToggle)
        {
            if (MyToggle.name == "SprayToggle")
            {
                IsSpray = MyToggle.isOn;
            }
        }
        public void UseInput(ColourPicker MyColourPicker)
        {
            if (MyColourPicker.name == "Tab2")
            {
                PrimaryColor = MyColourPicker.GetColor();
                PainterMaterial.color = PrimaryColor;
            }
            else if (MyColourPicker.name == "Tab3")
            {
                SecondaryColor = MyColourPicker.GetColor();
            }
        }
        #endregion

        #region Raycast
        /// <summary>
        /// First raycast for guis
        /// Then raycast for worlds
        /// </summary>
        private void RaycastViewer()
        {
            //Create the PointerEventData with null for the EventSystem
            PointerEventData MyPointerEvent = new PointerEventData(EventSystem.current);
            //Set required parameters, in this case, mouse position
            MyPointerEvent.position = Input.mousePosition;
            //Create list to receive all results
            List<RaycastResult> MyResults = new List<RaycastResult>();
            //Raycast it
            EventSystem.current.RaycastAll(MyPointerEvent, MyResults);
            //Debug.LogError("Raycast results: " + MyResults.Count + " at position: " + MyPointerEvent.position.ToString());
            for (int i = 0; i < MyResults.Count; i++)
            {
                if (MyResults[i].gameObject.tag == "TextureEditor" && MyResults[i].gameObject.GetComponent<RawImage>())
                {
                    IsTextureHit = true;
                    HighlightedImage = MyResults[i].gameObject.GetComponent<RawImage>();
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (HighlightedImage.texture != null)
                        {
                            MyImage = HighlightedImage;
                            OnResize();
                            SetBrushTypeInternal(TexturePaintType.Brush);
                        }
                    }
                    break;
                }
            }
            // Paint directly onto polygon!
            /*if (MyResults.Count == 0)
            {
                Ray MyRay;
                MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(MyRay.origin, MyRay.direction, out MyHit))
                {
                    Chunk MyChunk = MyHit.collider.gameObject.GetComponent<Chunk>();
                    if (MyChunk)
                    {
                        if (MyWorld != MyChunk.GetWorld())
                        {
                            //SelectWorld(MyChunk.GetWorld());
                        }
                        DidRayHit = true;
                    }
                }
            }*/
        }

        /// <summary>
        /// This should be done every time
        /// </summary>
        private void RefreshSelection()
        {
            PaintBrushSize = new Vector2(
                MyImage.GetComponent<RectTransform>().GetWidth() / MyImage.texture.width,
                 MyImage.GetComponent<RectTransform>().GetHeight() / MyImage.texture.height);
        }

        private void OnResize()
        {
            RefreshSelection();
            GetInput("SizeXInput").text = "" + MyImage.texture.width;
            GetInput("SizeYInput").text = "" + MyImage.texture.height;
        }
        // Pai
        /// <summary>
        /// Raycast the selected raw image
        /// </summary>
        public void RaycastImage()
        {
            // refresh the scale -  scale position by texture size
            Vector2 TextureSize = new Vector2(MyImage.texture.width, MyImage.texture.height);
            Vector2 RealSize = MyImage.GetComponent<RectTransform>().GetSize();
            Vector2 MyScale = new Vector2(TextureSize.x / RealSize.x, TextureSize.y / RealSize.y);
            Vector2 MyInRectPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                MyImage.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main,
                out MyInRectPosition))
            {
                //Debug.Log("Mouse Inside rect! " + MyInRectPosition.ToString() +"- scale - " + MyScale.ToString());
                PaintPosition = MyInRectPosition;
                PaintPosition.x *= MyScale.x;
                PaintPosition.y *= MyScale.y;
                //PaintWorldPosition = PaintPosition;
                PaintPosition += TextureSize / 2;
                PaintPosition = new Vector2(
                    Mathf.FloorToInt(PaintPosition.x),
                    Mathf.FloorToInt(PaintPosition.y)
                    );
                IsInRect = (PaintPosition.x >= 0 && PaintPosition.x < TextureSize.x &&
                    PaintPosition.y >= 0 && PaintPosition.y < TextureSize.y);
                PaintWorldPosition = PaintToWorldPosition(PaintPosition);
            }
            else
            {
                PaintPosition = new Vector2(-1, -1);
                //MySelectObject.SetActive(false);
                IsInRect = false;
            }
            /*RectTransformUtility.ScreenPointToWorldPointInRectangle(
                MyImage.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main,
                out PaintWorldPosition);*/
        }

        Vector2 PaintToWorldPosition(Vector2 PaintPosition)
        {
            Vector2 TextureSize = new Vector2(MyImage.texture.width, MyImage.texture.height);
            Vector2 RealSize = MyImage.GetComponent<RectTransform>().GetSize();
            /*PaintWorldPosition = new Vector2(
                Mathf.FloorToInt(PaintWorldPosition.x),
                Mathf.FloorToInt(PaintWorldPosition.y)
                );*/
            PaintPosition -= TextureSize / 2;
            PaintPosition.x /= TextureSize.x;
            PaintPosition.y /= TextureSize.y;
            PaintPosition.x *= RealSize.x;
            PaintPosition.y *= RealSize.y;
            return PaintPosition;
        }
        #endregion

        #region PainterPreview
        static float PainterLineThickness = 0.001f;
        static float LineRes = 5;
        static float ViewerLineThickness = 0.001f;

        void OnRenderObject()
        {
            if (Camera.current == Camera.main)
            {
                if (IsTextureHit && HighlightedImage != null)
                {
                    Vector3[] MyCorners = new Vector3[4];
                    HighlightedImage.GetComponent<RectTransform>().GetWorldCorners(MyCorners);
                    RenderRect(HighlightedMaterial, MyCorners, ViewerLineThickness);
                }
                if (IsPainting())
                {
                    Vector3[] MyCorners = new Vector3[4];
                    MyImage.GetComponent<RectTransform>().GetWorldCorners(MyCorners);
                    RenderRect(SelectedMaterial, MyCorners, ViewerLineThickness);
                    if (IsInRect)
                    {
                        // for 512, get 32, so its width / texture pixels = 32 of size
                        RenderRect(PaintWorldPosition, PaintBrushSize);
                    }
                    if (IsLineBegin)
                    {
                        RenderRect(LineBeginWorldPosition, PaintBrushSize);
                    }
                }
            }
        }

        void RenderRect(Vector3 MyPosition, Vector2 MySize)
        {
            Vector3[] Corners = new Vector3[4];
            Corners[0] = MyImage.GetComponent<RectTransform>().TransformPoint(MyPosition);
            Corners[1] = MyImage.GetComponent<RectTransform>().TransformPoint(MyPosition + new Vector3(MySize.x, 0, 0));
            Corners[2] = MyImage.GetComponent<RectTransform>().TransformPoint(MyPosition + new Vector3(MySize.x, MySize.y, 0));
            Corners[3] = MyImage.GetComponent<RectTransform>().TransformPoint(MyPosition + new Vector3(0, MySize.y, 0));
            RenderRect(PainterMaterial, Corners, PainterLineThickness * MySize.x / 32);
        }

        void RenderRect(Material MyMaterial, Vector3[] MyCorners, float LineThickness)
        {
            bool IsOrtho = true;
            if (IsOrtho)
            {
                for (int i = 0; i < MyCorners.Length; i++)
                {
                    MyCorners[i] = Camera.main.WorldToScreenPoint(MyCorners[i]);
                    MyCorners[i].x /= Screen.width;
                    MyCorners[i].y /= Screen.height;
                    //Debug.Log(i + " MyCorners - " + MyCorners[i].ToString());
                }
            }
            GL.PushMatrix();
            MyMaterial.SetPass(0);
            if (IsOrtho)
            {
                GL.LoadOrtho();
            }
            GL.Begin(GL.LINES);
            DrawLine(MyCorners[0], MyCorners[1], LineThickness);
            DrawLine(MyCorners[1], MyCorners[2], LineThickness);
            DrawLine(MyCorners[2], MyCorners[3], LineThickness);    // bottom
            DrawLine(MyCorners[3], MyCorners[0], LineThickness);
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawLine(Vector3 p1, Vector3 p2, float width)
        {
            float Difference = width / LineRes;
            if (Difference == 0)
            {
                Difference = 1;
            }
            Vector3 normal = Vector3.Cross(p1, p2);
            Vector3 side = Vector3.Cross(normal, p2 - p1);
            side.Normalize();
            //Debug.Log("Drawing Line-" + side.ToString());
            for (float i = -width; i <= width; i += Difference)
            {
                Vector3 o = side * i;
                GL.Vertex3((p1 + o).x, (p1 + o).y, (p1 + o).z);
                GL.Vertex3((p2 + o).x, (p2 + o).y, (p2 + o).z);
            }
        }
        #endregion
    }
}