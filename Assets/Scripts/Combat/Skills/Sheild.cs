using UnityEngine;
using System.Collections;

namespace Zeltex.Combat 
{
	public class Sheild : Skill
    {
       // protected new string SkillName = "Sheild";
        //[Header("Input")]
        //public KeyCode ActivateKey = KeyCode.Mouse0;
        //private bool IsInput = true;
        //public bool IsActivated = false;

        [Header("Spell Stats")]
		private bool IsExplodeOnImpact = false;	// if so, it won't repel just explode bullets
		private float RepelForce = 40;

		private MeshRenderer MyMesh;
		private SphereCollider MyCollider;
		private ParticleSystem MyParticles;
		private SheildCollisionHandler MyThingy;

		public override void Start() 
		{
			Transform MySheildObject = transform.Find ("Sheild");
			if (MySheildObject)
			{
				MyMesh = MySheildObject.gameObject.GetComponent<MeshRenderer> ();
				MyCollider = MySheildObject.gameObject.GetComponent<SphereCollider> ();
				MyParticles = MySheildObject.gameObject.GetComponent<ParticleSystem> ();
                MyThingy = MySheildObject.gameObject.GetComponent<SheildCollisionHandler>();
                if (MyThingy == null)
                    MyThingy = MySheildObject.gameObject.AddComponent<SheildCollisionHandler>();
				MyThingy.MySheild = this;
			}
			base.Start ();
			//DoTheStart ();
		}

		//public override 
		void Update() 
		{
			UpdateSheild ();        
			//base.Update();
			//DoTheUpdate ();
		}
        
		override public void ActivateOnNetwork() 	
		{
			//Debug.LogError ("Activating!");
			//if (NewState != IsActivated) 
			{
				if (NewState && HasEnergy() || !NewState) 
				{
					LastTime = Time.time;
					IsActivated = NewState;
					MyMesh.enabled = NewState;
					MyCollider.enabled = NewState;
					MyParticles.enableEmission = NewState;
					MyThingy.enabled = NewState;
					if (NewState)
						UseEnergy ();
				}
			}
		}
		private void UpdateSheild()
		{
			if (IsActivated)
			{
				// consume stat per second
				if (Time.time - LastTime >= Data.FireRate) 
				{
					LastTime = Time.time;
					UseEnergy ();
				}
			}
		}

		public void UseTheForce(GameObject MyColliderObject)
        {
            OnHitSheild(MyColliderObject);
        }

        /// <summary>
        /// The network version of OnHitSheild
        /// </summary>
		/*public void OnHitSheild(int BulletID)
		{
			PhotonView MyBullet = PhotonView.Find(BulletID);
			if (MyBullet && MyBullet.GetComponent<Bullet> ())
			{
				OnHitSheild (MyBullet.gameObject);
			}
		}*/

		public void OnHitSheild(GameObject MyColliderObject) 
		{
			if (IsActivated) 
			{
				if (MyColliderObject.tag != null)
				if (MyColliderObject.tag == "Character" || MyColliderObject.tag == "Bullet") 
				{
					if (MyColliderObject.tag == "Bullet") 
					{
						Zeltex.Combat.Bullet MyBullet = MyColliderObject.GetComponent<Zeltex.Combat.Bullet> ();
						if (MyBullet)
						{
							if (MyBullet.GetSpawner () == gameObject)
								return;	// if bullet belongs to user of sheild dont use the force
							if (!MyBullet.HasUsed()) 
							{
								MyBullet.Explode (MyColliderObject);
								UseEnergy ();	// use more
							}
						}
					}
					//Debug.LogError ("Sheild using the force on" + MyColliderObject.name);
					//Rigidbody MyRigid = MyColliderObject.GetComponent<Rigidbody> ();
					//if (MyRigid) 
					//{
					//	float Force = MyRigid.velocity.magnitude;
					//	if (Force < RepelForce)
					//		Force = RepelForce;
					//	MyRigid.AddForceAtPosition ((MyColliderObject.transform.position - transform.position).normalized * Force, transform.position);
					//}
				}
			}
		}
	}
}
