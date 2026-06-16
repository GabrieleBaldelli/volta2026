using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enable_Collider : MonoBehaviour
{
    public Collider2D colliderDaDisattivare;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(colliderDaDisattivare != null)
                colliderDaDisattivare.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(colliderDaDisattivare != null)
                colliderDaDisattivare.enabled = true;
        }
    }
}
