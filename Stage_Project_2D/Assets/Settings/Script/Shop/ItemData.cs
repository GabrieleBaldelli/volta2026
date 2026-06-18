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
    public float value;
    public float duration;

    public bool Consume(PlayerMovement player, MonoBehaviour coroutineRunner)
    {
        if(player == null)
            return false;

        switch(effectType)
        {
            case ItemEffectType.Heal:
                player.Vita = Mathf.Min(player.Vita + value, player.VitaMassima);
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
}
