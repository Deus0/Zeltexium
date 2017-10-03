using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Characters;
using Zeltex.AI;

namespace Zeltex.Combat 
{
    /// <summary>
    /// Commands bots to move around
    /// shows icons based on their commands
    /// </summary>
	public class BotCommander : MonoBehaviour 
	{
        #region Variables
        [SerializeField] private List<Bot> MyBots = new List<Bot>();
		private Vector3 LastHitPosition;
        private World LastHitWorld;
        #endregion

        public void Activate()
        {
           //Debug.LogError("Selecting a bot inside " + gameObject.name);
            SelectBot();
        }
        public void Activate2()
        {
           // Debug.LogError("Commanding a bot inside " + gameObject.name);
            CommandBot();
        }
        /// <summary>
        /// Get the rayhit object
        /// </summary>
        public GameObject GetRayHitObject() 
		{
			RaycastHit MyRaycast;
			if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out MyRaycast, 10)) 
			{
				LastHitPosition = MyRaycast.point;
                if (MyRaycast.collider.gameObject.GetComponent<World>())
                {
                    LastHitWorld = MyRaycast.collider.gameObject.GetComponent<World>();
                }
                else if (MyRaycast.collider.gameObject.GetComponent<Chunk>())
                {
                    LastHitWorld = MyRaycast.collider.gameObject.GetComponent<Chunk>().GetWorld();
                }
                else
                {
                    LastHitWorld = null;
                }
                return MyRaycast.collider.gameObject;
			}
			return null;
		}

        /// <summary>
        /// The main action!
        /// </summary>
		public void CommandBot() 
		{
			GameObject MyHitObject = GetRayHitObject ();
            if (MyHitObject)
            {
                for (int i = 0; i < MyBots.Count; i++)
                {
                    Bot MyBot = MyBots[i];
				    if (LastHitWorld != null) 
				    {
					    MyBot.MoveToPosition(LastHitPosition + new Vector3(0, LastHitWorld.transform.localScale.y * LastHitWorld.VoxelScale.y, 0) / 2f);
					    // instead i should use BotBrain class to tell it to do something, it will change the state
                        return;
				    }
                    else if (MyHitObject.gameObject.tag == "World")
                    {
                        MyBot.MoveToPosition(LastHitPosition + new Vector3(0, 0.4f, 0));
                        return;
                    }

                    Character MyHitCharacter = MyHitObject.GetComponent<Character>();
                    if (MyHitCharacter)
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            MyBot.FollowTarget(MyHitCharacter.gameObject);
                        }
                        else
                        {
                            if (MyBot.gameObject == MyHitCharacter.gameObject)
                            {
                                MyBot.FollowTarget(gameObject);
                            }
                            else
                            {
                                MyBot.WasHit(MyHitCharacter);
                            }

                        }
                    }

                }
            }
        }
        /// <summary>
        /// Selection of bots!
        /// </summary>
		public void SelectBot() 
		{
			GameObject MyHitObject = GetRayHitObject ();
			if (MyHitObject) 
			{
				Bot MyHitBot = MyHitObject.GetComponent<Bot> ();
				if (MyHitBot)
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        Add(MyHitBot, true);
                    }
                    else
                    {
                        Clear();
                        Add(MyHitBot, false);
                    }
				}
                else
                {
                    Clear();
                }
			}
            else
            {
                Clear();
            }
		}
        /// <summary>
        /// Add it to list
        /// Spawn an icon above its head
        /// </summary>
        /// <param name="MyBot"></param>
        private void Add(Bot MyBot, bool IsReverse)
        {
            for (int i = 0; i < MyBots.Count; i++)
            {
                if (MyBots[i] == MyBot)
                {
                    if (IsReverse)  // if reverse selection, and already selected, remove
                    {
                        Remove(i);
                    }
                    // otherwise just keep it selected!
                    return;
                }
            }
            // Enable the icon above its head
            MyBot.transform.Find("CommandedIcon").gameObject.SetActive(true);
            MyBots.Add(MyBot);
        }
        private void Remove(int Index)
        {
            // Delete the icon above its head
            MyBots[Index].transform.Find("CommandedIcon").gameObject.SetActive(false);
            MyBots.RemoveAt(Index);
        }
        public void Clear()
        {
            for (int i = MyBots.Count-1; i >= 0; i--)
            {
                Remove(i);
            }
            // Clear the icons above their heads
            MyBots.Clear();
        }
	}
}
