using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Characters;

namespace Zeltex.Combat
{
    /// <summary>
    /// Categories for spells.
    /// </summary>
    [System.Serializable]
    public enum SpellCastType
    {
        Projectile,
        Instant,
        Physical,
        Channeled,
        Bubble,
        TargetAOE
    }
    /// <summary>
    /// Spells used for pewpewing
    /// </summary>
    [System.Serializable]
    public class Spell : Element
    {
        #region Variables
        public Character MyCharacter;
        public NetworkInstanceId NetworkID;
        [Header("Base Stats")]
        // Cost Of Spell - Initial
        // Cost Of Spell - OnGoing
        public float StatTime = 0;  // 0 for just once for the cost
        public SpellCastType SpellType;
        public string CastAnimation;
        public Color32 ColorTint = Color.white;
        /*public bool IsInstantCast;
        public bool IsPhysicalHit;  // use like cone collider to check if hit other character
        public bool IsLaserBeam;    // Uses mana per second. Fires a beam of particles rather then just one.
        public bool IsSnipe;    // increase vision*/
        [Header("Projectile Properties")]  // on hit effects
        public bool IsProjectile;
        public bool FireOnHold = true;
        public float FireRate = 0.5f;
        public float Randomness = 0.0f;
        public float BulletForce = 160;
        public float LifeTime = 2f;   // length of time projectile is alive for
        public float Size = 1f;

        public string SpawnSoundName = "";    // Uses 'Spell' as default!
        public string ImplodeSoundName = "";    // Uses 'Spell' as default!
        public string ExplodeSoundName = "";    // Uses 'Spell' as default!
        public string MeshName;     // uses polygonal model or voxel model!
        [Header("Use Stat")]  // Use Stat
        public bool IsUseStat = true;
        public string StatUseName = "Mana";
        public float StatCost = 1;
        [Header("On Hit Stat")]  // on hit effects
        public bool IsAddStat = true;
        public string AddStatName = "Health";
        public float AddStatValue = -0.5f;
        public float AddStatVariance = 0.5f;
        // initial force given to the projectile
        public bool DoesDestroyOnHitCharacters = true;
        public bool DoesDestroyOnHitTerrain = false;
        public bool IsDestroyTerrainOnHit = false;
        public bool CanPassThrough = false; // passes through characters and walls
        [Header("Buff")]
        public bool IsOnHitBuff = false;
        // Burn, Slow, Fear, Knockup, Stun
        public Stat OnHitEffect = new Stat();        // dot or debuff applied on hit
        // Add on Effects
        [Header("Explosion")]   // does splash damage
        public bool IsExplosion = false;
        public float ExplosionSize = 0;
        [Header("Stick")]
        public bool IsStick = false;
        public float StickTime = 0f;
        [Header("Seeking")] // while in air, the projectile will seek out the target if it is in range
        public bool IsSeek = false;
        public float SeekDistance = 5;
        public float SeekDelay = 0.4f;    // delay before seeking begins
        [Header("Summoning")] // After hitting, a portal will open and summon a minion
        public bool IsSummon = false;
        public string MinionName = "";
        public string MinionClass = "";
        public string MinionRace = "";
        // float PortalOpenSpeed = 1f;
        [Header("Chain Lightning")] // hits one target and moves onto the next
        public bool IsChain = false;
        public float ChainDelay = 0.25f;    // delay before chain damage begins
        public float DamageReduction = 0.8f;
        [Header("Sheilding")] // when applied to a target will sheild them
        public bool IsSheilding = false;
        public float SheildDelay = 0.25f;    // delay before chain damage begins
        [Header("Portal")]
        public bool IsPortal;
        #endregion

        public Spell Clone()
        {
            Spell NewSpell = new Spell();
            NewSpell.RunScript(GetScript());
            return NewSpell;
        }

        #region ElementOverrides

/*
        {
            return DataFolderNames.Spells;
        }*/

        public override string GetScript()
        {
            //Debug.Log("Saving spell: " + Name);
            return FileUtil.ConvertToSingle(GetScriptList());
        }

        public override void RunScript(string Data)
        {
            RunScriptList(FileUtil.ConvertToList(Data));
        }
        #endregion

