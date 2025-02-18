using System.Collections;
using UnityEngine;

public class TesterPassFiveScript : MonoBehaviour
{
    private Rigidbody2D testerRB;
    private float jumpHeight = 700;  // the force applied when jumping
    private float moveDir;  // the horizontal direction to move
    private float moveSpeed = 12;
    public bool inAir;
    private bool isJumping = false; //prevent double jumps

    void Start()
    {
        testerRB = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        CheckAhead();

        if (testerRB.velocity.y <= 0 || CheckAhead())
        {
            moveDir = 1.0f;
        }
        else
        {
            moveDir = 0.0f; 
        }

        testerRB.velocity = new Vector2(moveSpeed * moveDir, testerRB.velocity.y);
    }

    private IEnumerator Jump()
    {
        isJumping = true;
        inAir = true;
        Debug.Log("Jumped");

        testerRB.AddForce(new Vector2(testerRB.velocity.x, jumpHeight));

        yield return new WaitForSeconds(0.1f);

        isJumping = false; //allow new jump
    }

    private bool CheckAhead()
    {
        //check if there is a gap directly ahead
        RaycastHit2D hitY1 = Physics2D.Raycast(new Vector2(transform.position.x + 1, transform.position.y - 0.5f), Vector2.down, 100.0f);

        RaycastHit2D[] gaps = new RaycastHit2D[5];

        //check ahead for heightended ground
        RaycastHit2D hitX = Physics2D.Raycast(new Vector2(transform.position.x + 1, transform.position.y), Vector2.right, 0.1f);

        //check if there is a gap or ground **directly** in front to see whether or not to jump
        bool shouldJump = (hitX.collider != null && (hitX.collider.CompareTag("Ground") || hitX.collider.CompareTag("Platform"))) ||
                            (hitY1.collider != null && hitY1.collider.CompareTag("LoseZone"));

        //jump if conditions are correct
        if (shouldJump)
        {
            if (!inAir && !isJumping)
            {
                inAir = true;
                StartCoroutine(Jump());
            }
            return false;   //dont move forward
        }
       
        //ensure there is ground to land on
        for (int i = 0; i < 5; i++)
        {
            Vector2 startPosition = new Vector2(transform.position.x + (i + 1), transform.position.y - 0.5f);

            gaps[i] = Physics2D.Raycast(startPosition, Vector2.down, 100.0f);

            if (gaps[i].collider != null && !gaps[i].collider.gameObject.CompareTag("LoseZone") && !gaps[i].collider.gameObject.CompareTag("Enemy"))
            {
                return true;
            }
        }

        return false;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.5f) // Ensure it's a top collision
                {
                    inAir = false;
                    return;
                }
            }
        }

        if (other.gameObject.CompareTag("Enemy"))
            Physics2D.IgnoreCollision(other.collider, GetComponent<Collider2D>());
    } 

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.5f) // Ensure it's a top collision
                {
                    inAir = false;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Platform"))
        {
            inAir = true;
        }
    }
}