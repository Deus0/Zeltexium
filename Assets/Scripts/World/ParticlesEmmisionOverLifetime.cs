using UnityEngine;
using System.Collections;

namespace Zeltex.AnimationUtilities
{
    public class ParticlesEmmisionOverLifetime : MonoBehaviour
    {
	    ParticleSystem MyParticles;
	    //float TimeStart = 0f;
	    //int InitialEmmision = 500;
	    public float TimeTotal = 10f;
	    // Use this for initialization
	    void Awake () {
		    //TimeStart = Time.time;
		    MyParticles = gameObject.GetComponent<ParticleSystem> ();
            if (MyParticles)
            {
		        /*MyParticles.startLifetime = Random.Range (3f,10f);
		        MyParticles.loop = false;
		        MyParticles.Emit(InitialEmmision/5);
		        MyParticles.enableEmission = true;*/
            }
            Destroy (gameObject, TimeTotal);
	    }
	
	    // Update is called once per frame
	    void Update ()
        {
		    if (MyParticles)
            {
			    /*float AnimatePercent =  (Time.time - TimeStart) / TimeTotal;
			    int EmmisionRate = (int)(Mathf.Lerp (InitialEmmision, 0, AnimatePercent));
			    MyParticles.emissionRate = EmmisionRate;
			    MyParticles.startSpeed = 3f*AnimatePercent;*/
			    //MyParticles.startLifetime = 2f-AnimatePercent;
		    }
	    }
    }
}