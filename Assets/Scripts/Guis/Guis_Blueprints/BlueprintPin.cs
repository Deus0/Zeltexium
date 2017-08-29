using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeltex.Guis.Blueprints
{
    /// <summary>
    /// Each node contains many pins, input or output
    /// Each pin can have many links, but can also be single only
    /// </summary>
    public class BlueprintPin : MonoBehaviour
    {
        public bool IsInput;   // either input or output pin
        public bool IsSingle = true;
        public List<BlueprintLink> MyLinks;       // each node contains many links
        public Button MyButton;
        public int Index;
        public BlueprintNode ParentNode;

        private void Start()
        {
            MyButton = GetComponent<Button>();
        }

        /// <summary>
        /// if a single link can only link to one thing
        /// </summary>
        public bool CanBeginLink()
        {
            if (IsSingle)
            {
                return (MyLinks.Count == 0);
            }
            else
            {
                return true;
            }
        }

        public bool CanLinkTo(BlueprintPin OtherLink)
        {
            // if other pin is already linkeds
            if (OtherLink.CanBeginLink() == false)
            {
                return false;
            }
            // if parents are the same
            if (OtherLink.ParentNode == ParentNode)
            {
                return false;
            }
            // cannot link the same type of pin to each other
            if ((IsInput && OtherLink.IsInput) || (!IsInput && !OtherLink.IsInput))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Connect(BlueprintLink MyLink)
        {
            MyLinks.Add(MyLink);
        }

        public void Disconnect(BlueprintLink MyLink)
        {
            MyLinks.Remove(MyLink);
        }

        public bool IsLinked()
        {
            return (MyLinks.Count > 0);
        }
    }
}