using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
nella shieldCorutine come faresti per far si che mentre il nemico sta attaccando
e io shieldo ma non nell'istante in cui attacco, ma ho cmq lo scudo, la sua barra si consuma
Input.GetMouseButtonDown(1) == true / false
*/

public class ShieldBar : MonoBehaviour
{
    public const int SHIELD_MAX = 5;
    private int shield;

    public void Start()
    {
        shield = SHIELD_MAX;
    }

    public void Update()
    {
        GameObject p = GameObject.Find("Player");
        PlayerMovement playerScript = p.GetComponent<PlayerMovement>();

        GameObject e = GameObject.Find("Enemy1");
        Enemy1 enemyScript = e.GetComponent<Enemy1>();
        
        if(playerScript.IsShielding == true && enemyScript.IsAttacking && Input.GetMouseButtonDown(1) == false)
        {
            UpdateShieldBar();
        }
    }

    public void UpdateShieldBar()
    {
        shield--;

        Slider slider = GetComponent<Slider>();
        slider.value = shield / SHIELD_MAX;
    }
}
