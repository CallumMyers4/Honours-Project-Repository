using System.Collections;
using UnityEngine;

public class TesterPassFiveScript : MonoBehaviour
{
    private Rigidbody2D testerRB;
    private float jumpHeight = 700;  // the force applied when jumping
    private float moveDir;  // the horizontal direction to move
    private float moveSpeed = 15;
    public bool inAir;
    private bool isJumping = false; //prevent double jumps

    void Start()
    {
        testerRB = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        CheckAhead();

        if (testerRB.velocity.y <= 0)
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

    private void CheckAhead()
    {     
        RaycastHit2D hitX = Physics2D.Raycast(new Vector2(transform.position.x + 1, transform.position.y), Vector2.right, 0.1f);
        RaycastHit2D hitY = Physics2D.Raycast(new Vector2(transform.position.x + 1, transform.position.y - 0.5f), Vector2.down, 100.0f);

        if (!inAir && !isJumping &&
            ((hitX.collider != null && (hitX.collider.CompareTag("Ground") || hitX.collider.CompareTag("Platform"))) ||
             (hitY.collider != null && hitY.collider.CompareTag("LoseZone"))))
        {
            inAir = true;
            StartCoroutine(Jump());
        }
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
