using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Combat;


namespace Zeltex.Characters
{
    /// <summary>
    /// Type of test we are doing
    /// </summary>
    public enum TestCharacterType
    {
        LevelUp,
        Explode,
        Respawn
    }
    /// <summary>
    /// Testing Character class
    /// </summary>
    [ExecuteInEditMode]
    public class TestCharacter : MonoBehaviour
    {
        public bool IsTest;
        public TestCharacterType MyType;
        public KeyCode MyTestKey = KeyCode.Period;
        public Character MyCharacter;

        void Start()
        {
            if (MyCharacter == null)
            {
                MyCharacter = gameObject.GetComponent<Character>();
            }
        }

        // Update is called once per frame
        void Update ()
        {
		    if (Input.GetKeyDown(MyTestKey) || IsTest)
            {
                IsTest = false;
                if (MyType == TestCharacterType.LevelUp)
                {
                    Stat MyStat = MyCharacter.GetComponent<CharacterStats>().GetStat("Experience");
                    float ExperienceNeeded = MyStat.GetMaxState();
                    //Debug.LogError("Adding Experience: " + ExperienceNeeded + ":" + MyStat.GetState() + ":" + MyStat.StatType + ":" + MyStat.Description);
                    MyCharacter.GetComponent<CharacterStats>().AddExperience(ExperienceNeeded);
                }
            }
	    }
    }

}
