using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis.Blueprints
{
    /// <summary>
    /// Each blueprint node contains my links
    /// </summary>
    public class BlueprintNode : MonoBehaviour
    {
        private static bool IsDebugGizmos;
        public string Instruction = "";
        public Blueprint MyBlueprint;
        public List<BlueprintLink> MyLinks;       // each node contains many links
        public List<BlueprintPin> InputPins;       // each node contains many links
        public List<BlueprintPin> OutputPins;       // each node contains many links

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (IsDebugGizmos)
            {
                Gizmos.color = Color.red;
                /*for (int i = 0; i < MyLinks.Count; i++)
                {
                    if (MyLinks[i].Outputpin)
                    {
                        Gizmos.DrawLine(InputLinks[i].InputButton.transform.position, InputLinks[i].OutputButton.transform.transform.position);
                    }
                }
                Gizmos.color = Color.cyan;
                for (int i = 0; i < OutputLinks.Count; i++)
                {
                    if (OutputLinks[i].InputNode)
                    {
                        Gizmos.DrawLine(OutputLinks[i].OutputButton.transform.transform.position, OutputLinks[i].InputButton.transform.position);
                    }
                }
                if (MyBlueprint.IsLinking)
                {
                    Gizmos.color = MyBlueprint.ConnectingLink.GetColor();
                    if (MyBlueprint.IsLinkingInput)
                    {
                        Vector2 LocalMousePosition;
                        RectTransform ButtonRect = MyBlueprint.ConnectingLink.InputButton.GetComponent<RectTransform>();
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            ButtonRect,
                            Input.mousePosition,
                            Camera.main,
                            out LocalMousePosition);
                        Gizmos.DrawLine(MyBlueprint.ConnectingLink.InputButton.transform.position, ButtonRect.TransformPoint(LocalMousePosition));
                    }
                    else
                    {
                        Vector2 LocalMousePosition;
                        RectTransform ButtonRect = MyBlueprint.ConnectingLink.OutputButton.GetComponent<RectTransform>();
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            ButtonRect,
                            Input.mousePosition,
                            Camera.main,
                            out LocalMousePosition);
                        Gizmos.DrawLine(MyBlueprint.ConnectingLink.OutputButton.transform.position, ButtonRect.TransformPoint(LocalMousePosition));
                    }
                }*/
            }
        }
#endif

        /// <summary>
        /// Update the render lines for each node
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < MyLinks.Count; i++)
            {
                MyLinks[i].UpdateLinked();
            }
            if (MyBlueprint.IsLinking)
            {
                Vector2 LocalMousePosition;
                RectTransform ButtonRect = MyBlueprint.ConnectingPin.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    ButtonRect,
                    Input.mousePosition,
                    Camera.main,
                    out LocalMousePosition);
                Vector3 MousePosition = ButtonRect.TransformPoint(LocalMousePosition);
                MyBlueprint.ConnectingLink.UpdateLinking(MousePosition);
            }
        }

        /// <summary>
        ///  Use this for initialization
        /// </summary>
        public void Initialize(Blueprint NewBlueprint)
        {
            InputPins.Clear();
            OutputPins.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform Child = transform.GetChild(i);
                bool IsInput = Child.name.Contains("Input");
                bool IsOutput = Child.name.Contains("Output");
                if (IsInput || IsOutput)
                {
                    BlueprintPin MyPin = Child.GetComponent<BlueprintPin>();
                    MyPin.ParentNode = this;
                    if (IsOutput)
                    {
                        MyPin.Index = OutputPins.Count;
                        OutputPins.Add(MyPin);
                    }
                    else if (IsInput)
                    {
                        MyPin.Index = InputPins.Count;
                        InputPins.Add(MyPin);
                    }
                    Button MyButton = Child.GetComponent<Button>();
                    MyButton.onClick.RemoveAllListeners();
                    MyButton.onClick.AddEvent(
                        delegate {
                            OnClick(MyPin);
                        });
                }
            }
            MyBlueprint = NewBlueprint;
        }

        /// <summary>
        /// On Click, if not linking, create a link
        /// Otherwise connect the link to the clicked pin
        /// If clicked pin was the same, disconnect the pins
        /// </summary>
        private void OnClick(BlueprintPin MyPin)
        {
            if (MyBlueprint.IsLinking)
            {
                // if clicks on link that started to link
                if (MyPin == MyBlueprint.ConnectingPin)
                {
                    DisconnectConnectingPin(MyPin);
                }
                else if (MyBlueprint.ConnectingPin.CanLinkTo(MyPin))
                {
                    Debug.Log("Linking" + MyBlueprint.ConnectingPin.name + " to " + MyPin.name);
                    MyBlueprint.ConnectingLink.Connect(MyPin);
                    OnLinkingEnded();
                }
                // else do nothing
            }
            else
            {
                if (MyPin.CanBeginLink())
                {
                    BlueprintLink NewLink = new BlueprintLink();
                    NewLink.Initialize(MyPin, this);
                    MyBlueprint.IsLinking = true;
                    MyBlueprint.ConnectingPin = MyPin;
                    MyBlueprint.ConnectingLink = NewLink;
                    MyBlueprint.LinkingNode = this;
                    MyLinks.Add(NewLink);
                    Debug.Log("Beginning to link " + MyPin.name);
                }
                else
                {
                    Debug.Log("Cannot link " + MyPin.name);
                    DisconnectConnectingPin(MyPin);
                }
            }
        }

        private void DisconnectConnectingPin(BlueprintPin MyPin)
        {
            // DIsconnect it!
            if (MyBlueprint.ConnectingLink == null)
            {
                for (int i = 0; i < MyLinks.Count; i++)
                {
                    if (MyLinks[i].Contains(MyPin))
                    {
                        MyBlueprint.ConnectingLink = MyLinks[i];
                        break;
                    }
                }
            }
            if (MyBlueprint.ConnectingLink != null)
            {
                MyBlueprint.ConnectingLink.Disconnect();
            }
            else
            {
                Debug.LogError("Could not find link for pin: " + MyPin.name);
            }
            OnLinkingEnded();
        }

        private void OnLinkingEnded()
        {
            // on finished linking
            MyBlueprint.IsLinking = false;
            MyBlueprint.ConnectingPin = null;
            MyBlueprint.ConnectingLink = null;
            MyBlueprint.LinkingNode = null;
        }

        public InputField InstructionInputField;
        public void EditInstruction()
        {
            Debug.Log("Editing instruction in " + name);
            Instruction = InstructionInputField.text;
        }
    }
}



