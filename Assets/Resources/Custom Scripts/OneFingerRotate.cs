using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneFingerRotate : MonoBehaviour
{
    private float rotationRate = 1.0f;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 1)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    transform.Rotate(0, -touch.deltaPosition.x * rotationRate, 0, Space.World);
                }
            }
        }    
    }
}
