using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance { get; private set; }

    [Header("经验值")]
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int currentLevel = 1;

    [Header("经验成长")]
    public int baseXP = 100;
    public int xpIncreasePerLevel = 50;

    public event Action<int> OnXPChanged;
    public event Action OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        xpToNextLevel = baseXP;
        OnXPChanged?.Invoke(currentXP);
    }

    public void AddXP(int amount)
    {
        // 应用经验倍率
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
            amount = Mathf.RoundToInt(amount * stats.expMultiplier);

        currentXP += amount;
        OnXPChanged?.Invoke(currentXP);
        Debug.Log($"获得经验 {amount}，当前 {currentXP}/{xpToNextLevel}");

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        xpToNextLevel = baseXP + (currentLevel - 1) * xpIncreasePerLevel;
        Debug.Log($"升级！当前等级 {currentLevel}，下阶段需要 {xpToNextLevel} 经验");
        OnLevelUp?.Invoke();
    }
}