/*if (MyBlueprint.IsLinkingInput == true)
{
    MyBlueprint.IsLinking = false;
    // Connect up this link to the other!
    //Debug.Log("Connecting input node " + MyBlueprint.ConnectingLink.InputNode.name + " to output node - " + OutputLink.OutputNode.name);
    MyBlueprint.ConnectingLink.ConnectToOutput(OutputLink); // connect the input node to the output
}
else if (MyBlueprint.LinkingNode == this)
{
    MyBlueprint.IsLinking = false;
    Debug.LogError("Disconnecting A.");
    OutputLink.DisconnectInput();
}*/
/*private void StartLinking(BlueprintLink MyLink, bool IsLinkingInput)
{
    MyBlueprint.IsLinking = true;
    MyBlueprint.IsLinkingInput = IsLinkingInput;
    MyBlueprint.LinkingNode = this;
    Debug.Log("Started Linking from " + name + " - IsInputNode: " + IsLinkingInput);
    MyBlueprint.ConnectingLink = MyLink;
    MyLink.OnBegin(IsLinkingInput, this);
}*/
/*for (int i = 0; i < InputLinks.Count; i++)
{
    BlueprintLink InputLink = InputLinks[i];
    Button InputButton = InputLink.InputButton;
    InputButton.onClick.RemoveAllListeners();
    InputButton.onClick.AddEvent(
        delegate {
            OnClickInput(InputLink, i);
        });
}
for (int i = 0; i < OutputLinks.Count; i++)
{
    BlueprintLink OutputLink = OutputLinks[i];
    Button OutputButton = OutputLink.OutputButton;
}*/
/// <summary>
/// When the user clicks an input node
/// </summary>
/*private void OnClickInput(BlueprintLink InputLink, int MyIndex)
{
    if (MyBlueprint.IsLinking)  // second click
    {
        if (MyBlueprint.IsLinkingInput == false)
        {
            MyBlueprint.IsLinking = false;
            // Connect up this link to the other!
            Debug.Log("Connecting output node " + MyBlueprint.ConnectingLink.OutputNode.name + " to input node - " + InputLink.InputNode.name);
            MyBlueprint.ConnectingLink.ConnectToInput(InputLink);
        }
        else if (MyBlueprint.IsLinkingInput && MyBlueprint.LinkingNode == this)
        {
            MyBlueprint.IsLinking = false;
            Debug.LogError("Disconnecting B.");
            InputLink.DisconnectOutput();
        }
        else
        {
            Debug.LogError("Something weird happening..");
        }
    }
    // if not linking yet
    else
    {
        StartLinking(InputLink, true);
    }
}

private void OnClickOutput(BlueprintLink OutputLink, int MyIndex)
{
    if (MyBlueprint.IsLinking)
    {
        if (MyBlueprint.IsLinkingInput == true)
        {
            MyBlueprint.IsLinking = false;
            // Connect up this link to the other!
            Debug.Log("Connecting input node " + MyBlueprint.ConnectingLink.InputNode.name + " to output node - " + OutputLink.OutputNode.name);
            MyBlueprint.ConnectingLink.ConnectToOutput(OutputLink); // connect the input node to the output
        }
        else if (MyBlueprint.LinkingNode == this)
        {
            MyBlueprint.IsLinking = false;
            Debug.LogError("Disconnecting A.");
            OutputLink.DisconnectInput();
        }
    }
    else
    {
        StartLinking(OutputLink, false);
    }
}*/
