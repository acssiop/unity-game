using System.Collections.Generic;
using UnityEngine;

public class WeaponPool : MonoBehaviour
{
    // 静态单例，全局访问点
    public static WeaponPool Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry
    {
        public WeaponConfig config;
        public int prewarmCount = 2;
    }

    public List<PoolEntry> entries = new List<PoolEntry>();
    private Dictionary<WeaponConfig, Queue<Weapon>> poolDict;

    void Awake()
    {

        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 构建对象池
        poolDict = new Dictionary<WeaponConfig, Queue<Weapon>>();
        foreach (var entry in entries)
        {
            var queue = new Queue<Weapon>();
            for (int i = 0; i < entry.prewarmCount; i++)
            {
                var weapon = CreateNew(entry.config);
                weapon.gameObject.SetActive(false);
                queue.Enqueue(weapon);
            }
            poolDict[entry.config] = queue;
        }
        foreach (var entry in entries)
        {
            // 如果 config 无效，直接跳过该条目
            if (entry.config == null)
            {
                Debug.LogWarning("PoolEntry 的 config 为空，已跳过", this);
                continue;
            }

            var queue = new Queue<Weapon>();
            for (int i = 0; i < entry.prewarmCount; i++)
            {
                var weapon = CreateNew(entry.config);
                if (weapon == null)  // CreateNew 可能因为 prefab 为空等原因返回 null
                {
                    Debug.LogError($"无法预创建武器 '{entry.config.name}'，请检查 prefab", this);
                    break;  // 跳出内层循环，该条目的预创建提前结束
                }
                weapon.gameObject.SetActive(false);
                queue.Enqueue(weapon);
            }
            poolDict[entry.config] = queue;
        }
    }

    Weapon CreateNew(WeaponConfig config)
    {
        if (config == null)
        {
            Debug.LogError("CreateNew 收到 null WeaponConfig", this);
            return null;
        }
        if (config.weaponPrefab == null)
        {
            Debug.LogError($"WeaponConfig '{config.name}' 的 weaponPrefab 未赋值", this);
            return null;
        }

        var go = Instantiate(config.weaponPrefab);
        var weapon = go.GetComponent<Weapon>();
        weapon.config = config;
        weapon.onReturnToPool = ReturnWeapon;
        return weapon;
    }

    public Weapon Get(WeaponConfig config, int level)
    {
        if (!poolDict.ContainsKey(config))
            poolDict[config] = new Queue<Weapon>();

        var queue = poolDict[config];
        Weapon weapon;
        if (queue.Count > 0)
        {
            weapon = queue.Dequeue();
            weapon.gameObject.SetActive(true);
        }
        else
        {
            weapon = CreateNew(config);
        }

        weapon.currentLevel = level;
        weapon.ResetState();
        return weapon;
    }

    public void ReturnWeapon(Weapon weapon)
    {
        weapon.transform.SetParent(null);
        weapon.gameObject.SetActive(false);
        poolDict[weapon.config].Enqueue(weapon);
    }
}