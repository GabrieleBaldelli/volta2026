using System.Collections.Generic;
using UnityEngine;

public class PassiveSpellInventory : MonoBehaviour
{
    [SerializeField] private int maxEquippedSpells = 3;
    [SerializeField] private List<SpellData> ownedSpells = new List<SpellData>();
    [SerializeField] private List<SpellData> equippedSpells = new List<SpellData>();

    public IReadOnlyList<SpellData> OwnedSpells
    {
        get { return ownedSpells; }
    }

    public IReadOnlyList<SpellData> EquippedSpells
    {
        get { return equippedSpells; }
    }

    public int MaxEquippedSpells
    {
        get { return maxEquippedSpells; }
    }

    public bool OwnsSpell(SpellData spell)
    {
        return spell != null && ownedSpells.Contains(spell);
    }

    public bool IsEquipped(SpellData spell)
    {
        return spell != null && equippedSpells.Contains(spell);
    }

    public bool AddSpell(SpellData spell)
    {
        if(spell == null || ownedSpells.Contains(spell))
            return false;

        ownedSpells.Add(spell);
        return true;
    }

    public bool EquipSpell(SpellData spell)
    {
        if(!OwnsSpell(spell) || IsEquipped(spell) || equippedSpells.Count >= maxEquippedSpells)
            return false;

        equippedSpells.Add(spell);
        return true;
    }

    public bool UnequipSpell(SpellData spell)
    {
        if(spell == null)
            return false;

        return equippedSpells.Remove(spell);
    }
}
