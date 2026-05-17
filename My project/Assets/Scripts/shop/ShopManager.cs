using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;
using Debug = UnityEngine.Debug;

public class ShopManager : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject shopPanel;
    public Transform cardContainer;
    public GameObject cardPrefab;
    public Button refreshButton;
    public Text refreshCostText;                // 刷新费用文本
    public Button startWaveButton;

    [Header("卡池")]
    public List<UpgradeData> cardPool;

    [Header("刷新费用")]
    public int baseRefreshCost = 10;
    public int refreshCostIncrement = 10;
    public int maxRefreshCost = 50;
    private int refreshCount = 0;

    [Header("手动布局（卡片）")]
    public float cardSpacing = 20f;
    public float cardStartX = 0f;

    private List<UpgradeData> currentCards = new List<UpgradeData>();
    private List<GameObject> spawnedCards = new List<GameObject>();

    [Header("武器展示区")]
    public Transform weaponSlotsContainer;       // 武器卡槽父物体
    public GameObject weaponSlotPrefab;          // 武器展示卡片预制体

    // ========== 初始化 ==========
    void Start()
    {
        shopPanel.SetActive(false);
        refreshButton.onClick.AddListener(RefreshCards);
        startWaveButton.onClick.AddListener(StartNextWave);
    }

    void OnEnable()
    {
        if (shopPanel.activeSelf)
        {
            refreshCount = 0;
            GenerateCards();
            RefreshWeaponDisplay();              // 显示当前武器
        }
    }

    // ========== 卡片生成与布局 ==========
    void GenerateCards()
    {
        ClearCards();
        List<UpgradeData> pool = new List<UpgradeData>(cardPool);
        currentCards.Clear();

        // 随机抽取3张
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            currentCards.Add(pool[index]);
            pool.RemoveAt(index);
        }

        // 按价格升序排序
        currentCards.Sort((a, b) => a.baseCost.CompareTo(b.baseCost));

        // 生成 UI 实例
        foreach (UpgradeData data in currentCards)
        {
            GameObject go = Instantiate(cardPrefab, cardContainer);
            CardUI cardUI = go.GetComponent<CardUI>();
            if (cardUI != null) cardUI.Setup(data, this);
            spawnedCards.Add(go);
        }

        // 手动排列位置
        LayoutCards();
        UpdateRefreshCostDisplay();
    }

    void ClearCards()
    {
        foreach (GameObject go in spawnedCards) Destroy(go);
        spawnedCards.Clear();
        currentCards.Clear();
    }

    /// <summary> 手动水平排列卡片（居中） </summary>
    private void LayoutCards()
    {
        if (spawnedCards.Count == 0) return;

        RectTransform firstRT = spawnedCards[0].GetComponent<RectTransform>();
        float cardWidth = firstRT.rect.width;
        float totalWidth = spawnedCards.Count * cardWidth + (spawnedCards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            RectTransform rt = spawnedCards[i].GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float xPos = startX + i * (cardWidth + cardSpacing);
            rt.anchoredPosition = new Vector2(xPos, 0);
        }
    }

    // ========== 购买逻辑 ==========
    public void PurchaseCard(CardUI cardUI, UpgradeData data)
    {
        // 扣除金币
        if (PlayerGold.Instance == null || !PlayerGold.Instance.SpendGold(data.baseCost))
            return;

        bool purchaseSuccess = true;

        switch (data.type)
        {
            // 属性卡
            case UpgradeType.AttackDamage:
            case UpgradeType.AttackSpeed:
            case UpgradeType.MoveSpeed:
            case UpgradeType.MaxHealth:
            case UpgradeType.HealthRegen:
                ApplyUpgrade(data);
                break;

            // 武器卡
            case UpgradeType.WeaponCardWhite:
            case UpgradeType.WeaponCardGreen:
            case UpgradeType.WeaponCardBlue:
            case UpgradeType.WeaponCardPurple:
                if (data.weaponData != null)
                {
                    bool added = WeaponManager.Instance.AddWeapon(data.weaponData);
                    if (added)
                        RefreshWeaponDisplay();
                    else
                    {
                        Debug.Log("添加武器失败（槽满或合成失败）");
                        purchaseSuccess = false;
                    }
                }
                else
                {
                    Debug.LogError("武器卡缺少 WeaponData 引用！");
                    purchaseSuccess = false;
                }
                break;

            // 出售武器（出售第一个）
            case UpgradeType.SellWeapon:
                if (WeaponManager.Instance.weapons.Count > 0)
                {
                    WeaponData first = WeaponManager.Instance.weapons[0].data;
                    SellPlayerWeapon(first);
                }
                else
                {
                    Debug.Log("没有武器可出售");
                    purchaseSuccess = false;
                }
                break;

            // 变异卡（暂未实现）
            case UpgradeType.WeaponMutation:
                Debug.Log("变异卡待实现");
                purchaseSuccess = false;
                break;

            default:
                Debug.LogWarning($"未处理的卡片类型: {data.type}");
                purchaseSuccess = false;
                break;
        }

        // 购买成功：移除卡片并刷新其他卡片的可购买状态
        if (purchaseSuccess)
        {
            int idx = currentCards.IndexOf(data);
            if (idx != -1)
            {
                currentCards.RemoveAt(idx);
                Destroy(spawnedCards[idx]);
                spawnedCards.RemoveAt(idx);
            }
            foreach (var go in spawnedCards)
            {
                CardUI ui = go.GetComponent<CardUI>();
                if (ui != null) ui.UpdateInteractable();
            }
        }
        else
        {
            // 失败则退还金币
            PlayerGold.Instance.AddGold(data.baseCost);
        }
    }

    void ApplyUpgrade(UpgradeData data)
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats == null) return;

        switch (data.type)
        {
            case UpgradeType.AttackDamage: stats.attackMultiplier += 0.1f; break;
            case UpgradeType.AttackSpeed: stats.attackSpeedMultiplier += 0.05f; break;
            case UpgradeType.MoveSpeed: stats.moveSpeedMultiplier += 0.05f; break;
            case UpgradeType.MaxHealth:
                Health hp = stats.GetComponent<Health>();
                hp?.IncreaseMaxHealth(25);
                break;
            case UpgradeType.HealthRegen: stats.healthRegen += 1; break;
                // 其他属性可根据枚举扩展
        }
    }

    // ========== 刷新 ==========
    void RefreshCards()
    {
        int cost = GetRefreshCost();
        if (PlayerGold.Instance == null || !PlayerGold.Instance.SpendGold(cost))
            return;

        refreshCount++;
        GenerateCards();
    }

    int GetRefreshCost()
    {
        int cost = baseRefreshCost + refreshCount * refreshCostIncrement;
        return Mathf.Min(cost, maxRefreshCost);
    }

    void UpdateRefreshCostDisplay()
    {
        if (refreshCostText != null)
            refreshCostText.text = $"刷新 ({GetRefreshCost()} G)";
    }

    // ========== 开始下一波 ==========
    void StartNextWave()
    {
        Enemy_Spawner spawner = FindObjectOfType<Enemy_Spawner>();
        if (spawner != null) spawner.StartNextWave();
        shopPanel.SetActive(false);
    }

    // ========== 武器展示区 ==========
    public void RefreshWeaponDisplay()
    {
        // 清除旧卡片
        foreach (Transform child in weaponSlotsContainer)
            Destroy(child.gameObject);

        if (WeaponManager.Instance == null) return;

        foreach (WeaponManager.WeaponInstance wep in WeaponManager.Instance.weapons)
        {
            if (wep.data == null) continue;
            GameObject slot = Instantiate(weaponSlotPrefab, weaponSlotsContainer);
            WeaponSlotUI slotUI = slot.GetComponent<WeaponSlotUI>();
            if (slotUI != null)
                slotUI.Setup(wep.data, this);
        }
    }

    public void SellPlayerWeapon(WeaponData data)
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.SellWeapon(data);
            RefreshWeaponDisplay();
        }
    }
}