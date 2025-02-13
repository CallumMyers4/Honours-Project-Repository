using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterPassFiveScript : MonoBehaviour
{
    private Rigidbody2D testerRB;
    private float jumpHeight = 700;     //the force applied when jumping
    private float moveDir;  //the horizontal direction to move
    private float moveSpeed = 15;
    public bool inAir;

    // Start is called before the first frame update
    void Start()
    {
        testerRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //check if jump is needed
        CheckAhead();

        //move forward unless jumping (jumping is the only case where velocity.y > 0, since it is the only time player moves up)
        if (testerRB.velocity.y <= 0)
        {
            moveDir = 1.0f;
        }
        else
        {
            moveDir = 0.0f; //while jumping dont move, to ensure the jump covers as much ground as possible, only move at the peak height
        }

        testerRB.velocity = new Vector2(moveSpeed * moveDir, testerRB.velocity.y);  //change the velocity of the
                                                                                    // rigidbody to move in X axis
    }

    private void Jump()
    {
        if (!inAir)
            testerRB.AddForce(new Vector2(testerRB.velocity.x, jumpHeight));    //add a force to the Y velocity of the player's rigidbody
    }

    private void CheckAhead()
    {     
        //check for higher ground ahead
        RaycastHit2D hitX = Physics2D.Raycast(new Vector2(transform.position.x + 1, transform.position.y), Vector2.right, 0.1f);

        //check for ground slightly ahead and below (to avoid falling off)
        RaycastHit2D hitY = Physics2D.Raycast(new Vector2(transform.position.x + 1, transform.position.y - 0.5f), Vector2.down, 100.0f);

        //jump if the next block is ground/platform
        if (hitX.collider != null && hitX.collider.gameObject.CompareTag("Ground"))
        {
            Jump();
        }

        if (hitX.collider != null && hitX.collider.gameObject.CompareTag("Platform"))
        {
            Jump();
        }

        if (hitY.collider != null && hitY.collider.gameObject.CompareTag("LoseZone"))
        {
            Jump();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") && other.gameObject.transform.position.y < testerRB.position.y)
        {
            inAir = false;
        }

        if (other.gameObject.CompareTag("Enemy")) 	
            Physics2D.IgnoreCollision(other.collider, GetComponent<Collider2D>()); 
    } 

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") && other.gameObject.transform.position.y < testerRB.position.y)
        {
            inAir = false;
        }
    }


    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            inAir = true;
        }
    } 
}
