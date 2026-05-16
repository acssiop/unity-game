using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class PlayerGold : MonoBehaviour
{
    public static PlayerGold Instance { get; private set; }

    [Header("当前金币")]
    public int currentGold = 0;

    // 金币变化事件，供 UI 更新
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 可选：跨场景保留
        // DontDestroyOnLoad(gameObject);
        Debug.Log("PlayerGold Awake - Instance 已设置");
    }

    private void Start()
    {
        // 初始化 UI 显示
        OnGoldChanged?.Invoke(currentGold);
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"获得金币 {amount}，当前总额: {currentGold}");
        OnGoldChanged?.Invoke(currentGold);
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }
        return false;
    }
}