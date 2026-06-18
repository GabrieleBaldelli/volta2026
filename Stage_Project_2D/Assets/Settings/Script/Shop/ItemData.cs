using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ItemEffectType
{
    None,
    Heal,
    RestoreShield,
    SpeedBoost,
    DamageBoost,
    MaxHealthBoost,
    AddCoins
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Item")] 

public class ItemData : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public int price;
    public Sprite icon;
    public ItemEffectType effectType;
    [Tooltip("Heal: percentuale della vita massima curata. RestoreShield/AddCoins/MaxHealthBoost: valore sommato. SpeedBoost/DamageBoost: valore sommato temporaneamente per duration secondi.")]
    public float value;
    public float duration;

    public bool Consume(PlayerMovement player, MonoBehaviour coroutineRunner)
    {
        if(player == null)
            return false;

        switch(effectType)
        {
            case ItemEffectType.Heal:
                player.Vita = Mathf.Min(player.Vita + GetHealingAmount(player), player.VitaMassima);
                UpdateLifeBar(player);
                return true;
            case ItemEffectType.RestoreShield:
                RestoreShield(player);
                return true;
            case ItemEffectType.SpeedBoost:
                StartEffectCoroutine(coroutineRunner, TemporarySpeed(player));
                return true;
            case ItemEffectType.DamageBoost:
                StartEffectCoroutine(coroutineRunner, TemporaryDamage(player));
                return true;
            case ItemEffectType.MaxHealthBoost:
                player.IncreaseMaxHealth(value, true);
                return true;
            case ItemEffectType.AddCoins:
                player.CoinSetGet += Mathf.RoundToInt(value);
                return true;
            default:
                return false;
        }
    }

    private void RestoreShield(PlayerMovement player)
    {
        ShieldBar shieldBar = player.GetComponentInChildren<ShieldBar>();

        if(shieldBar != null)
            shieldBar.shieldSetGet = Mathf.Min(shieldBar.shieldSetGet + value, shieldBar.maxShield);
    }

    private IEnumerator TemporarySpeed(PlayerMovement player)
    {
        player.speed += value;
        yield return new WaitForSeconds(duration);

        if(player != null)
            player.speed -= value;
    }

    private IEnumerator TemporaryDamage(PlayerMovement player)
    {
        player.danno += value;
        yield return new WaitForSeconds(duration);

        if(player != null)
            player.danno -= value;
    }

    private void StartEffectCoroutine(MonoBehaviour coroutineRunner, IEnumerator routine)
    {
        if(coroutineRunner != null)
            coroutineRunner.StartCoroutine(routine);
    }

    private void UpdateLifeBar(PlayerMovement player)
    {
        LifeBar lifeBar = player.GetComponentInChildren<LifeBar>();

        if(lifeBar != null)
            lifeBar.UpdateLifeBar(player.Vita, player.VitaMassima);
    }

    public string GetValueMeaning(PlayerMovement player = null)
    {
        switch(effectType)
        {
            case ItemEffectType.Heal:
                return GetHealingMeaning(player);
            case ItemEffectType.RestoreShield:
                return "Value si somma allo scudo attuale: +" + FormatValue(value) + " shield.";
            case ItemEffectType.SpeedBoost:
                return "Value si somma temporaneamente alla velocita': +" + FormatValue(value) + " speed per " + FormatValue(duration) + "s.";
            case ItemEffectType.DamageBoost:
                return "Value si somma temporaneamente al danno: +" + FormatValue(value) + " danno per " + FormatValue(duration) + "s.";
            case ItemEffectType.MaxHealthBoost:
                return "Value si somma alla vita massima: +" + FormatValue(value) + " PV.";
            case ItemEffectType.AddCoins:
                return "Value si somma alle monete: +" + Mathf.RoundToInt(value) + " coin.";
            default:
                return string.Empty;
        }
    }

    private float GetHealingAmount(PlayerMovement player)
    {
        if(player == null)
            return 0f;

        return player.VitaMassima * (value / 100f);
    }

    private string GetHealingMeaning(PlayerMovement player)
    {
        if(player == null)
            return "Value cura una percentuale della vita massima: " + FormatValue(value) + "%.";

        return "Value cura il " + FormatValue(value) + "% della vita massima (" + FormatValue(player.VitaMassima) + "): +" + FormatValue(GetHealingAmount(player)) + " PV.";
    }

    private string FormatValue(float number)
    {
        return number.ToString("0.##");
    }
}
