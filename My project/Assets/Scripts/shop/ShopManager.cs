using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject shopPanel;
    public Transform cardContainer;
    public GameObject cardPrefab;
    public Button refreshButton;
    public UnityEngine.UI.Text refreshCostText;          // ← 现在可以正常拖拽了
    public Button startWaveButton;

    [Header("卡池")]
    public List<UpgradeData> cardPool;

    [Header("刷新费用")]
    public int baseRefreshCost = 10;
    public int refreshCostIncrement = 10;
    public int maxRefreshCost = 50;
    private int refreshCount = 0;

    [Header("手动布局（卡片）")]
    public float cardSpacing = 20f;          // 卡片间距
    public float cardStartX = 0f;           // 首张卡片 X 起始（相对于父级中心）

    private List<UpgradeData> currentCards = new List<UpgradeData>();
    private List<GameObject> spawnedCards = new List<GameObject>();

    void Start()
    {
        shopPanel.SetActive(false);
        refreshButton.onClick.AddListener(RefreshCards);
        startWaveButton.onClick.AddListener(StartNextWave);
    }

    void OnEnable()  // 当 shopPanel 激活时
    {
        if (shopPanel.activeSelf)
        {
            refreshCount = 0;
            GenerateCards();
        }
    }

    /// <summary>
    /// 手动水平排列当前生成的所有卡片
    /// </summary>
    private void LayoutCards()
    {
        if (spawnedCards.Count == 0) return;

        // 获取第一张卡片的宽度（假设所有卡片宽高一致）
        RectTransform firstRT = spawnedCards[0].GetComponent<RectTransform>();
        float cardWidth = firstRT.rect.width;
        float cardHeight = firstRT.rect.height;

        // 总宽度
        float totalWidth = spawnedCards.Count * cardWidth + (spawnedCards.Count - 1) * cardSpacing;

        // 从起始 X 开始排列（这里以 CardContainer 的中心为基准，首张卡片靠左）
        float startX = -totalWidth / 2f + cardWidth / 2f;   // 让整体居中

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            RectTransform rt = spawnedCards[i].GetComponent<RectTransform>();
            if (rt == null) continue;

            // 设置锚点为居中，方便直接通过 anchoredPosition 控制
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float xPos = startX + i * (cardWidth + cardSpacing);
            rt.anchoredPosition = new Vector2(xPos, 0);
        }
    }

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

        // *** 按价格升序排序 ***
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

    public void PurchaseCard(CardUI cardUI, UpgradeData data)
    {
        if (PlayerGold.Instance == null || !PlayerGold.Instance.SpendGold(data.baseCost))
            return;

        ApplyUpgrade(data);

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
                if (hp != null) hp.IncreaseMaxHealth(25);
                break;
            case UpgradeType.HealthRegen:
                stats.healthRegen += 1;
                break;
        }
    }

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

    void StartNextWave()
    {
        Enemy_Spawner spawner = FindObjectOfType<Enemy_Spawner>();
        if (spawner != null) spawner.StartNextWave();
        shopPanel.SetActive(false);
    }
}