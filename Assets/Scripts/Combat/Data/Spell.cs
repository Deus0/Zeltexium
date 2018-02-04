using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Zeltex.Util;
using Zeltex.Characters;
using Newtonsoft.Json;

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
        [Header("Base Stats")]
        // Cost Of Spell - Initial
        // Cost Of Spell - OnGoing
        [JsonProperty]
        public float StatTime = 0;  // 0 for just once for the cost
        [JsonProperty]
        public SpellCastType SpellType;
        /*public bool IsInstantCast;
        public bool IsPhysicalHit;  // use like cone collider to check if hit other character
        public bool IsLaserBeam;    // Uses mana per second. Fires a beam of particles rather then just one.
        public bool IsSnipe;    // increase vision*/
        [Header("Projectile Properties")]  // on hit effects
        [JsonProperty]
        public bool IsProjectile;
        [JsonProperty]
        public bool FireOnHold = true;
        [JsonProperty]
        public float FireRate = 0.5f;
        [JsonProperty]
        public float Randomness = 0.0f;
        [JsonProperty]
        public float BulletForce = 160;
        [JsonProperty]
        public float LifeTime = 2f;   // length of time projectile is alive for
        [JsonProperty]
        public float Size = 1f;

        [JsonProperty]
        public string SpawnSoundName = "";    // Uses 'Spell' as default!
        [JsonProperty]
        public string ImplodeSoundName = "";    // Uses 'Spell' as default!
        [JsonProperty]
        public string ExplodeSoundName = "";    // Uses 'Spell' as default!
        //public string MeshName;     // uses polygonal model or voxel model!
        [Header("Use Stat")]  // Use Stat
        [JsonProperty]
        public bool IsUseStat = true;
        [JsonProperty]
        public string StatUseName = "Mana";
        [JsonProperty]
        public float StatCost = 1;
        [Header("On Hit Stat")]  // on hit effects
        [JsonProperty]
        public bool IsAddStat = true;
        [JsonProperty]
        public string AddStatName = "Health";
        [JsonProperty]
        public float AddStatValue = -0.5f;
        [JsonProperty]
        public float AddStatVariance = 0.5f;
        // initial force given to the projectile
        [JsonProperty]
        public bool DoesDestroyOnHitCharacters = true;
        public bool DoesDestroyOnHitTerrain = false;
        [JsonProperty]
        public bool IsDestroyTerrainOnHit = false;
        [JsonProperty]
        public bool CanPassThrough = false; // passes through characters and walls
        [Header("Buff")]
        [JsonProperty]
        public bool IsOnHitBuff = false;
        // Burn, Slow, Fear, Knockup, Stun
        [JsonProperty]
        public Stat OnHitEffect = new Stat();        // dot or debuff applied on hit
        // Add on Effects
        [Header("Explosion")]   // does splash damage
        [JsonProperty]
        public bool IsExplosion = false;
        [JsonProperty]
        public float ExplosionSize = 0;
        [Header("Stick")]
        [JsonProperty]
        public bool IsStick = false;
        [JsonProperty]
        public float StickTime = 0f;
        [Header("Seeking")] // while in air, the projectile will seek out the target if it is in range
        [JsonProperty]
        public bool IsSeek = false;
        [JsonProperty]
        public float SeekDistance = 5;
        [JsonProperty]
        public float SeekDelay = 0.4f;    // delay before seeking begins
        [Header("Summoning")] // After hitting, a portal will open and summon a minion
        [JsonProperty]
        public bool IsSummon = false;
        [JsonProperty]
        public string MinionName = "";
        [JsonProperty]
        public string MinionClass = "";
        [JsonProperty]
        public string MinionRace = "";
        // float PortalOpenSpeed = 1f;
        [Header("Chain Lightning")] // hits one target and moves onto the next
        [JsonProperty]
        public bool IsChain = false;
        [JsonProperty]
        public float ChainDelay = 0.25f;    // delay before chain damage begins
        [JsonProperty]
        public float DamageReduction = 0.8f;
        [Header("Sheilding")] // when applied to a target will sheild them
        [JsonProperty]
        public bool IsSheilding = false;
        [JsonProperty]
        public float SheildDelay = 0.25f;    // delay before chain damage begins
        [Header("Portal")]
        [JsonProperty]
        public bool IsPortal;

        [Header("Art")]
        [JsonProperty]
        public string CastAnimation;
        [JsonProperty]
        public Color32 ColorTint = Color.white;
        [JsonProperty]
        public Voxels.VoxelModel MyModel;

        [JsonIgnore]
        public Character MyCharacter;
        [JsonIgnore]
        public NetworkInstanceId NetworkID;
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
                Sound.Zound MyZound = (DataManager.Get().GetElement(DataFolderNames.Sounds, SoundName) as Zeltex.Sound.Zound);
                if (MyZound != null)
                {
                    return MyZound.GetAudioClip();
                }
            }
            return null;
        }
        #endregion
    }

}