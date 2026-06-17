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
    private bool hasCustomShieldValue;

    public float shieldSetGet
    {
        get
        {
            return shield;
        }
        set
        {
            SetShield(value);
        }
    }

    private bool isConsumingAttack = false;

    private PlayerMovement playerScript;

    public void Start()
    {
        playerScript = GetComponentInParent<PlayerMovement>();

        if(hasCustomShieldValue)
            shield = Mathf.Clamp(shield, 0f, maxShield);
        else
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

    public void SetShield(float value)
    {
        shield = Mathf.Clamp(value, 0f, maxShield);
        hasCustomShieldValue = true;
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
