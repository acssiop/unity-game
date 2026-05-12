using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;           // 池子标签（例如 "Bullet"）
        public GameObject prefab;    // 子弹预制体
        public int size = 20;       // 初始池大小
    }

    public static ObjectPool Instance; // 单例方便全局调用

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化所有池子
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// 从池中获取一个可用对象
    /// </summary>
    public GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"对象池中没有标签为 {tag} 的池子");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 池中无可用对象时动态扩展（可选）
            Pool poolData = pools.Find(p => p.tag == tag);
            if (poolData != null)
            {
                GameObject obj = Instantiate(poolData.prefab);
                obj.SetActive(true);
                Debug.LogWarning($"对象池 {tag} 容量不足，动态生成了新对象");
                return obj;
            }
        }
        return null;
    }

    /// <summary>
    /// 将对象返还池子
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag)) return;

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}