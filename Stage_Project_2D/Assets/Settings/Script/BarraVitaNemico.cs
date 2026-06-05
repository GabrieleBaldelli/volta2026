using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);


    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }

    }

}