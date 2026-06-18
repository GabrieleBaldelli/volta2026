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
    [Tooltip("HealOnAttack/HealOnKill: percentuale della vita massima curata. DamageBonus: moltiplicatore diretto, danno finale = danno * value. ExtraCoinOnKill: monete sommate. SpeedBonus/MaxHealthBonus: valore sommato.")]
    public float value;

    private float appliedDamageBonus;
    private bool hasAppliedDamageBonus;

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
                appliedDamageBonus = player.danno * value - player.danno;
                player.danno += appliedDamageBonus;
                hasAppliedDamageBonus = true;
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
                if(hasAppliedDamageBonus)
                {
                    player.danno -= appliedDamageBonus;
                    appliedDamageBonus = 0f;
                    hasAppliedDamageBonus = false;
                }
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

        float healAmount = player.VitaMassima * (value / 100f);
        player.Vita = Mathf.Min(player.Vita + healAmount, player.VitaMassima);
        LifeBar lifeBar = player.GetComponentInChildren<LifeBar>();

        if(lifeBar != null)
            lifeBar.UpdateLifeBar(player.Vita, player.VitaMassima);
    }

    public string GetValueMeaning(PlayerMovement player = null)
    {
        switch(effectType)
        {
            case PassiveSpellEffectType.HealOnAttack:
            case PassiveSpellEffectType.HealOnKill:
                return GetHealingMeaning(player);
            case PassiveSpellEffectType.MaxHealthBonus:
                return "Value si somma alla vita massima: +" + FormatValue(value) + " PV.";
            case PassiveSpellEffectType.SpeedBonus:
                return "Value si somma alla velocita': +" + FormatValue(value) + " speed.";
            case PassiveSpellEffectType.DamageBonus:
                return GetDamageMeaning(player);
            case PassiveSpellEffectType.ExtraCoinOnKill:
                return "Value si somma alle monete ottenute: +" + Mathf.RoundToInt(value) + " coin.";
            default:
                return string.Empty;
        }
    }

    private string GetHealingMeaning(PlayerMovement player)
    {
        if(player == null)
            return "Value cura una percentuale della vita massima: " + FormatValue(value) + "%.";

        float healAmount = player.VitaMassima * (value / 100f);
        return "Value cura il " + FormatValue(value) + "% della vita massima (" + FormatValue(player.VitaMassima) + "): +" + FormatValue(healAmount) + " PV.";
    }

    private string GetDamageMeaning(PlayerMovement player)
    {
        if(player == null)
            return "Value moltiplica direttamente il danno: danno finale = danno * " + FormatValue(value) + ".";

        float finalDamage = player.danno * value;
        return "Value moltiplica direttamente il danno. Danno " + FormatValue(player.danno) + " * " + FormatValue(value) + " = " + FormatValue(finalDamage) + ".";
    }

    private string FormatValue(float number)
    {
        return number.ToString("0.##");
    }
}
