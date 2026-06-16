using UnityEngine;

public class PlayerUpgradeStats : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement player;
    public ShieldBar shieldBar;

    [Header("Upgrade Points")]
    public int upgradePoints = 0;

    [Header("Sword")]
    public int swordLevel = 1;
    public int maxSwordLevel = 20;
    public float swordDamageIncrease = 0.5f;

    [Header("Shield")]
    public int shieldLevel = 1;
    public int maxShieldLevel = 20;
    public float shieldMaxIncrease = 0.5f;

    public float SwordDamage => player != null ? player.danno : 0f;
    public float MoveSpeed => player != null ? player.speed : 0f;
    public float ShieldMax => shieldBar != null ? shieldBar.maxShield : 0f;
    public int PlayerLevel => player != null ? Mathf.FloorToInt(player.livello) : 0;
    public int CurrentXP => player != null ? Mathf.FloorToInt(player.XpAttuale) : 0;
    public int NextLevelXP => player != null ? Mathf.CeilToInt(player.XpProssimoLivello) : 0;

    private void Awake()
    {
        if(player == null)
            player = GetComponent<PlayerMovement>();

        if(shieldBar == null)
            shieldBar = GetComponentInChildren<ShieldBar>();
    }

    public bool UpgradeSword()
    {
        if(!CanUpgrade(swordLevel, maxSwordLevel) || player == null)
            return false;

        upgradePoints--;
        swordLevel++;
        player.danno += swordDamageIncrease;
        return true;
    }

    public bool UpgradeShield()
    {
        if(!CanUpgrade(shieldLevel, maxShieldLevel) || shieldBar == null)
            return false;

        upgradePoints--;
        shieldLevel++;
        shieldBar.IncreaseMaxShield(shieldMaxIncrease, true);
        return true;
    }

    private bool CanUpgrade(int currentLevel, int maxLevel)
    {
        return upgradePoints > 0 && currentLevel < maxLevel;
    }
}
