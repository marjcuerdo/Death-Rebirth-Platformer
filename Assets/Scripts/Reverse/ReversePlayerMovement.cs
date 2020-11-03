﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReversePlayerMovement : MonoBehaviour
{

	public ReverseCharacterController controller;

	public ReverseHealth hObj;
    public ReverseScore sObj;
    public ReverseTimer tObj;

	SpriteRenderer sr; //
    Color srOrigColor; //

    public GameObject spawnPoint1;

	public float runSpeed = 300;

	float horizontalMove = 0f;
	bool jump = false;
	bool crouch = false;
	bool gotHurt = false;
    bool isDead = false;
    bool gotHealth = false;
    public bool advanceLevel = false;
    public bool isNewGame = true;

    SpriteRenderer[] sprites;

    public NextLevel lObj;

	void Start() {
        sObj = GetComponent<ReverseScore>();
		hObj = GetComponent<ReverseHealth>();
        tObj = GetComponent<ReverseTimer>();
        lObj = GameObject.Find("Chest").GetComponent<NextLevel>();

		sr = GetComponent<SpriteRenderer>();

        srOrigColor = sr.color;

        sprites = GetComponentsInChildren<SpriteRenderer>();

        /*for (int i=0; i < spriteRenderers.Count; ++i) {
            colors.Add(spriteRenderers.material.color);
            renderer.material.color = new Color(1,1,1,0.5f);
        }*/
	}

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump")) 
        {
        	jump = true;
        	//Debug.Log("jumping");
        }

        if (Input.GetButtonDown("Crouch")) 
        {
        	crouch = true;
        	//Debug.Log("crouch down");
        } else if (Input.GetButtonUp("Crouch")) {
        	crouch = false;
        	//Debug.Log("crouch up");
        }

        if (gotHurt) {

            for (int i=0; i <sprites.Length; i++) {
                sprites[i].color = new Color(1,1,1,0.5f);
            }
            
            //sr.color = new Color(1,1,1,0.5f);
        	/*for (int i=0; i < spriteRenderers.Count; ++i) {
            
                renderer.material.color = colors[i];
            }*/

            StartCoroutine("FadeBack");
        }

        if (gotHealth) {
            for (int i=0; i <sprites.Length; i++) {
                sprites[i].color = new Color (254/255f, 215/255f, 0f, 1f);
            }
            StartCoroutine("FadeBack");         
        }

        // Restart level if death conditions are met
        if (isDead) {
            isDead = false;

            // reload current level / beginning of checkpoint
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // when chest/finish point is reached, load next level
        if (advanceLevel) {
            lObj.LoadNextScene();
        }

    }

    IEnumerator FadeBack() {
        if (gotHealth) {
            yield return new WaitForSeconds(0.5f);
            for (int i=0; i <sprites.Length; i++) {
                sprites[i].color = srOrigColor;
            }
            gotHealth = false;
        }

        if (gotHurt) {
            yield return new WaitForSeconds(0.5f);
            for (int i=0; i <sprites.Length; i++) {
                sprites[i].color = srOrigColor;
            }
            //sr.color = srOrigColor;
            gotHurt = false;
        }
    }

    void FixedUpdate () 
    {
    	// Move our character
    	controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
    	jump = false;

        // Respawn to beginning of level when health is 0
        if (hObj.health == 0) {

             // continue timer when player's health runs out
            PlayerPrefs.SetFloat("TimeRem", tObj.timeRemaining); 
            PlayerPrefs.SetFloat("TimeInc", tObj.timeInc);
            
            isDead = true;

            // for CHECKPOINTS (possibly)
            //this.transform.position = spawnPoint1.transform.position;
            //hObj.health = 5; // reset health
            //reset level

        }
    	
    }

    // Player triggers with colliders

    void OnTriggerEnter2D(Collider2D col) {

    	if(col.gameObject.tag == "Coins") {
			//Debug.Log("got coin");

            // Add points to player score
            sObj.AddPoints(5);
			Destroy(col.gameObject);
		} 


		// When player gets hurt
		else if (col.gameObject.tag == "Enemy") {

			// Health decrease by 1
			hObj.TakeDamage(1);

			// Change player's color
			gotHurt = true;


			//Debug.Log("got spike hurt");
		}

        // When player gets health pack
        else if (col.gameObject.tag == "Health") {

            // Health decrease by 1
            hObj.AddHealth();

            // make health pack inactive
            col.gameObject.SetActive(false);

            gotHealth = true;
        }

        // When player dies
        else if (col.gameObject.tag == "DeathZone") {
            // Respawn player to beg of level

            // continue timer when player dies
            PlayerPrefs.SetFloat("TimeRem", tObj.timeRemaining); 
            PlayerPrefs.SetFloat("TimeInc", tObj.timeInc);
            isDead = true;
        }

        // When player reaches end of level
        else if (col.gameObject.tag == "Finish") {

            PlayerPrefs.SetFloat("TimeRem", tObj.timeRemaining);
            PlayerPrefs.SetFloat("TimeInc", tObj.timeInc);
            PlayerPrefs.SetInt("Player Score", sObj.score);
            PlayerPrefs.SetInt("Player Health", hObj.health);

            isNewGame = false;
            advanceLevel = true;
        }
    }

    // Player collides with other colliders
    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "Enemy") {
            // Health decrease by 1
            hObj.TakeDamage(1);

            // Change player's color
            gotHurt = true;


            //Debug.Log("got HIT");
        }
    }


}