        #region File
        /// <summary>
        /// Gets the script for a spell
        /// </summary>
        public List<string> GetScriptList()
        {
            List<string> MyData = new List<string>();
            MyData.Add("/Spell " + Name);
            if (IsProjectile)
            {
                MyData.Add("/Projectile");
                MyData.Add("" + FireOnHold);
                MyData.Add("" + FireRate);
                MyData.Add("" + Randomness);
                MyData.Add("" + BulletForce);
                MyData.Add("" + LifeTime);
                MyData.Add("" + Size);
            }
            if (IsUseStat)
            {
                MyData.Add("/UseStat");
                MyData.Add("" + StatUseName);
                MyData.Add("" + StatCost);
            }
            if (IsAddStat)
            {
                MyData.Add("/AddStat");
                MyData.Add("" + AddStatName);
                MyData.Add("" + AddStatValue);
            }
            if (ColorTint != Color.white)
            {
                MyData.Add("/Color");
                MyData.Add("" + ColorTint.r);
                MyData.Add("" + ColorTint.g);
                MyData.Add("" + ColorTint.b);
                MyData.Add("" + ColorTint.a);
            }
            if (IsExplosion)
            {
                MyData.Add("/Explosion");
                MyData.Add("" + ExplosionSize);
            }
            if (IsStick)
            {
                MyData.Add("/Stick");
                MyData.Add("" + StickTime);
            }
            if (IsSeek)
            {
                MyData.Add("/Seek");
                MyData.Add("" + SeekDistance);
                MyData.Add("" + SeekDelay);
            }
            if (IsSummon)
            {
                MyData.Add("/Summon");
                MyData.Add(MinionName);
                MyData.Add(MinionClass);
                MyData.Add(MinionRace);
               // MyData.Add("" + PortalOpenSpeed);
            }
            if (IsChain)
            {
                MyData.Add("/ChainLightning");
                MyData.Add("" + ChainDelay);
                MyData.Add("" + DamageReduction);
            }
            if (SpawnSoundName != "")
            {
                MyData.Add("/Sound");
                MyData.Add(SpawnSoundName);
            }
            if (ImplodeSoundName != "")
            {
                MyData.Add("/ImplodeSound");
                MyData.Add(ImplodeSoundName);
            }
            if (ExplodeSoundName != "")
            {
                MyData.Add("/ExplodeSound");
                MyData.Add(ExplodeSoundName);
            }
            return MyData;
        }

