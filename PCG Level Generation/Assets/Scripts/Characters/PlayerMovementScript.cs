using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovementScript : MonoBehaviour
{
    public bool dead, win;  //keep track of when player dies or wins the level
    private float moveSpeed = 15, jumpHeight = 700;    //horizontal move speed of player, vertical force applied when player jumps
    private bool inAir = false;     //keep track of when not player is in the air
    private float moveDir;  //the direction to move
    private Vector2 spawnPoint; //store the players start point in the level
    public int coinsCollected = 0;  //how many coins the player has picked up in the current level
    private Rigidbody2D playerRB;   //access to rigidbody component of the player
    private Animator playerAnimator;    //access to the animator component of the player
    private SpriteRenderer playerRenderer;  //access to the sprite renderer component of the player

    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();     //assign the player's rigidbody component
        playerAnimator = GetComponent<Animator>();  //assign the player's animator component
        playerRenderer = GetComponent<SpriteRenderer>();  //assign the player's sprite renderer component

        //assign player's spawn point to starting position
        spawnPoint.x = transform.position.x;
        spawnPoint.y = transform.position.y;
    }


    void Update()
    {
        //failsafe in case player falls off map without hitting lose zone, the level will reset
        if (transform.position.y < -10)
        {
            dead = true;
            transform.position = new Vector3(spawnPoint.x, spawnPoint.y, transform.position.z);
        }

        moveDir = Input.GetAxis("Horizontal");  //get the player's input using the built in input values in the project settings

        playerAnimator.SetBool("midair", inAir);    //use jumping animation when not on the ground

        //if player is moving, play running animation otherwise use idle 
        if (moveDir != 0)
        {
            playerAnimator.SetBool("moving", true);     //tells animator to set animation to running

            if (moveDir < 0)
            {
                playerRenderer.flipX = true;    //face the sprite left
            }
            else
            {
                playerRenderer.flipX = false;   //face the sprite right
            }
        }
        else
            playerAnimator.SetBool("moving", false);    //tells animator to switch back to idle

        //jump when player presses the jump button whilst on the ground
        if (Input.GetButtonDown("Jump") && inAir == false) 
        {
            playerRB.AddForce(new Vector2(playerRB.velocity.x, jumpHeight));    //add a force to the Y velocity of the player's rigidbody
        }
    }


    void FixedUpdate()
    {
        playerRB.velocity = new Vector2(moveSpeed * moveDir, playerRB.velocity.y);  //change the velocity of the player's rigidbody to move in X axis
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //if other object is ground or platform 
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Platform"))
        {
            //run through every contact point in the other gameobject
            foreach (ContactPoint2D contact in other.contacts)
            {
                //if the player's contact is above the other object's contact
                if (contact.normal.y > 0.5f)
                {
                    inAir = false;  //tell game player is back on the ground
                    return;
                }
            }
        }
        
        //if other object is an enemy or part of the lose zone beneath gaps
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("LoseZone"))
        {
            dead = true;    //tell other script player is dead
            transform.position = new Vector3(spawnPoint.x, spawnPoint.y, transform.position.z);     //reset to spawn
        }
    } 

    private void OnCollisionStay2D(Collision2D other)
    {
        //if other object is ground or platform 
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Platform"))
        {
            //run through every contact point in the other gameobject
            foreach (ContactPoint2D contact in other.contacts)
            {
                //if the player's contact is above the other object's contact
                if (contact.normal.y > 0.5f)
                {
                    inAir = false;  //tell game player is back on the ground
                    return;
                }
            }
        }
    }


    private void OnCollisionExit2D(Collision2D other)
    {
        //if coming off the ground or a platform
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Platform"))
        {
            inAir = true;   //tell game player is not grounded
        }
        
        //if moving away from enemy or lose zone
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("LoseZone"))
        {
            dead = false;   //set player to back alive since they have moved back to spawn point
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {      
        //if jumping through a coin object
        if (other.gameObject.CompareTag("Collectable"))
        {
            coinsCollected++;   //increase the number of coins collected by player
            Destroy(other.gameObject);  //destroy the coin to prevent multiple pickups
        }

        //if other object is part of the win zone
        if (other.gameObject.CompareTag("WinZone"))
            win = true;     //run the player wins functions
    }
}
