using UnityEngine;

public class PlayerUpgradeStats : MonoBehaviour
{
    // Queste variabili statiche restano in memoria anche quando Unity cambia scena.
    // Per questo le usiamo per portare gli upgrade dalla Room 1 alla Room 2.
    // Non sono un salvataggio su file: se chiudi il gioco, questi valori si perdono.
    private static bool hasSavedProgress;
    private static int savedUpgradePoints;
    private static int savedSwordLevel;
    private static int savedShieldLevel;
    private static int savedHealthLevel;
    private static float savedXP;
    private static float savedPlayerLevel = 1f;
    private static float savedNextLevelMultiplier = 1f;
    private static float savedSwordDamage = 20f;
    private static float savedHealth = PlayerMovement.vitaMassimaIniziale;
    private static float savedMaxHealth = PlayerMovement.vitaMassimaIniziale;
    private static float savedCurrentShield;
    private static float savedMaxShield = 5f;

    // Serve per evitare che OnDisable risalvi i dati subito dopo un reset per morte.
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

    [Header("Health")]
    // Livello e limite massimo del potenziamento della vita.
    public int healthLevel = 0;
    public int maxHealthLevel = 20;

    // Quanta vita massima viene aggiunta a ogni upgrade.
    public float healthMaxIncrease = 5f;

    // Valori letti dal menu upgrade per mostrare statistiche aggiornate.
    public float SwordDamage => player != null ? player.danno : 0f;
    public float MoveSpeed => player != null ? player.speed : 0f;
    public float ShieldMax => shieldBar != null ? shieldBar.maxShield : 0f;
    public float HealthCurrent => player != null ? player.Vita : 0f;
    public float HealthMax => player != null ? player.VitaMassima : 0f;
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

        // Appena nasce il player in una nuova scena, ricarica gli upgrade salvati.
        LoadProgress();
    }

    private void OnDisable()
    {
        // Quando il player viene disattivato durante un cambio scena, salva i progressi attuali.
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
        shieldBar.IncreaseMaxShield(shieldMaxIncrease, false);
        SaveProgress();
        return true;
    }

    public bool UpgradeHealth()
    {
        // Non potenzia se mancano punti, se la vita e' al massimo o se manca il player.
        if(!CanUpgrade(healthLevel, maxHealthLevel) || player == null)
            return false;

        // Consuma un punto, aumenta il livello e aumenta la vita massima.
        upgradePoints--;
        healthLevel++;
        player.IncreaseMaxHealth(healthMaxIncrease, false);
        SaveProgress();
        return true;
    }

    public void SaveProgress()
    {
        // Salva i livelli degli upgrade e i punti ancora non spesi.
        savedUpgradePoints = upgradePoints;
        savedSwordLevel = swordLevel;
        savedShieldLevel = shieldLevel;
        savedHealthLevel = healthLevel;

        // Salva anche vita attuale e vita massima, cosi' il player mantiene i PV tra le room.
        if(player != null)
        {
            savedSwordDamage = player.danno;
            savedHealth = player.Vita;
            savedMaxHealth = player.VitaMassima;
            savedXP = player.XpAttuale;
            savedPlayerLevel = player.livello;
            savedNextLevelMultiplier = player.MoltiplicatoreLivelloSuccessivo;
        }

        // Salva lo scudo attuale, non solo il suo livello massimo.
        if(shieldBar != null)
        {
            savedCurrentShield = shieldBar.shieldSetGet;
            savedMaxShield = shieldBar.maxShield;
        }

        // Da questo momento LoadProgress sa che esistono dati da ricaricare.
        hasSavedProgress = true;
    }

    public void ResetProgress()
    {
        // Rimette a zero i valori dell'oggetto attuale.
        upgradePoints = 0;
        swordLevel = 0;
        shieldLevel = 0;
        healthLevel = 0;

        // Cancella anche i valori statici usati per passare da una scena all'altra.
        savedUpgradePoints = 0;
        savedSwordLevel = 0;
        savedShieldLevel = 0;
        savedHealthLevel = 0;
        savedXP = 0f;
        savedPlayerLevel = 1f;
        savedNextLevelMultiplier = 1f;
        savedSwordDamage = 20f;
        savedHealth = PlayerMovement.vitaMassimaIniziale;
        savedMaxHealth = PlayerMovement.vitaMassimaIniziale;
        savedCurrentShield = 0f;
        savedMaxShield = 5f;
        hasSavedProgress = false;

        // Dopo un reset non vogliamo che OnDisable salvi di nuovo i vecchi valori.
        saveProgressOnDisable = false;
    }

    private void LoadProgress()
    {
        // In editor o se non e' mai stato salvato nulla, non c'e' niente da ricaricare.
        if(!Application.isPlaying || !hasSavedProgress)
            return;

        // Ripristina punti e livelli degli upgrade.
        upgradePoints = savedUpgradePoints;
        swordLevel = savedSwordLevel;
        shieldLevel = savedShieldLevel;
        healthLevel = savedHealthLevel;

        if(player != null)
        {
            // Ripristina anche livello e XP, cosi' il menu in Room 2 mostra la progressione di Room 1.
            player.RestoreProgression(savedXP, savedPlayerLevel, savedNextLevelMultiplier);

            // Ripristina i valori reali salvati, senza sommare di nuovo i bonus.
            player.danno = savedSwordDamage;
            player.vitaMassima = savedMaxHealth;

            // Clamp evita che la vita caricata superi la nuova vita massima.
            player.Vita = Mathf.Clamp(savedHealth, 0f, player.VitaMassima);
            UpdatePlayerLifeBar();
        }

        if(shieldBar != null)
        {
            // Ripristina prima lo scudo massimo, poi rimette lo scudo attuale salvato.
            shieldBar.maxShield = savedMaxShield;
            shieldBar.SetShield(savedCurrentShield);
        }
    }

    private void UpdatePlayerLifeBar()
    {
        LifeBar lifeBar = GetComponentInChildren<LifeBar>();

        if(lifeBar != null && player != null)
            lifeBar.UpdateLifeBar(player.Vita, player.VitaMassima);
    }

    private bool CanUpgrade(int currentLevel, int maxLevel)
    {
        // Un upgrade e' valido solo se hai punti e non hai raggiunto il livello massimo.
        return upgradePoints > 0 && currentLevel < maxLevel;
    }
}
