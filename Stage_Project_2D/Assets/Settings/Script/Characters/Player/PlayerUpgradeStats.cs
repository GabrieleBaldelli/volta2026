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
    public int maxSwordLevel = 5;
    public float swordDamageIncrease = 5f;

    [Header("Shield")]
    public int shieldLevel = 1;
    public int maxShieldLevel = 5;
    public float shieldMaxIncrease = 1f;

    [Header("Character")]
    public int speedLevel = 1;
    public int maxSpeedLevel = 5;
    public float speedIncrease = 0.5f;

    public float SwordDamage => player != null ? player.danno : 0f;
    public float MoveSpeed => player != null ? player.speed : 0f;
    public float ShieldMax => shieldBar != null ? shieldBar.maxShield : 0f;
    public int PlayerLevel => player != null ? Mathf.FloorToInt(player.livello) : 0;

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

    public bool UpgradeSpeed()
    {
        if(!CanUpgrade(speedLevel, maxSpeedLevel) || player == null)
            return false;

        upgradePoints--;
        speedLevel++;
        player.speed += speedIncrease;
        return true;
    }

    private bool CanUpgrade(int currentLevel, int maxLevel)
    {
        return upgradePoints > 0 && currentLevel < maxLevel;
    }
}
