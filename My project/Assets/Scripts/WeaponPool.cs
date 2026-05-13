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
    }

    Weapon CreateNew(WeaponConfig config)
    {
        var go = Instantiate(config.weaponPrefab);
        var weapon = go.GetComponent<Weapon>();
        weapon.config = config;
        // 绑定回收回调
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