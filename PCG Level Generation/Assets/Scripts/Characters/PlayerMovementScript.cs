using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    //Variables
    private float moveSpeed = 5;    //the horizontal move speed of player
    private float jumpHeight = 350;     //the force applied when player jumps
    private bool inAir = false;     //whether or not player is in the air
    private float moveDir;  //the horizontal direction to move
    private Rigidbody2D playerRB;   //access to rigidbody component of the player
    private Animator playerAnimator;    //access to the animator component of the player
    private SpriteRenderer playerRenderer;  //access to the sprite renderer component of the player


    // Start is called before the first frame update
    void Start()
    {
        //Setting Components
        playerRB = GetComponent<Rigidbody2D>();     //assign the player's rigidbody component to the variable
        playerAnimator = GetComponent<Animator>();  //assign the player's animator component to the variable
        playerRenderer = GetComponent<SpriteRenderer>();  //assign the player's sprite renderer component to the variable
    }


    // Update is called once per frame used for physics calculations
    void Update()
    {
        //Input
        moveDir = Input.GetAxis("Horizontal");  //get the player's input using the built in input values in the project settings


        //Animations
        playerAnimator.SetBool("midair", inAir);    //use jumping animation when not on the ground

        //use running animation when moving
        if (moveDir != 0)
        {
            playerAnimator.SetBool("moving", true);     //tells animator to set animation to running

            if (moveDir < 0)
            {
                playerRenderer.flipX = true; //face the sprite left
            }
            else
            {
                playerRenderer.flipX = false; //face the sprite right
            }
        }
        else
            playerAnimator.SetBool("moving", false);    //tells animator to switch back to idle

        
    }


    // FixedUpdate is called once per frame after Update and used for physics calculations
    void FixedUpdate()
    {
        playerRB.velocity = new Vector2(moveSpeed * moveDir, playerRB.velocity.y);  //change the velocity of the
                                                                                    // player's rigidbody to move in X axis

        //jump method
        if (Input.GetButtonDown("Jump") && inAir == false)  //checks if player is pressing the jump button whilst on the ground
        {
            playerRB.AddForce(new Vector2(playerRB.velocity.x, jumpHeight));    //add a force to the Y velocity of the player's rigidbody
        }
    }


    // OnCollisionEnter2D triggers when the rigidbody detects a collision with another GameObject
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            inAir = false;
        }
    } 


    // OnCollisionExit2D triggers when the rigidbody detects the rigibody has moved outwith a collision zone
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            inAir = true;
        }
    } 
}
