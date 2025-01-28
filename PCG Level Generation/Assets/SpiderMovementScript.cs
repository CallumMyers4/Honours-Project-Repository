using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovementScript : MonoBehaviour
{
    //speed to move at
    private float moveSpeed = 3.0f;
    //reference to sprite renderer for flipping
    [SerializeField]
    SpriteRenderer spriteRenderer;

    //decide which way to move
    bool moveLeft = true;

    // Update is called once per frame
    void Update()
    {
        if (moveLeft)
        {
            if (CheckIfSafe(-1))
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.position.x - 1, transform.position.y),
                                                        moveSpeed * Time.deltaTime);
            }
            else
            {
                spriteRenderer.flipX = true;
                moveLeft = false;
            }
        }
        else
        {
            if (CheckIfSafe(1))
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.position.x + 1, transform.position.y),
                                                        moveSpeed * Time.deltaTime);
            }
            else
            {
                spriteRenderer.flipX = false;
                moveLeft = true;
            }
        }
    }

    bool CheckIfSafe(int direction)
    {
        //fire one raycast to check 1 block ahead on X, and all the way down on Y (1 block ahead)
        RaycastHit2D hitX = Physics2D.Raycast(new Vector2(transform.position.x + direction, transform.position.y), Vector2.zero, 1.0f);
        RaycastHit2D hitY = Physics2D.Raycast(new Vector2(transform.position.x + direction, transform.position.y), Vector2.down, 100.0f);

        //if finds a block in the way or a gap then tell spider not to move
        if ((hitX.collider != null && hitX.collider.gameObject.CompareTag("Ground")) || hitY.collider == null)
        {
            return false;
        }
        else
            return true;
    }
}