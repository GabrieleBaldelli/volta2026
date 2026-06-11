using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform PlayerPosition;
    public float Speed = 1.0f;

    //private Vector3 offset_Y = new Vector3(0, 2f, 0);

    private void LateUpdate()
    {
        if (PlayerPosition == null) 
        {
            Debug.LogWarning("PlayerPosition is not assigned in the FollowPlayer script.");
            return;
        }

        //offset_Y = PlayerPosition.position + offset_Y;

        transform.position = PlayerPosition.position; // + offset_Y;
        // Z = lerpSpeed * Time.deltaTime
    }
}
