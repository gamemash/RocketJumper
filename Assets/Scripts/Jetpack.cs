using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Jetpack : MonoBehaviour {

	Animator anim;
	Rigidbody2D rb;
	SpriteRenderer sr;
	float lastBoostTime;

	public float jetpackPower;
	public float boostPower;
	public float walkForce;
	public float maxWalkVelocity;
	public Rigidbody2D rocket;

	public LayerMask groundLayer;
	public Transform groundCheck;

	public float boostCoolDownPeriod;

	public Text boostText;
	public Text fuelText;

	bool jumpInput; 

	float fuel = 100;
	public float fuelConsumptionRate;
	public float fuelReplenishRate;
	public float fallMultiplier = 5f;

	public Camera c;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();
		sr = GetComponent<SpriteRenderer> ();
	}

	bool canMove () {
		return true;
	}

	bool canBoost () {				
		return lastBoostTime + boostCoolDownPeriod <= Time.time;
	}

	bool grounded () {
		return Physics2D.OverlapCircle (groundCheck.position, 0.2f, groundLayer);
	}

	void FixedUpdate() {		
		if (jumpInput) {
			if (fuel > 0) {
				fuel = fuel - fuelConsumptionRate;

				if (rb.velocity.y < -1.8) {
					Debug.Log (rb.velocity.y);
					rb.velocity = new Vector2 (rb.velocity.x, rb.velocity.y * 0.7f);
				} else {
					rb.AddForce (new Vector2 (0, jetpackPower), ForceMode2D.Force);
				}

				anim.SetBool ("JetpackOn", true);
			} else {
				anim.SetBool ("JetpackOn", false);
			}
		} else {
			anim.SetBool ("JetpackOn", false);
			if (fuel < 100) {
				fuel = fuel + fuelReplenishRate;
			}
		}

		if (rb.velocity.y < 0 && !jumpInput) {
			rb.velocity += Vector2.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
			rb.drag = 0.3f;
		} else {
			rb.drag = 3;
		}
	}

	void updateBoostText() {
		if (canBoost()) {
			boostText.text = "BOOST READY";
		} else {
			boostText.text = string.Format("{0:N2}", ((lastBoostTime + boostCoolDownPeriod) - Time.time));	
		}	
	}

	void updateFuelText() {		
		fuelText.text = string.Format("FUEL: {0:N}", fuel);
	}
	
	// Update is called once per frame
	void Update () {
//		if (Input.GetAxis ("Jump") > 0) {
//			jumpInput = true;
//		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			Debug.Log ("spaceDown");
			jumpInput = true;
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			Debug.Log ("spaceUp");
			jumpInput = false;
		}
//
		if (canBoost() && Input.GetAxis ("Fire3") > 0) {
			anim.SetTrigger ("Boosted");
			lastBoostTime = Time.time;
			rb.velocity = new Vector2 (rb.velocity.x, 0);
			rb.AddForce (new Vector2 (0, boostPower), ForceMode2D.Impulse);
		}

		updateBoostText ();
		updateFuelText ();
//
		float moveInput = Input.GetAxis ("Horizontal");

		// Cap the walking speed.
		if (Mathf.Abs(rb.velocity.x) > maxWalkVelocity) {
			if (rb.velocity.x < 0) {
				rb.velocity = new Vector2 (-maxWalkVelocity, rb.velocity.y);
			} else {
				rb.velocity = new Vector2 (maxWalkVelocity, rb.velocity.y);
			}
		}

		if (canMove()) {
			rb.AddForce(new Vector2(moveInput * walkForce, 0));
		}

		if (Mathf.Abs (moveInput) > 0) {
			anim.SetFloat ("MoveSpeed", Mathf.Abs (rb.velocity.x));
			anim.speed = Mathf.Abs (0.5f + (Mathf.Abs(rb.velocity.x) / 3f));
		} else {
			anim.SetFloat ("MoveSpeed", 0);
			anim.speed = 1;
		}

		if (moveInput > 0) {
			sr.flipX = false;
		} else if (moveInput < 0) {
			sr.flipX = true;
		}

			
		if (Input.GetButtonDown("Fire1")) {
			FireRocket();
		}
	}		

	void FireRocket () {
	    var pos = Input.mousePosition;
		pos.z = transform.position.z - c.transform.position.z;
		pos = c.ScreenToWorldPoint(pos);

		var q = Quaternion.FromToRotation(Vector3.right, pos - rb.transform.position);
//		var go = Instantiate(prefab, transform.position, q);
//		rb.rigidbody2D.AddForce(go.transform.up * 500.0);

		var spawnpos = rb.transform.position;
		Rigidbody2D rocketClone = (Rigidbody2D) Instantiate(rocket, spawnpos, q);
		Physics2D.IgnoreCollision (GetComponent<Collider2D> (), rocketClone.GetComponent<Collider2D> ());	
	}
}
