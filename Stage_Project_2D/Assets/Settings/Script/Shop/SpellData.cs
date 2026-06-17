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

[CreateAssetMenu(fileName = "SpellData", menuName = "Eleonore Shop/Passive Spell")]
public class SpellData : ScriptableObject
{
    public string spellName;
    [TextArea] public string description;
    public int price;
    public Sprite icon;
    public PassiveSpellEffectType effectType;
    public float value;
}
