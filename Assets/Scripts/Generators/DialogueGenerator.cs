using UnityEngine;
using System.Collections;
using MakerGuiSystem;

namespace Zeltex.Dialogue
{
    /// <summary>
    /// Generate random like dialogue
    /// </summary>
    public class DialogueGenerator : MonoBehaviour
	{

		public IEnumerator GenerateData()
        {
            //MyDialogueMaker.MyNames.Add("NewDialogue " + MyDialogueMaker.MyNames.Count);
            string MyScript1 =
@"
/Dialogue
/id Beginning
/Character Goood Evening Sir.
/Player Greetings.
/Character I feel a dark and mysterious force coming from you.
/default Line 1
/id Line 1
/Character But enough of that son. I have other matters to attend to. What is your favourite colour?
/Player [1] Red
[2] Blue
/default Line 2
/options Line 2,Line 3
/id Line 2
/Character Well, I don't think we can get along now.
/default Exit
/id Line 3
/Character Me too!! I will give you this torch. Be wary of the darkness.
/Character Alot of town folk being dissapearing, ya hear?
/default Exit
/id Exit
/Character You should head further north to the town of Gales. You will find what you seek there.
/default End
/EndDialogue
";
			//MyDialogueMaker.MyNames.Add("NewDialogue " + MyDialogueMaker.MyNames.Count);
			string MyScript2 =
@"
/Dialogue
/id 1
/Character Good day sir.
/id 2
/Player What?.
/id 3
/Character Nothing sir.
/id 4
/Player Very well then.
/EndDialogue
";
			//MyDialogueMaker.MyNames.Add("NewDialogue " + MyDialogueMaker.MyNames.Count);

			string MyScript3 =
@"/Dialogue
/id Beginning
/Character Hello There Pal.
/Player ....So?
/Character DIEEE MORTAL!
/default Line 1
/id Line 1
/Character So How many People have you exploded in your life?
/Player [1] Ummm... 	[2] ALOT, DIE VERMINS!
/default Line 2
/options Line 2,Line 3
/id Line 2
/Character Well now, thats just rude..
/default Exit
/id Line 3
/Character Quite an impressive feat!
/default Exit
/id Exit
/Character Now now, shoe away sonny jimmy ma boi.
/default End
/EndDialogue
";

			//MyDialogueMaker.MyNames.Add("NewDialogue " + MyDialogueMaker.MyNames.Count);
			string MyScript4 =
@"/Dialogue
/id Beginning
/Character This be here, the son of the shepard..
/Player ....So?
/Character DIEEE MORTAL!
/default Line 1
/id Line 1
/Character So How many People have you exploded in your life?
/Player [1] Ummm... 	[2] ALOT, DIE VERMINS!
/default Line 2
/options Line 2,Line 3
/id Line 2
/Character Well now, thats just rude..
/default Exit
/id Line 3
/Character Quite an impressive feat!
/default Exit
/id Exit
/Character Now now, shoe away sonny jimmy ma boi.
/default End
/EndDialogue
";
			DataManager.Get().Add("Dialogues", "Dialogue1", MyScript1);
			DataManager.Get().Add("Dialogues", "Dialogue2", MyScript2);
			DataManager.Get().Add("Dialogues", "Dialogue3", MyScript3);
			DataManager.Get().Add("Dialogues", "Dialogue4", MyScript4);
			yield break;
        }
    }
}