using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis.Blueprints
{
    /// <summary>
    /// A basic blueprint, spawns nodes
    /// </summary>
    [ExecuteInEditMode]
    public class Blueprint : MonoBehaviour
    {
        [Header("Debug")]
        public bool IsSpawn;
        public bool IsClear;
        [Header("Data")]
        [SerializeField]
        private Material NodeMaterial;
        [SerializeField]
        private GameObject NodePrefab;
        [SerializeField]
        private List<BlueprintNode> Nodes;
        [Header("PrefabGeneration")]
        [SerializeField]
        private Vector2 DefaultNodeSize = new Vector2(160, 40);
        [Tooltip("Used for lines between inputs/outputs")]
        public Material LineMaterial;

        [SerializeField]
        private BlueprintNode SelectedNode;

        public bool IsLinking;
        public bool IsLinkingInput;
        public BlueprintNode LinkingNode;
        public BlueprintLink ConnectingLink;
        public BlueprintPin ConnectingPin;

        private void Start()
        {
            NodePrefab.SetActive(false);
        }

        private void Update()
        {
            if (IsSpawn)
            {
                IsSpawn = false;
                // spawn a gui prefab
                SpawnNode();
            }
            if (IsClear)
            {
                IsClear = false;
                ClearNodes();
            }
        }

        /// <summary>
        /// Clear all the data
        /// </summary>
        private void ClearNodes()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i])
                {
                    DestroyImmediate(Nodes[i]);
                }
            }
            Nodes.Clear();
        }

        /// <summary>
        /// Spawns a gui node
        /// </summary>
        public void SpawnNode()
        {
            GameObject NewNode;
            if (NodePrefab)
            {
                NewNode = Instantiate(NodePrefab);
            }
            else
            {
                NewNode = new GameObject();
            }
            NewNode.SetActive(true);
            NewNode.name = "Node" + Nodes.Count;
            BlueprintNode MyBlueprintNode = NewNode.GetComponent<BlueprintNode>();
            if (MyBlueprintNode == null)
            {
                MyBlueprintNode = NewNode.AddComponent<BlueprintNode>();
            }
            RectTransform NodeRect = NewNode.GetComponent<RectTransform>();
            if (NodeRect == null)
            {
                NodeRect = NewNode.AddComponent<RectTransform>();
            }
            if (NodePrefab == null)
            {
                NodeRect.sizeDelta = DefaultNodeSize;
            }
            NodeRect.SetParent(transform);
            NodeRect.anchoredPosition3D = Vector3.zero;
            NodeRect.localScale = new Vector3(1, 1, 1);
            NodeRect.localEulerAngles = Vector3.zero;
            RawImage MyImage = NewNode.GetComponent<RawImage>();
            if (MyImage == null)
            {
                MyImage = NewNode.AddComponent<RawImage>();
            }
            MyImage.material = NodeMaterial;
            // Close Buttons
            Transform CloseTransform = NewNode.transform.Find("Close");
            if (CloseTransform)
            {
                Button CloseButton = CloseTransform.gameObject.GetComponent<Button>();
                CloseButton.onClick.AddEvent(delegate { DestroyNode(MyBlueprintNode); });
            }
            else
            {
                Debug.LogWarning("NodePrefab needs a child named 'Close'");
            }
            Transform Header = NewNode.transform.Find("Header");
            if (Header)
            {
                GuiDraggable MyDragger = Header.GetComponent<GuiDraggable>();
                if (MyDragger)
                {
                    MyDragger.WindowRect = GetComponent<RectTransform>();
                    MyDragger.MyRect = NodeRect;
                    MyDragger.OnSelect.RemoveAllListeners();
                    MyDragger.OnSelect.AddEvent(delegate { SelectNode(MyBlueprintNode); });
                }
                Transform HeaderText = Header.Find("Text");
                if (HeaderText)
                {
                    HeaderText.gameObject.GetComponent<Text>().text = NameGenerator.GenerateVoxelName();
                }
            }
            else
            {
                Debug.LogWarning("NodePrefab needs a child named 'Header'");
            }

            Transform InstructionInputTransform = NewNode.transform.Find("Instruction");
            if (InstructionInputTransform)
            {
                InputField InstructionInput = InstructionInputTransform.GetComponent<InputField>();
                MyBlueprintNode.InstructionInputField = InstructionInput;
                InstructionInput.onEndEdit.AddListener(delegate { MyBlueprintNode.EditInstruction(); });
            }
            Nodes.Add(MyBlueprintNode);
            MyBlueprintNode.Initialize(this);
        }

        private void DestroyNode(BlueprintNode MyNode)
        {
            if (MyNode != null && Nodes.Contains(MyNode))
            {
                Nodes.Remove(MyNode);
                Destroy(MyNode.gameObject);
            }
        }

        private void SelectNode(BlueprintNode MyNode)
        {
            SelectedNode = MyNode;
            SelectedNode.transform.SetAsLastSibling();
        }
        
        public RawImage MyTextureEditor;
        public void Execute()
        {
            Debug.Log("Executing blueprint. Nodes: " + Nodes.Count);
            List<BlueprintNode> MyNodes = new List<BlueprintNode>();
            MyNodes.Add(SelectedNode);
            BlueprintNode CurrentNode = SelectedNode;
            //Debug.Log("Starting at: " + CurrentNode.name);
            if (SelectedNode.OutputPins.Count > 0)
            {
                while (CurrentNode != null)
                {
                    if (CurrentNode.OutputPins.Count > 0)
                    {
                        BlueprintPin OutputPin = CurrentNode.OutputPins[0]; 
                        if (OutputPin.IsLinked())
                        {
                            // get new node
                            CurrentNode = OutputPin.MyLinks[0].InputPin.ParentNode;  // found the next node in list
                            MyNodes.Add(OutputPin.MyLinks[0].InputPin.ParentNode);
                        }
                        else
                        {
                            CurrentNode = null;  // finish processing nodes
                        }
                    }
                    else
                    {
                        CurrentNode = null;
                    }
                }
            }
            else
            {
                Debug.Log("No output pins..");
            }
            Generators.TextureGenerator.Get().SetColors(Color.white, Color.black);
            for (int i = 0; i < MyNodes.Count; i++)
            {
                //Debug.Log("Processed: " + i + " " + MyNodes[i].name);
                string Instruction = MyNodes[i].Instruction;
                /*Color32 Primary = new Color32(
                    (byte)Random.Range(0, 255),
                    (byte)Random.Range(0, 255),
                    (byte)Random.Range(0, 255), 
                    255);
                Color32 Secondary = new Color(Primary.r * 0.7f, Primary.g * 0.7f, Primary.b * 0.7f, 255);
                Generators.TextureGenerator.Get().SetColors(Primary, Secondary);
                Generators.TextureGenerator.Get().RandomizeNoiseOffset();*/
                Debug.Log("Processed: " + Instruction);
                if (MyTextureEditor)
                {
                    float Percentage = 0.5f;
                    List<float> Values = new List<float>();
                    bool IsReading = false;
                    int ValuesStart = -1;
                    string Commands = "";
                    for (int j = 0; j < Instruction.Length; j++)
                    {
                        if (!IsReading && Instruction[j] == '(')
                        {
                            ValuesStart = j + 1;
                            IsReading = true;
                        }
                        else if (IsReading && Instruction[j] == ')' && ValuesStart != -1)
                        {
                            Commands = Instruction.Substring(ValuesStart, j - ValuesStart);
                            break;
                        }
                    }
                    if (Commands != "")
                    {
                        Instruction = Instruction.Substring(0, ValuesStart - 1);
                        Debug.Log("Processed input " + Instruction + ":" + Values);
                        string[] CommandsSeperated = Commands.Split(',');
                        for (int j = 0; j < CommandsSeperated.Length; j++)
                        {
                            Values.Add(float.Parse(CommandsSeperated[j]));
                        }
                    }
                    if (Instruction == "Noise")
                    {
                        Generators.TextureGenerator.Get().Noise(MyTextureEditor.texture as Texture2D);
                    }
                    else if (Instruction == "Seed")
                    {
                        if (Values.Count == 0)
                        {
                            Generators.TextureGenerator.Get().RandomizeNoiseOffset();
                        }
                        else if (Values.Count == 1)
                        {
                            Generators.TextureGenerator.Get().NoiseOffset = new Vector2(Values[0], 0);
                        }
                        else if (Values.Count == 2)
                        {
                            Generators.TextureGenerator.Get().NoiseOffset = new Vector2(Values[0], Values[1]);
                        }
                    }
                    else if (Instruction == "AddNoise")
                    {
                        if (Values.Count == 1)
                        {
                            Percentage = Values[0];
                        }
                        Generators.TextureGenerator.Get().Noise(MyTextureEditor.texture as Texture2D, Percentage);
                    }
                    else if (Instruction == "Bricks")
                    {
                        Generators.TextureGenerator.Get().Bricks(MyTextureEditor.texture as Texture2D);
                    }
                    else if (Instruction == "PrimaryColor")
                    {
                        if (Values.Count == 3)
                        {
                            Generators.TextureGenerator.Get().SetPrimaryColor(new Color(Values[0], Values[1], Values[2], 1f));
                        }
                        else if (Values.Count == 1)
                        {
                            Generators.TextureGenerator.Get().SetPrimaryColor(new Color(Values[0], Values[0], Values[0], 1f));
                        }
                    }
                    else if (Instruction == "SecondaryColor")
                    {
                        if (Values.Count == 3)
                        {
                            Generators.TextureGenerator.Get().SetSecondaryColor(new Color(Values[0], Values[1], Values[2], 1f));
                        }
                        else if (Values.Count == 1)
                        {
                            Generators.TextureGenerator.Get().SetSecondaryColor(new Color(Values[0], Values[0], Values[0], 1f));
                        }
                    }
                }
            }
        }
    }
}
