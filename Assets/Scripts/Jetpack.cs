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
		// If able to jump
		if (jumpInput) {
			// And I have more than 0 fuel.
			if (fuel > 0) {
				// Eat up some fuel.
				fuel = fuel - fuelConsumptionRate;

				// And add a upward force to the player.
				// However, this force is tuned to act differently depending on what direction the player
				// is falling in. This gives a certain 'gamefeel'. real physics feel floaty, and the game
				// I want to make does not necessarily obey the laws of physics.
				if (rb.velocity.y < -1.8) {
					Debug.Log (rb.velocity.y);
					rb.velocity = new Vector2 (rb.velocity.x, rb.velocity.y * 0.7f);
				} else {
					rb.AddForce (new Vector2 (0, jetpackPower), ForceMode2D.Force);
				}

				// Let the animation controller know that it should play the jetpack animation.
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

		// Increase the rate of falling. This is another 'gamefeel' thing.
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
		// Set jumpInput to true, so we can apply forces in FixedUpdate() which is a more
		// reliable place to apply physics forces.
		// Update() however is a nicer place to watch for input, since it happens more often.
		if (Input.GetKeyDown (KeyCode.Space)) {
			jumpInput = true;
		}

		// Set jumpInput to false when space is let go.
		if (Input.GetKeyUp (KeyCode.Space)) {
			Debug.Log ("spaceUp");
			jumpInput = false;
		}

		// Use the jetpack boost if the player presses 'Shift', but only if canBoost() returns true.
		if (canBoost() && Input.GetAxis ("Fire3") > 0) {
			anim.SetTrigger ("Boosted");
			lastBoostTime = Time.time;
			rb.velocity = new Vector2 (rb.velocity.x, 0); // Cancel all vertical velocity so the boost force is always consistent.
			                                              // This makes the boost predictable for the player, as well as ensures that
			                                              // no matter how fast the player is falling, the boost will always result in
			                                              // satisfying upward motion.
			rb.AddForce (new Vector2 (0, boostPower), ForceMode2D.Impulse);
		}

		// Get the direction of input (W and D keys.) 
		// It is -1 for W and 1 for D.
		float moveInput = Input.GetAxis ("Horizontal");

		// Cap the walking speed.
		if (Mathf.Abs(rb.velocity.x) > maxWalkVelocity) {
			if (rb.velocity.x < 0) {
				rb.velocity = new Vector2 (-maxWalkVelocity, rb.velocity.y);
			} else {
				rb.velocity = new Vector2 (maxWalkVelocity, rb.velocity.y);
			}
		}

		// Apply a force to make the player walk.
		if (canMove()) {
			rb.AddForce(new Vector2(moveInput * walkForce, 0));
		}

		// Make the walking animation speed depend on how fast he is actually walking.
		if (Mathf.Abs (moveInput) > 0) {
			anim.SetFloat ("MoveSpeed", Mathf.Abs (rb.velocity.x));
			anim.speed = Mathf.Abs (0.5f + (Mathf.Abs(rb.velocity.x) / 3f));
		} else {
			anim.SetFloat ("MoveSpeed", 0);
			anim.speed = 1;
		}

		// Flip the sprite depending on the input direction. (So he walks in the right direction :) )
		if (moveInput > 0) {
			sr.flipX = false;
		} else if (moveInput < 0) {
			sr.flipX = true;
		}

			
		// Fire a rocket if the mouse is clicked or ctrl is pressed.
		if (Input.GetButtonDown("Fire1")) {
			FireRocket();
		}

		// Update the GUI elements that indicate timers and other status
		updateBoostText (); 
		updateFuelText ();
	}		

	void FireRocket () {
		// Get the position of the mouse click, not in pixels on the screen, but in the
		// actual world.
	    var pos = Input.mousePosition;
		pos.z = transform.position.z - c.transform.position.z;
		pos = c.ScreenToWorldPoint(pos);

		// Get a rotation that points the rocket in the direction of the mouse click.
		var q = Quaternion.FromToRotation(Vector3.right, pos - rb.transform.position);

		// Spawn a rocket, but make sure it can't colide with the player itself.
		var spawnpos = rb.transform.position;
		Rigidbody2D rocketClone = (Rigidbody2D) Instantiate(rocket, spawnpos, q);
		Physics2D.IgnoreCollision (GetComponent<Collider2D> (), rocketClone.GetComponent<Collider2D> ());	
	}
}
