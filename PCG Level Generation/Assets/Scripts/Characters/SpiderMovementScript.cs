using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovementScript : MonoBehaviour
{
    private float moveSpeed = 3.0f;    //speed to move at
    private Vector3 spawnPoint; //remember spawn point
    private bool moveLeft = true;       //decide which way to move
    [SerializeField]
    SpriteRenderer spriteRenderer;      //reference to sprite renderer for flipping
    private PlayerMovementScript playerRef; //reference to the player to know when player dies

    void Start()
    {
        spawnPoint = transform.position;    //set spawn point to starting position
        
        playerRef = FindObjectOfType<PlayerMovementScript>();   //find reference to player object

    }

    // Update is called once per frame
    void Update()
    {
        //if moving left and is still safe, go left, else try going right
        if (moveLeft)
        {  
            //check if position - 1 (x) is safe for spider to move forward, then move if so
            if (CheckIfSafe(-1))
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.position.x - 1, transform.position.y),
                                                        moveSpeed * Time.deltaTime);
            }
            //otherwise flip the sprite and try moving right
            else
            {
                spriteRenderer.flipX = true;
                moveLeft = false;
            }
        }
        else
        {
            //check if position + 1 (x) is safe for spider to move forward, then move if so
            if (CheckIfSafe(1))
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector2(transform.position.x + 1, transform.position.y),
                                                        moveSpeed * Time.deltaTime);
            }
            //otherwise flip sprite and try moving left
            else
            {
                spriteRenderer.flipX = false;
                moveLeft = true;
            }
        }

        //reset position to spawn point if player dies
        if (playerRef.dead)
        {
            transform.position = spawnPoint;
        }
    }

    //function used to check if next position in current direction is safe to move to
    private bool CheckIfSafe(int direction)
    {
        //fire a raycast in a straight line ahead
        RaycastHit2D hitX = Physics2D.Raycast(new Vector2(transform.position.x + direction, transform.position.y), Vector2.right * direction, 0.01f);

        //fire a raycast a block ahead directly downwards
        RaycastHit2D hitY = Physics2D.Raycast(new Vector2(transform.position.x + direction, transform.position.y - 0.5f), Vector2.down, 5.0f);

        //if the straight raycast hits higher ground or if the downwards raycast hits a lose zone (meaning theres a gap ahead)
        if ((hitX.collider != null && hitX.collider.gameObject.CompareTag("Ground")) || hitY.collider == null || hitY.collider.gameObject.CompareTag("LoseZone"))
        {
            return false;   //return not safe to move
        }
        //if the next block is walkable
        else
            return true;    //return safe to move
    }
}