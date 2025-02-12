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
    private PlayerMovementScript playerRef; //reference to the player to know when dead
    private Vector3 spawnPoint; //store spawn point
    //decide which way to move
    bool moveLeft = true;

    void Start()
    {
        spawnPoint = transform.position;
        
        playerRef = FindObjectOfType<PlayerMovementScript>();   //find player

    }

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

        //reset if player dies
        if (playerRef.dead)
        {
            transform.position = spawnPoint;
            Debug.Log("Going back");
        }
    }

    bool CheckIfSafe(int direction)
    {
        //fire one raycast to check 1 block ahead on X, and all the way down on Y (1 block ahead)
        //if in future i dont want spider coming off platform, change 100.f in hitY to 1 and it will stay on platform too

        //check for higher ground ahead
        RaycastHit2D hitX = Physics2D.Raycast(new Vector2(transform.position.x + direction, transform.position.y), Vector2.right * direction, 0.01f);

        //check for ground slightly ahead and below (to avoid falling off)
        RaycastHit2D hitY = Physics2D.Raycast(new Vector2(transform.position.x + direction, transform.position.y - 0.5f), Vector2.down, 5.0f);

        //if finds a block in the way or a gap then tell spider not to move
        if ((hitX.collider != null && hitX.collider.gameObject.CompareTag("Ground")) || hitY.collider == null)
        {
            return false;
        }
        else
            return true;
    }
}