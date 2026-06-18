using UnityEngine;

public enum PassiveSpellEffectType
{
    HealOnAttack,
    HealOnKill,
    MaxHealthBonus,
    SpeedBonus,
    DamageBonus,
    ExtraCoinOnKill
}

[CreateAssetMenu(fileName = "SpellData", menuName = "Spell")]
public class SpellData : ScriptableObject
{
    public string spellName;
    [TextArea] public string description;
    public int price;
    public Sprite icon;
    public PassiveSpellEffectType effectType;
    public float value;

    public void ApplyEquipped(PlayerMovement player)
    {
        if(player == null)
            return;

        switch(effectType)
        {
            case PassiveSpellEffectType.MaxHealthBonus:
                player.vitaMassima += value;
                break;
            case PassiveSpellEffectType.SpeedBonus:
                player.speed += value;
                break;
            case PassiveSpellEffectType.DamageBonus:
                player.danno += value;
                break;
        }
    }

    public void RemoveEquipped(PlayerMovement player)
    {
        if(player == null)
            return;

        switch(effectType)
        {
            case PassiveSpellEffectType.MaxHealthBonus:
                player.vitaMassima -= value;

                if(player.Vita > player.VitaMassima)
                    player.Vita = player.VitaMassima;

                break;
            case PassiveSpellEffectType.SpeedBonus:
                player.speed -= value;
                break;
            case PassiveSpellEffectType.DamageBonus:
                player.danno -= value;
                break;
        }
    }

    public void OnAttackHit(PlayerMovement player)
    {
        if(effectType == PassiveSpellEffectType.HealOnAttack)
            HealPlayer(player);
    }

    public void OnEnemyKilled(PlayerMovement player)
    {
        if(effectType == PassiveSpellEffectType.HealOnKill)
            HealPlayer(player);
    }

    public int GetCoinRewardBonus()
    {
        if(effectType != PassiveSpellEffectType.ExtraCoinOnKill)
            return 0;

        return Mathf.RoundToInt(value);
    }

    private void HealPlayer(PlayerMovement player)
    {
        if(player == null || value <= 0f)
            return;

        player.Vita = Mathf.Min(player.Vita + value, player.VitaMassima);
        LifeBar lifeBar = player.GetComponentInChildren<LifeBar>();

        if(lifeBar != null)
            lifeBar.UpdateLifeBar(player.Vita, player.VitaMassima);
    }
}
