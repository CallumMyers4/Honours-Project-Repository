using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.ShaderGraph.Internal;
#endif
using UnityEngine;

public class BatMovementScript : MonoBehaviour
{
    private Vector2 spawnPoint = new Vector2(0, 0);     //bat's position when spawned in, used to reference where to fly to
    private float moveSpeed = 3.0f, distance = 4;       //speed to move at, how many blocks to go in each direction

    [SerializeField]
    SpriteRenderer spriteRenderer;      //reference to sprite renderer for flipping sprite when turning
    private PlayerMovementScript playerRef;     //reference to the player to know when player dies (to reset)

    bool flyLeft = false;       //when true, move left, else, move right

    void Start()
    {
        spawnPoint = transform.position;            //set spawn point to starting position

        playerRef = FindObjectOfType<PlayerMovementScript>();   //get reference to player's script
    }

    void Update()
    {
        //if moving left
        if (flyLeft)
        {
            //if still within the distance of the path continue flying, else turn around
            if (transform.position.x  > spawnPoint.x - distance)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector2(spawnPoint.x - distance, spawnPoint.y),
                                                        moveSpeed * Time.deltaTime);
            }
            else
            {
                spriteRenderer.flipX = false;
                flyLeft = false;
            }
        }
        //if flying right
        else
        {
            //if still within the distance of the path continue flying, else turn around
            if (transform.position.x  < spawnPoint.x + distance)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector2(spawnPoint.x + distance, spawnPoint.y),
                                                        moveSpeed * Time.deltaTime);
            }
            else
            {
                spriteRenderer.flipX = true;
                flyLeft = true;
            }
        }

        //go back to spawn point if player dies
        if (playerRef.dead)
        {
            transform.position = spawnPoint;
        }
    }
}
