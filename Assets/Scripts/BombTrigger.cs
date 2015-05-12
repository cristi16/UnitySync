using UnityEngine;
using System.Collections;

public class BombTrigger : MonoBehaviour {

	void Start () {
	
	}
	
    void OnTriggerEnter(Collider col)
    {
       if(col.gameObject.tag == "FPS")
       {
           foreach(var ps in GetComponentsInChildren<ParticleSystem>())
           {
               ps.Play();
           }
       }
    }
}
