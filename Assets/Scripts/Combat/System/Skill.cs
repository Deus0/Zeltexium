using UnityEngine;
using UnityEngine.Networking;
using Zeltex.Characters;

namespace Zeltex.Combat 
{
    /// <summary>
    /// Base class for all spells that use stats as their base.
    /// </summary>
	public class Skill : NetworkBehaviour
	{
        #region Variables
        //protected new string SkillName = "SkillName";
        protected Character MyCharacter;
        protected NetworkIdentity MyIdentity;
        protected bool IsToggle;	// can turn on and off while switching skills
		protected bool IsActivated = false;
        // for ZelGuiables
        //protected
        [Header("Skill Options")]
        public bool IsInput = true;

        //[Header("Spell Stats")]
        //public float StatCost = 3;
        //public string StatName = "Mana";
        //public float CoolDown = 1f;
        protected float LastTime;
        protected bool NewState;
        [Header("Data")]
        [SerializeField]
        protected Spell Data;
        #endregion

        #region Mono
        public virtual void Start()
        {
            MyCharacter = GetComponent<Character>();
            MyIdentity = GetComponent<NetworkIdentity>();
            /*if (MyCharacter.MyStats == null)
            {
                Debug.LogError(name + " has null character stats.");
                MyCharacter.MyStats = new CharacterStats();
            }*/
        }
        #endregion

        #region GettersAndSetters
        // IsInput is true when player has sheild selected
        public void ActivateInput() { IsInput = true; }
        public void DisableInput() { IsInput = false; }
        public bool IsActivate() { return IsActivated; }

        #endregion

        #region EnergyUseage
        protected bool HasEnergy()
        {
            if (Data != null && Data.StatCost == 0)
            {
                return true;
            }
            if (Data != null && MyCharacter.GetStats() != null)
            {
                Stat MyStat = MyCharacter.GetStats().GetStat(Data.StatUseName);
                if (MyStat == null)
                {
                    //Debug.LogError("Skill being use without stat.");
                    return false;
                }
                return (MyStat.GetState() >= Data.StatCost);
            }
            else
            {
                Debug.LogError(name + " has no stats." + (MyCharacter.GetStats() == null) + " or null data.." + (Data == null));
                return true;
            }
        }
        protected void UseEnergy()
        {
            if (MyCharacter.GetStats() != null)
            {
                MyCharacter.GetStats().AddStatState(Data.StatUseName, -Data.StatCost);
                CheckEnergy();
            }
        }
        private void CheckEnergy()
        {
            if (MyCharacter.GetData().MyStatsHandler.GetStatValue(Data.StatUseName) <= 0)
            {
                Deactivate();
            }
        }
        #endregion

        #region Activation
        public void Deactivate()
        {
            NewState = false;
            ActivateOnNetwork();
        }

		public void Activate() 
		{
			if (IsInput) 
			{
				IsActivated = !IsActivated;
                NewState = IsActivated;
                ActivateOnNetwork();
            }
		}

		virtual public void ActivateOnNetwork()
		{
			Debug.LogError ("Virtual");
		}
        #endregion
    }
}
