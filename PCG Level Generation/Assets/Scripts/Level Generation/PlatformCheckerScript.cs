using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformCheckerScript : MonoBehaviour
{
    //ref to source https://stackoverflow.com/questions/63106256/find-and-return-nearest-gameobject-with-tag-unity
    // Start is called before the first frame update

    private float threshold = 2.0f;
    void Start()
    {
        //start with high distance
        float minDistance = 99999.0f;

        //get all ground objects
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("Ground");
        
        //shouldnt happen but if somehow there are no ground objects then dont crash
        if (groundObjects.Length == 0)
           minDistance = 9999999.0f;

        //set current position to platforms position
        Vector3 currentPosition = transform.position;


        //check all ground objects and find the one with the lowest distance
        for (int i = 0; i < groundObjects.Length; i++)
        {
            float distance = Vector3.Distance(currentPosition, groundObjects[i].transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        //if too close then destroy platform
        if (minDistance <= threshold)
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
