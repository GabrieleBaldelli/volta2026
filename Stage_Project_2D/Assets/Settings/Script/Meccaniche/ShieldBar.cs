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
    public const float SHIELD_MAX = 5f;
    private float shield;

    public float shieldSetGet
    {
        get
        {
            return shield;
        }
        set
        {
            shield = value;
        }
    }

    private bool isConsumingAttack = false;

    private PlayerMovement playerScript;

    public void Start()
    {
        playerScript = GetComponentInParent<PlayerMovement>();
        shield = SHIELD_MAX;
    }

    public void Update()
    {
        if(isConsumingAttack)
            return;

        if(playerScript == null)
            return;

        if(playerScript.IsAnyEnemyAttacking() && playerScript.IsPerfectShildingSetGet == false && playerScript.IsShildingSetGet)
        {
            StartCoroutine(UpdateShieldBar());
        }
    }

    public IEnumerator UpdateShieldBar()
    {
        isConsumingAttack = true;

        shield--;

        Slider slider = GetComponent<Slider>();
        slider.value = shield / SHIELD_MAX;

        yield return new WaitForSeconds(0.5f);

        isConsumingAttack = false;
    }
}
