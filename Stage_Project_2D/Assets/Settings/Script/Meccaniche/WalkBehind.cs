using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkBehind : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            GetComponentInParent<SpriteRenderer>().sortingLayerName = "WalkBehind";
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            GetComponentInParent<SpriteRenderer>().sortingLayerName = "WalkInfront";
        }
    }
}
