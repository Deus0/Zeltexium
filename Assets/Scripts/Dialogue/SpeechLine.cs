using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Zeltex.Dialogue
{
    /// <summary>
    /// A single line of speech.
    /// It contains the speaker, the speech text, the animation type, the speed of animation.
    /// To Do:
    ///     - Add variation of animation types
    ///     - Add speed of animation
    ///     - Add Colour Alterations
    ///     - Add Command system per text, similar to the mobile game
    /// </summary>
    [System.Serializable]
    public class SpeechLine
    {
        [JsonProperty]
        public string Speaker = "";
        [JsonProperty]
        public string Speech = "";

        public SpeechLine()
        {

        }

        public SpeechLine(string Speaker_, string Speech_)
        {
            Speaker = Speaker_;
            Speech = Speech_;
        }

        public string GetLabelText()
        {
            return Speaker + ": " + Speech;
        }
    }
}