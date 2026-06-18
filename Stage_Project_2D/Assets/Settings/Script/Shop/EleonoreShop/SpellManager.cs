using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PassiveSpellInventory))]
public class PassiveSpellManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PassiveSpellInventory inventory;

    private readonly List<SpellData> appliedSpells = new List<SpellData>();

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

        bool added = inventory.AddSpell(spell);

        if(added)
            RefreshInventoryMenu();

        return added;
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
            if(spell != null)
                spell.OnAttackHit(player);
    }

    public void NotifyEnemyKilled()
    {
        if(player == null || inventory == null)
            return;

        foreach(SpellData spell in inventory.EquippedSpells)
            if(spell != null)
                spell.OnEnemyKilled(player);
    }

    public int GetCoinRewardWithPassives(int baseReward)
    {
        if(inventory == null)
            return baseReward;

        int reward = baseReward;

        foreach(SpellData spell in inventory.EquippedSpells)
            if(spell != null)
                reward += spell.GetCoinRewardBonus();

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

            spell.ApplyEquipped(player);
            appliedSpells.Add(spell);
        }
    }

    private void RemoveEquippedBonuses()
    {
        if(player == null)
            return;

        foreach(SpellData spell in appliedSpells)
            if(spell != null)
                spell.RemoveEquipped(player);

        appliedSpells.Clear();
    }

    private void FindMissingReferences()
    {
        if(player == null)
            player = FindObjectOfType<PlayerMovement>();

        if(inventory == null)
            inventory = GetComponent<PassiveSpellInventory>();
    }

    private void RefreshInventoryMenu()
    {
        InventoryMenager inventoryMenager = InventoryMenager.Instance;

        if(inventoryMenager == null)
            inventoryMenager = FindObjectOfType<InventoryMenager>();

        if(inventoryMenager != null)
            inventoryMenager.RefreshSpells();
    }
}
