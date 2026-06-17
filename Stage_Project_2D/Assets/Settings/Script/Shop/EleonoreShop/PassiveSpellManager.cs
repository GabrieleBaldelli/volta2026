using UnityEngine;

[RequireComponent(typeof(PassiveSpellInventory))]
public class PassiveSpellManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PassiveSpellInventory inventory;

    private float appliedMaxHealthBonus;
    private float appliedSpeedBonus;
    private float appliedDamageBonus;

    private void Awake()
    {
        FindMissingReferences();
    }

    private void OnEnable()
    {
        FindMissingReferences();
        RefreshEquippedBonuses();
    }

    private void OnDisable()
    {
        RemoveEquippedBonuses();
    }

    public bool BuySpell(SpellData spell)
    {
        if(spell == null || player == null || inventory == null)
            return false;

        if(inventory.OwnsSpell(spell) || player.CoinSetGet < spell.price)
            return false;

        player.CoinSetGet -= spell.price;
        return inventory.AddSpell(spell);
    }

    public bool EquipSpell(SpellData spell)
    {
        if(inventory == null || !inventory.EquipSpell(spell))
            return false;

        RefreshEquippedBonuses();
        return true;
    }

    public bool UnequipSpell(SpellData spell)
    {
        if(inventory == null || !inventory.UnequipSpell(spell))
            return false;

        RefreshEquippedBonuses();
        return true;
    }

    public void NotifyAttackHit()
    {
        if(player == null || inventory == null)
            return;

        foreach(SpellData spell in inventory.EquippedSpells)
        {
            if(spell != null && spell.effectType == PassiveSpellEffectType.HealOnAttack)
                HealPlayer(spell.value);
        }
    }

    public void NotifyEnemyKilled()
    {
        if(player == null || inventory == null)
            return;

        foreach(SpellData spell in inventory.EquippedSpells)
        {
            if(spell != null && spell.effectType == PassiveSpellEffectType.HealOnKill)
                HealPlayer(spell.value);
        }
    }

    public int GetCoinRewardWithPassives(int baseReward)
    {
        if(inventory == null)
            return baseReward;

        int reward = baseReward;

        foreach(SpellData spell in inventory.EquippedSpells)
        {
            if(spell != null && spell.effectType == PassiveSpellEffectType.ExtraCoinOnKill)
                reward += Mathf.RoundToInt(spell.value);
        }

        return Mathf.Max(0, reward);
    }

    public void RefreshEquippedBonuses()
    {
        if(player == null || inventory == null)
            return;

        RemoveEquippedBonuses();

        foreach(SpellData spell in inventory.EquippedSpells)
        {
            if(spell == null)
                continue;

            switch(spell.effectType)
            {
                case PassiveSpellEffectType.MaxHealthBonus:
                    appliedMaxHealthBonus += spell.value;
                    break;
                case PassiveSpellEffectType.SpeedBonus:
                    appliedSpeedBonus += spell.value;
                    break;
                case PassiveSpellEffectType.DamageBonus:
                    appliedDamageBonus += spell.value;
                    break;
            }
        }

        player.vitaMassima += appliedMaxHealthBonus;
        player.speed += appliedSpeedBonus;
        player.danno += appliedDamageBonus;
    }

    private void RemoveEquippedBonuses()
    {
        if(player == null)
            return;

        player.vitaMassima -= appliedMaxHealthBonus;
        player.speed -= appliedSpeedBonus;
        player.danno -= appliedDamageBonus;

        appliedMaxHealthBonus = 0;
        appliedSpeedBonus = 0;
        appliedDamageBonus = 0;

        if(player.Vita > player.VitaMassima)
            player.Vita = player.VitaMassima;
    }

    private void HealPlayer(float amount)
    {
        if(amount <= 0)
            return;

        player.Vita = Mathf.Min(player.Vita + amount, player.VitaMassima);
    }

    private void FindMissingReferences()
    {
        if(player == null)
            player = FindObjectOfType<PlayerMovement>();

        if(inventory == null)
            inventory = GetComponent<PassiveSpellInventory>();
    }
}
