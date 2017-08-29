    using UnityEngine;
using Zeltex.AnimationUtilities;
using Zeltex.Characters;

namespace Zeltex.WorldUtilities
{
	/// <summary>
    /// Teleports a character when they walk on a collider
    /// If they stand on it for over a 2 seconds, their vision fades
    /// Then their body fades and they cannot move
    /// Then a delay for a  0.2 second x blocks away - it will show a particle moving there
    /// Then they rematerialize on the other side, with a one second fading back!
    /// Can also teleport them to other levels!
    /// </summary>
	public class Teleporter : MonoBehaviour
    {
        #region Variables
        public int EmmissionRate = 80;
		public float TeleportCooldown = 5f;
		float LastTeleported = -15f;
		public GameObject TeleportLocation;
		[Tooltip("The Particles created when item is picked up")]
		public GameObject ParticlesPrefab;
		[Tooltip("Played when item is picked up")]
		public AudioClip MyPickupSound;
		private bool IsUseDifference = false;
		private ParticleSystem MyParticles;

        private Character TeleportingCharacter;
        #endregion

        #region Mono
        void Start()
        {
            MyParticles = gameObject.GetComponent<ParticleSystem>();
            if (ParticlesPrefab)
            {
                ParticleSystem ParticlesB = ParticlesPrefab.GetComponent<ParticleSystem>();
                CopyParticles(ParticlesB, MyParticles);
            }
        }

        void Update()
        {
            if (Time.time - LastTeleported < TeleportCooldown)
            {
                float TimePercent = (Time.time - LastTeleported) / TeleportCooldown;
                if (Time.time - LastTeleported < TeleportCooldown - 1f)
                {
                    TimePercent = 0f;
                }
                if (MyParticles)
                {
                    MyParticles.emissionRate = EmmissionRate * TimePercent;
                }
            }
        }

        private void CopyParticles(ParticleSystem ParticlesA, ParticleSystem ParticlesB) 
		{
			ParticlesA.startColor = ParticlesB.startColor;
			ParticlesA.startDelay = ParticlesB.startDelay;
			ParticlesA.startLifetime = ParticlesB.startLifetime;
			ParticlesA.startRotation = ParticlesB.startRotation;
			ParticlesA.startSize = ParticlesB.startSize;
			ParticlesA.startSpeed = ParticlesB.startSpeed;
			ParticlesA.gravityModifier = ParticlesB.gravityModifier;
		}
        #endregion
        /// <summary>
        /// Links 2 teleporters together
        /// </summary>
        public void LinkTeleporter(Transform OtherTeleporter)
        {
            if (OtherTeleporter)
            {
                TeleportLocation = OtherTeleporter.gameObject;
                OtherTeleporter.gameObject.GetComponent<Teleporter>().TeleportLocation = gameObject;
            }
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Character>())
            {
                OnContact(other.gameObject);
            }
            else
            {
                GameObject MyCharacter = other.transform.FindRootFromBone();
                if (MyCharacter)
                {
                    OnContact(MyCharacter);
                }
            }
            /*if (other.gameObject.tag == "BonePart")
            {
                //Debug.LogError("Character: " + other.gameObject.name + " has entered teleport zone.");
            }*/
        }
        public void OnContact(GameObject MyCharacter)
        {
            //Debug.LogError("Character: " + MyCharacter.name + " has entered teleport zone.");
            TeleportToLocation(MyCharacter);
        }
		private void Use()
        {
			LastTeleported = Time.time;
		}

		public void TeleportToLocation(GameObject MyCharacter)
        {
			if (MyCharacter.GetComponent<Character>())
            {
			    if (Time.time - LastTeleported > TeleportCooldown)
                {
				    Use();
				    Teleporter MyTeleporter2 = TeleportLocation.GetComponent<Teleporter>();
				    //if (CollideWithItem.GetComponent<Zeltex.Items.ItemObject>())
                    {
					    Vector3 Difference = (TeleportLocation.transform.position-transform.position);	// difference between teleport locations
					    if (IsUseDifference)
                        {
                            MyCharacter.transform.position += Difference;
                        }
					    else
                        {
						    MyCharacter.transform.position = new Vector3(TeleportLocation.transform.position.x,
						                                                 MyCharacter.transform.position.y+Difference.y,
						                                                 TeleportLocation.transform.position.z);
					    }
					    if (MyTeleporter2 != null)
                        {
						    MyTeleporter2.Use();
					    }
					
					    if (ParticlesPrefab)
                        {
						    GameObject ItemLeftOver = (GameObject)Instantiate(
                                ParticlesPrefab,
                                TeleportLocation.transform.position, 
                                ParticlesPrefab.transform.rotation);
						    if (MyPickupSound)
                            {
							    AudioSource MySource = ItemLeftOver.AddComponent<AudioSource> ();
							    if (MyPickupSound != null)
                                {
								    MySource.PlayOneShot (MyPickupSound);
                                }
                            }
						    ItemLeftOver.AddComponent<ParticlesEmmisionOverLifetime> ();
						    Destroy (ItemLeftOver, 5f);
					    }
				    }
                }
            }
        }
	}
}