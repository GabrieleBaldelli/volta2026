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
    public float maxShield = 5f;
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
        shield = maxShield;
        UpdateShieldVisual();
    }

    public void Update()
    {
        if(isConsumingAttack)
            return;

        if(playerScript == null)
            return;

        if(playerScript.IsAnyEnemyAttacking() && playerScript.IsPerfectShieldingSetGet == false && playerScript.IsShieldingSetGet)
        {
            StartCoroutine(UpdateShieldBar());
        }
    }

    public IEnumerator UpdateShieldBar()
    {
        isConsumingAttack = true;

        shield--;

        UpdateShieldVisual();

        while(playerScript != null && playerScript.IsAnyEnemyAttacking() && playerScript.IsShieldingSetGet)
        {
            yield return null;
        }

        isConsumingAttack = false;
    }

    public void IncreaseMaxShield(float amount, bool refillShield)
    {
        maxShield += amount;

        if(refillShield)
        {
            shield = maxShield;
        }
        else
        {
            shield = Mathf.Min(shield, maxShield);
        }

        UpdateShieldVisual();
    }

    private void UpdateShieldVisual()
    {
        Slider slider = GetComponent<Slider>();

        if(slider != null && maxShield > 0)
        {
            slider.value = shield / maxShield;
        }
    }
}
