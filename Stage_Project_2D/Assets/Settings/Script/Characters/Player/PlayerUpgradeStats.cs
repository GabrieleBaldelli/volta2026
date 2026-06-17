using UnityEngine;

public class PlayerUpgradeStats : MonoBehaviour
{
    private static bool hasSavedProgress;
    private static int savedUpgradePoints;
    private static int savedSwordLevel;
    private static int savedShieldLevel;
    private static float savedHealth = PlayerMovement.vitaMassima;
    private static float savedCurrentShield;
    private bool saveProgressOnDisable = true;

    [Header("References")]
    // Riferimento al player: serve per modificare danno e leggere livello/XP.
    public PlayerMovement player;

    // Riferimento alla barra scudo: serve per aumentare lo scudo massimo.
    public ShieldBar shieldBar;

    [Header("Upgrade Points")]
    // Punti disponibili da spendere nei potenziamenti.
    // Vengono aumentati quando il player sale di livello.
    public int upgradePoints = 0;

    [Header("Sword")]
    // Livello e limite massimo del potenziamento della spada.
    public int swordLevel = 0;
    public int maxSwordLevel = 20;

    // Quanto danno viene aggiunto a ogni upgrade della spada.
    public float swordDamageIncrease = 0.5f;

    [Header("Shield")]
    // Livello e limite massimo del potenziamento dello scudo.
    public int shieldLevel = 0;
    public int maxShieldLevel = 20;

    // Quanto scudo massimo viene aggiunto a ogni upgrade.
    public float shieldMaxIncrease = 0.5f;

    // Valori letti dal menu upgrade per mostrare statistiche aggiornate.
    public float SwordDamage => player != null ? player.danno : 0f;
    public float MoveSpeed => player != null ? player.speed : 0f;
    public float ShieldMax => shieldBar != null ? shieldBar.maxShield : 0f;
    public int PlayerLevel => player != null ? Mathf.FloorToInt(player.livello) : 0;
    public int CurrentXP => player != null ? Mathf.FloorToInt(player.XpAttuale) : 0;
    public int NextLevelXP => player != null ? Mathf.CeilToInt(player.XpProssimoLivello) : 0;

    private void Awake()
    {
        // Se i riferimenti non sono assegnati dall'Inspector, prova a prenderli dal player.
        if(player == null)
            player = GetComponent<PlayerMovement>();

        if(shieldBar == null)
            shieldBar = GetComponentInChildren<ShieldBar>();

        LoadProgress();
    }

    private void OnDisable()
    {
        if(Application.isPlaying && saveProgressOnDisable)
            SaveProgress();
    }

    public bool UpgradeSword()
    {
        // Non potenzia se mancano punti, se la spada e' al massimo o se manca il player.
        if(!CanUpgrade(swordLevel, maxSwordLevel) || player == null)
            return false;

        // Consuma un punto, aumenta il livello e applica il danno extra.
        upgradePoints--;
        swordLevel++;
        player.danno += swordDamageIncrease;
        SaveProgress();
        return true;
    }

    public bool UpgradeShield()
    {
        // Non potenzia se mancano punti, se lo scudo e' al massimo o se manca la ShieldBar.
        if(!CanUpgrade(shieldLevel, maxShieldLevel) || shieldBar == null)
            return false;

        // Consuma un punto, aumenta il livello e aumenta lo scudo massimo.
        upgradePoints--;
        shieldLevel++;
        shieldBar.IncreaseMaxShield(shieldMaxIncrease, true);
        SaveProgress();
        return true;
    }

    public void SaveProgress()
    {
        savedUpgradePoints = upgradePoints;
        savedSwordLevel = swordLevel;
        savedShieldLevel = shieldLevel;

        if(player != null)
            savedHealth = player.Vita;

        if(shieldBar != null)
            savedCurrentShield = shieldBar.shieldSetGet;

        hasSavedProgress = true;
    }

    public void ResetProgress()
    {
        upgradePoints = 0;
        swordLevel = 0;
        shieldLevel = 0;

        savedUpgradePoints = 0;
        savedSwordLevel = 0;
        savedShieldLevel = 0;
        savedHealth = PlayerMovement.vitaMassima;
        savedCurrentShield = 0f;
        hasSavedProgress = false;
        saveProgressOnDisable = false;
    }

    private void LoadProgress()
    {
        if(!Application.isPlaying || !hasSavedProgress)
            return;

        upgradePoints = savedUpgradePoints;
        swordLevel = savedSwordLevel;
        shieldLevel = savedShieldLevel;

        if(player != null)
        {
            player.danno += swordLevel * swordDamageIncrease;
            player.Vita = Mathf.Clamp(savedHealth, 0f, PlayerMovement.vitaMassima);
            UpdatePlayerLifeBar();
        }

        if(shieldBar != null)
        {
            shieldBar.IncreaseMaxShield(shieldLevel * shieldMaxIncrease, true);
            shieldBar.SetShield(savedCurrentShield);
        }
    }

    private void UpdatePlayerLifeBar()
    {
        LifeBar lifeBar = GetComponentInChildren<LifeBar>();

        if(lifeBar != null && player != null)
            lifeBar.UpdateLifeBar(player.Vita, PlayerMovement.vitaMassima);
    }

    private bool CanUpgrade(int currentLevel, int maxLevel)
    {
        // Un upgrade e' valido solo se hai punti e non hai raggiunto il livello massimo.
        return upgradePoints > 0 && currentLevel < maxLevel;
    }
}