        /// <summary>
        /// Runs the script for a spell
        /// </summary>
        public void RunScriptList(List<string> MyData)
        {
            //Debug.LogError(" Reading Summon [" + "/Summon" + "] with length: " + "/Summon".Length);
            for (int i = 0; i < MyData.Count; i++)
            {
                MyData[i] = ScriptUtil.RemoveWhiteSpace(MyData[i]);
                //Debug.LogError(i + " - Reading line [" + MyData[i] + "] with length: " + MyData[i].Length);
                if (MyData[i].Contains("/Spell"))
                {
                    Name = ScriptUtil.RemoveCommand(MyData[i]);
                }
                else if(MyData[i].Contains("/UseStat"))
                {
                    IsUseStat = true;
                    StatUseName = MyData[i + 1];
                    StatCost = float.Parse(MyData[i + 2]);
                    i += 2;
                }
                else if (MyData[i].Contains("/AddStat"))
                {
                    //IsProjectile = true;
                    IsAddStat = true;
                    AddStatName = MyData[i + 1];
                    AddStatValue = float.Parse(MyData[i + 2]);
                    i += 2;
                }
                else if (MyData[i].Contains("/Stick"))
                {
                    IsStick = true;
                    StickTime = float.Parse(MyData[i + 1]);
                    i += 1;
                }
                else if (MyData[i].Contains("/Explosion"))
                {
                    IsExplosion = true;
                    ExplosionSize = float.Parse(MyData[i + 1]);
                    i += 1;
                }
                else if (MyData[i].Contains("/Seek"))
                {
                    IsSeek = true;
                    SeekDistance = float.Parse(MyData[i + 1]);
                    SeekDelay = float.Parse(MyData[i + 2]);
                    i += 2;
                }
                else if (MyData[i].Contains("/Color"))
                {
                    int ColorR = int.Parse(MyData[i + 1]);
                    int ColorG = int.Parse(MyData[i + 2]);
                    int ColorB = int.Parse(MyData[i + 3]);
                    int ColorA = int.Parse(MyData[i + 4]);
                    ColorTint = new Color32(
                        (byte)(ColorR),
                        (byte)(ColorG),
                        (byte)(ColorB),
                        (byte)(ColorA));
                    i += 4;
                }
                else if (MyData[i] == "/Projectile")
                {
                    IsProjectile = true;
                    FireOnHold = bool.Parse(MyData[i + 1]);
                    FireRate = float.Parse(MyData[i + 2]);
                    Randomness = float.Parse(MyData[i + 3]);
                    BulletForce = float.Parse(MyData[i + 4]);
                    LifeTime = float.Parse(MyData[i + 5]);
                    Size = float.Parse(MyData[i + 6]);
                    i += 6;
                }
                else if (MyData[i] == "/Summon")
                {
                    IsSummon = true;
                    MinionName = MyData[i + 1];
                    MinionClass = MyData[i + 2];
                    MinionRace = MyData[i + 3];
                    //PortalOpenSpeed = float.Parse(MyData[i + 3]);
                    i += 3;
                }
                else if (MyData[i] == "/ChainLightning")
                {
                    IsChain = true;
                    ChainDelay = float.Parse(MyData[i + 1]);
                    DamageReduction = float.Parse(MyData[i + 2]);
                    i += 2;
                }
                else if (MyData[i] == "/Sound")
                {
                    SpawnSoundName = ScriptUtil.RemoveWhiteSpace(MyData[i + 1]);
                    i += 1;
                }
                else if (MyData[i] == "/ImplodeSound")
                {
                    ImplodeSoundName = ScriptUtil.RemoveWhiteSpace(MyData[i + 1]);
                    i += 1;
                }
                else if (MyData[i] == "/ExplodeSound")
                {
                    ExplodeSoundName = ScriptUtil.RemoveWhiteSpace(MyData[i + 1]);
                    i += 1;
                }
            }
        }

        #endregion

        #region Setters

        public void SetMinionName(string NewMinionName)
        {
            if (MinionName != NewMinionName)
            {
                MinionName = NewMinionName;
                OnModified();
            }
        }

        public void SetRace(string NewRaceName)
        {
            if (MinionRace != NewRaceName)
            {
                MinionRace = NewRaceName;
                OnModified();
            }
        }
        public void SetClass(string NewClassName)
        {
            if (MinionClass != NewClassName)
            {
                MinionClass = NewClassName;
                OnModified();
            }
        }

        public void SetAddStatName(string NewAddStateName)
        {
            if (AddStatName != NewAddStateName)
            {
                AddStatName = NewAddStateName;
                OnModified();
            }
        }

        public void SetSpawnSoundName(string NewSpawnSoundName)
        {
            if (SpawnSoundName != NewSpawnSoundName)
            {
                SpawnSoundName = NewSpawnSoundName;
                Debug.LogError("Setting spawn sound name: " + SpawnSoundName);
                OnModified();
            }
        }
        public void SetExplodeSoundName(string NewExplodeSoundName)
        {
            if (ExplodeSoundName != NewExplodeSoundName)
            {
                ExplodeSoundName = NewExplodeSoundName;
                OnModified();
            }
        }
        public void SetImplodeSoundName(string NewImplodeSoundName)
        {
            if (ImplodeSoundName != NewImplodeSoundName)
            {
                ImplodeSoundName = NewImplodeSoundName;
                OnModified();
            }
        }
        #endregion

        #region Getters
        public float GetSoundVolume()
        {
            return 0.5f;
        }
        public AudioClip GetSpawnSound()
        {
            return GetSound(SpawnSoundName);
        }

        public AudioClip GetImplodeSound()
        {
            return GetSound(ImplodeSoundName);
        }

        public AudioClip GetHitTerrainSound()
        {
            return GetSound(ExplodeSoundName);
        }
        public AudioClip GetHitCharacterSound()
        {
            return GetSound(ExplodeSoundName);
        }

        private AudioClip GetSound(string SoundName)
        {
            if (DataManager.Get())
            {
                AudioClip MySound = DataManager.Get().GetSound("Sounds", SoundName);
                return MySound;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }

}