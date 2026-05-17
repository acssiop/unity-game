using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class LevelUpManager : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject levelUpPanel;
    public Transform cardContainer;
    public GameObject cardPrefab;

    [Header("升级池")]
    public List<UpgradeData> levelUpPool;

    private void Start()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (PlayerExperience.Instance != null)
        {
            PlayerExperience.Instance.OnLevelUp += ShowLevelUp;
            Debug.Log("LevelUpManager 已订阅 OnLevelUp 事件");
        }
        else
        {
            Debug.LogError("LevelUpManager 找不到 PlayerExperience.Instance！请确保玩家挂载了 PlayerExperience 脚本。");
        }
    }

    private void ShowLevelUp()
    {
        Debug.Log("ShowLevelUp 被调用！");
        if (levelUpPanel == null)
        {
            Debug.LogError("levelUpPanel 未拖拽！");
            return;
        }
        if (cardContainer == null)
        {
            Debug.LogError("cardContainer 未拖拽！");
            return;
        }
        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab 未拖拽！");
            return;
        }

        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);   // 强制激活

        // 从池中随机抽取 3 张
        List<UpgradeData> pool = new List<UpgradeData>(levelUpPool);
        List<UpgradeData> chosen = new List<UpgradeData>();
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            chosen.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        Debug.Log($"抽取到 {chosen.Count} 张卡片");

        // 立即清空所有旧卡片（不延迟）
        while (cardContainer.childCount > 0)
        {
            DestroyImmediate(cardContainer.GetChild(0).gameObject);
        }
        // 生成卡片
        foreach (UpgradeData data in chosen)
        {
            GameObject go = Instantiate(cardPrefab, cardContainer);
            CardUI cardUI = go.GetComponent<CardUI>();
            Button btn = go.GetComponent<Button>();
            if (cardUI != null && btn != null)
            {
                // 手动设置文本，避免 ShopManager 干扰
                cardUI.nameText.text = data.upgradeName;
                cardUI.descText.text = data.description;
                cardUI.costText.text = "免费";
                cardUI.buyButton.interactable = true;

                // 清除原有监听，绑定升级选择
                btn.onClick.RemoveAllListeners();
                UpgradeData captured = data;   // 避免闭包问题
                btn.onClick.AddListener(() => SelectUpgrade(captured));

                Debug.Log($"生成卡片: {data.upgradeName}");
            }
            else
            {
                Debug.LogError("卡片预制体缺少 CardUI 或 Button 组件！");
            }
        }

        // 清除原有强制重建布局，改为手动排列
        // LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer as RectTransform);

        // 手动计算并设置每张卡片的位置
        float cardSpacing = 30f;   // 卡片间距
        float cardWidth = 200f;    // 每张卡片的固定宽度（与预制体一致）
        int cardCount = cardContainer.childCount;

        // 总宽度 = 卡片宽度*数量 + 间距*(数量-1)
        float totalWidth = cardCount * cardWidth + (cardCount - 1) * cardSpacing;
        float startX = -totalWidth / 2f + cardWidth / 2f; // 左边缘起始位置（相对于容器中心）

        for (int i = 0; i < cardCount; i++)
        {
            Transform child = cardContainer.GetChild(i);
            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 强制设置锚点与轴心，方便绝对定位
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y); // 固定宽度
                float xPos = startX + i * (cardWidth + cardSpacing);
                rt.anchoredPosition = new Vector2(xPos, 0);
            }
        }
    }

    private void SelectUpgrade(UpgradeData data)
    {
        ApplyUpgrade(data);
        Time.timeScale = 1f;
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
        Debug.Log($"选择升级: {data.upgradeName}");
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats == null)
        {
            Debug.LogError("找不到 PlayerStats 组件！");
            return;
        }

        switch (data.type)
        {
            case UpgradeType.AttackDamage: stats.attackMultiplier += 0.2f; break;
            case UpgradeType.AttackSpeed: stats.attackSpeedMultiplier += 0.15f; break;
            case UpgradeType.MaxHealth:
                Health hp = stats.GetComponent<Health>();
                hp?.IncreaseMaxHealth(30);
                break;
            case UpgradeType.CritChance: stats.critChance += 0.05f; break;
            case UpgradeType.DodgeChance: stats.dodgeChance += 0.05f; break;
            case UpgradeType.ExpGain: stats.expMultiplier += 0.1f; break;
        }
    }

    private void OnDestroy()
    {
        if (PlayerExperience.Instance != null)
            PlayerExperience.Instance.OnLevelUp -= ShowLevelUp;
    }
}