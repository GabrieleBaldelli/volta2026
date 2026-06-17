using UnityEngine;

public class PassiveSpellShop : MonoBehaviour
{
    [SerializeField] private PassiveSpellManager spellManager;
    [SerializeField] private SpellData[] spellsForSale;

    private void Awake()
    {
        if(spellManager == null)
            spellManager = FindObjectOfType<PassiveSpellManager>();
    }

    public bool BuySpell(int index)
    {
        if(spellManager == null || spellsForSale == null || index < 0 || index >= spellsForSale.Length)
            return false;

        return spellManager.BuySpell(spellsForSale[index]);
    }

    public bool EquipSpell(int index)
    {
        if(spellManager == null || spellsForSale == null || index < 0 || index >= spellsForSale.Length)
            return false;

        return spellManager.EquipSpell(spellsForSale[index]);
    }

    public bool UnequipSpell(int index)
    {
        if(spellManager == null || spellsForSale == null || index < 0 || index >= spellsForSale.Length)
            return false;

        return spellManager.UnequipSpell(spellsForSale[index]);
    }
}
