using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BatMovementScript : MonoBehaviour
{
    //rememeber where bat was spawned
    private Vector2 spawnPoint = new Vector2(0, 0);
    //speed to move at
    private float moveSpeed = 3.0f;
    //how many blocks to go in each direction
    private float distance = 4;
    //reference to sprite renderer for flipping
    [SerializeField]
    SpriteRenderer spriteRenderer;

    //decide which way to fly
    bool flyLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        //set spawn point
        spawnPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (flyLeft)
        {
            Debug.Log("Going left");
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
        else
        {
            Debug.Log("Going right");
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
    }
}
