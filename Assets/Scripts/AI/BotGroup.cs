using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*		BotGroup class - added to the leader of the bots
 * 			It will command bots to move to certain positions depending on a formation
 * 
 * 		Formations:
 * 			Defending a target point/object
 * 			Moving in a singular line
 * 			Moving in a double(or n line) line
 * 			Surround a target
 * 			Flank a target (half the group hits their left side, half hits their right)
 * 
 * */
namespace Zeltex.AI
{
	public class BotGroup : MonoBehaviour
    {
		List<Bot> MyBots = new List<Bot>();

	}
}