using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

	public Transform CenterOfMass;
	public Transform explosion;
	Rigidbody2D rb;

	// Use this for initialization
	void Start () {		
		rb = GetComponent<Rigidbody2D> ();
	}

	// Update is called once per frame
	void Update () {		
	}

	void FixedUpdate(){
		rb.AddForce (new Vector2 (rb.transform.right.normalized.x * 1,rb.transform.right.normalized.y * 1), ForceMode2D.Impulse);

		if (rb.velocity != Vector2.zero) {
			rb.transform.right = Vector2.Lerp (rb.transform.right, rb.velocity.normalized, Time.fixedDeltaTime);
		}
	}

	void OnCollisionEnter2D(){
		var expl = Instantiate(explosion, transform.position, Quaternion.identity);
		Destroy(gameObject); // destroy the grenade
		Destroy(expl.GetComponent<PointEffector2D>(), 0.1f);
		Destroy(expl.gameObject, 3f);
	}
}
