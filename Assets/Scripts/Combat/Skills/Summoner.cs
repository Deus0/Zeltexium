using UnityEngine;
using System.Collections;

namespace Zeltex.Combat
{
    /// <summary>
    /// 
    /// </summary>
    public class Summoner : Skill
    {
        protected new string SkillName = "Summoner";
        public string ClassName = "Minion";
        public string RaceName = "Skeleton_0";
        public GameObject SummoningAnimation;

        override public void ActivateOnNetwork()   // sheild checks enery and sends negative state on activate
        {
           //Debug.LogError ("Activating Summoning");
           if (NewState && HasEnergy())
           {
                LastTime = Time.time;
                RaycastHit MyHit;
                if (UnityEngine.Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out MyHit))
                {
                    if (MyHit.collider.gameObject.GetComponent<Zeltex.Voxels.Chunk>() || (MyHit.collider.gameObject.tag == "World"))
                    {
                        GameObject NewSpawner = Instantiate(SummoningAnimation, MyHit.point, Quaternion.identity);
                        SummoningAnimation MyAnimation = NewSpawner.GetComponent<SummoningAnimation>();
                        MyAnimation.SpawnClass(ClassName, RaceName, gameObject);
                        UseEnergy();
                    }
                }
            }
        }
    }
}