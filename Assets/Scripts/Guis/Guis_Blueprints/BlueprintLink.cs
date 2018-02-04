using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis.Blueprints
{
    /// <summary>
    /// links between blueprint nodes
    /// Links are shared between two different nodes
    /// Each link contains a render line !
    /// </summary>
    [System.Serializable]
    public class BlueprintLink
    {
        private BlueprintNode ParentNode;   // the node that the link belongs too
        public BlueprintPin InputPin;
        public BlueprintPin OutputPin;
        public LineRenderer MyLineRenderer;
        private Color MyColor;
        private Color DefaultColor;
        private bool IsConnectingInput; // while connecting, this info is used

        public Color GetColor()
        {
            return MyColor;
        }

        /// <summary>
        /// Initializes the data
        /// </summary>
        public void Initialize(BlueprintPin MyPin, BlueprintNode NewParent)
        {
            ParentNode = NewParent;
            MyPin.Connect(this);
            IsConnectingInput = MyPin.IsInput;
            if (MyPin.IsInput)
            {
                InputPin = MyPin;
            }
            else
            {
                OutputPin = MyPin;
            }
            SetRandomColors();
            MyLineRenderer = CreateLineRenderer(MyPin.IsInput, MyLineRenderer);
            DefaultColor = MyPin.MyButton.colors.normalColor;
            if (IsConnectingInput)
            {
                SetLinkingButtonColor(InputPin.MyButton, MyColor);
            }
            else
            {
                SetLinkingButtonColor(OutputPin.MyButton, MyColor);
            }
        }

        public bool Contains(BlueprintPin MyPin)
        {
            if (MyPin == null)
            {
                return false;
            }
            else
            {
                return (MyPin == InputPin || MyPin == OutputPin);
            }
        }

        public void Connect(BlueprintPin MyPin)
        {
            if (IsConnectingInput)
            {
                OutputPin = MyPin;
            }
            else
            {
                InputPin = MyPin;
            }
            MyPin.ParentNode.MyLinks.Add(this);   // add link to both blueprint nodes
            SetLinkingButtonColor(MyPin.MyButton, MyColor);
            MyPin.Connect(this);
        }

        private LineRenderer CreateLineRenderer(bool IsLinkingInput, LineRenderer thisLineRenderer)
        {
            if (thisLineRenderer == null)
            {
                GameObject NewLineObject = new GameObject();
                NewLineObject.name = "NewLink";
                if (IsLinkingInput)
                {
                    NewLineObject.transform.SetParent(InputPin.transform);
                }
                else
                {
                    NewLineObject.transform.SetParent(OutputPin.transform);
                }
                thisLineRenderer = NewLineObject.AddComponent<LineRenderer>();
                thisLineRenderer.material = ParentNode.MyBlueprint.LineMaterial; //new Material(Shader.Find("Particles/Additive"));
                thisLineRenderer.startColor = MyColor;
                thisLineRenderer.endColor = MyColor;
                thisLineRenderer.widthMultiplier = 0.01f;
                thisLineRenderer.positionCount = 2;
                // set vertex count to 0
                return thisLineRenderer;
            }
            return thisLineRenderer;
        }

        private void DestroyRenderer(LineRenderer thisLine)
        {
            if (thisLine)
            {
                thisLine.gameObject.Die();
                thisLine = null;
            }
            else
            {
                Debug.LogError("Cannot destroy, already destroyed.");
            }
        }

        /// <summary>
        /// Set the line to reposition every frame
        /// </summary>
        public void UpdateLinked()
        {
            if (InputPin && OutputPin)
            {
                Vector3 ForwardVector = OutputPin.transform.forward;
                Vector3[] LinePoints = new Vector3[2];
                LinePoints[0] = InputPin.transform.position;
                LinePoints[1] = OutputPin.transform.position;
                for (int i = 0; i < LinePoints.Length; i++)
                {
                    LinePoints[i] -= ForwardVector * 0.001f;
                }
                MyLineRenderer.SetPositions(LinePoints);
            }
        }

        /// <summary>
        /// Renders a line from the pin to the mouse position
        /// </summary>
        /// <param name="MousePosition"></param>
        public void UpdateLinking(Vector3 MousePosition)
        {
            if (InputPin == null || OutputPin == null)
            {
                Vector3 ForwardVector;
                Vector3[] LinePoints = new Vector3[2];
                if (IsConnectingInput)
                {
                    LinePoints[0] = InputPin.transform.position;
                    ForwardVector = InputPin.transform.forward;
                }
                else
                {
                    LinePoints[0] = OutputPin.transform.position;
                    ForwardVector = OutputPin.transform.forward;
                }
                LinePoints[1] = MousePosition;
                for (int i = 0; i < LinePoints.Length; i++)
                {
                    LinePoints[i] -= ForwardVector * 0.001f;
                }
                MyLineRenderer.SetPositions(LinePoints);
            }
        }

        /// <summary>
        /// Clears the line renderer and resets colours
        /// </summary>
        public void Disconnect()
        {
            if (InputPin)
            {
                SetLinkingButtonColor(InputPin.MyButton, DefaultColor);
                InputPin.ParentNode.MyLinks.Remove(this);
                InputPin.Disconnect(this);
            }
            if (OutputPin)
            {
                SetLinkingButtonColor(OutputPin.MyButton, DefaultColor);
                OutputPin.ParentNode.MyLinks.Remove(this);
                OutputPin.Disconnect(this);
            }
            DestroyRenderer(MyLineRenderer);
        }

        private void SetLinkingButtonColor(Button MyButton, Color NewColor)
        {
            if (MyButton)
            {
                ColorBlock MyColorBlock = MyButton.colors;
                MyColorBlock.normalColor = NewColor;
                MyButton.colors = MyColorBlock;
            }
        }

        private void SetRandomColors()
        {
            int MyColorRandom = Random.Range(1, 10);
            if (MyColorRandom == 1)
            {
                MyColor = Color.red;
            }
            else if (MyColorRandom == 2)
            {
                MyColor = Color.blue;
            }
            else if (MyColorRandom == 3)
            {
                MyColor = Color.cyan;
            }
            else if (MyColorRandom == 4)
            {
                MyColor = Color.green;
            }
            else if (MyColorRandom == 5)
            {
                MyColor = Color.grey;
            }
            else if (MyColorRandom == 6)
            {
                MyColor = Color.yellow;
            }
            else if (MyColorRandom == 7)
            {
                MyColor = Color.magenta;
            }
            else if (MyColorRandom == 8)
            {
                MyColor = new Color(255, 0, 255);   // purple
            }
            else if (MyColorRandom == 9)
            {
                MyColor = Color.black;
            }
            else
            {
                MyColor = Color.white;
            }
        }
    }

}

/// <summary>
/// Connects the Inputlink as this links output
/// </summary>
/*public void ConnectToOutput(BlueprintLink OtherInputLink)
{
    if (IsConnected == false)
    {
        if (OtherInputLink != null)
        {
            InputLink = this;
            OutputLink = OtherInputLink;
            OutputNode = OutputLink.OutputNode;
            OutputButton = OutputLink.OutputButton;
            OutputLineRenderer = OutputLink.InputLineRenderer;
            Debug.Log("Connected Input: " + InputNode.name + " to Output: " + OutputNode.name);
            OnConnected();
        }
        else
        {
            Debug.LogError("Trying to connect to null Input");
        }

    }
}

/// <summary>
/// Connects output node up as this links input
/// </summary>
public void ConnectToInput(BlueprintLink OtherOutputLink)
{
    if (IsConnected == false)
    {
        if (OtherOutputLink != null)
        {
            OutputLink = this;
            InputLink = OtherOutputLink;
            InputNode = InputLink.InputNode;
            InputButton = InputLink.InputButton;
            InputLineRenderer = InputLink.OutputLineRenderer;
            Debug.Log("Connected Output: " + OutputNode.name + " to Input: " + InputNode.name);
            OnConnected();
        }
        else
        {
            Debug.LogError("Trying to connect to null Output");
        }
    }
}*/
