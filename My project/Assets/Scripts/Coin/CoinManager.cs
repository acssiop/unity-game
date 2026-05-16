using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("预制体")]
    public GameObject coinPrefab;
    public int poolSize = 30;

    [Header("运动参数")]
    public float moveSpeed = 5f;
    public float pickUpRadius = 3f;
    public float collectDistance = 0.3f;

    private Queue<GameObject> coinPool = new Queue<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("CoinManager Awake");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("CoinManager 找到玩家 Transform");
        }
        else
        {
            Debug.LogError("CoinManager 找不到 Tag=Player 的对象！");
        }

        for (int i = 0; i < poolSize; i++)
            CreateCoin();
    }

    private GameObject CreateCoin()
    {
        GameObject coin = Instantiate(coinPrefab, transform);
        coin.SetActive(false);
        coinPool.Enqueue(coin);
        return coin;
    }

    public void SpawnCoin(Vector3 position, int value)
    {
        if (coinPrefab == null)
        {
            Debug.LogError("CoinManager：coinPrefab 未设置！");
            return;
        }
        if (playerTransform == null)
        {
            Debug.LogError("CoinManager：playerTransform 为空，无法生成金币！");
            return;
        }
        if (coinPool.Count == 0) CreateCoin();

        GameObject coin = coinPool.Dequeue();
        // 添加随机偏移，避免金币完全重叠
        Vector3 randomOffset = new Vector3(
            Random.Range(-1.5f, 1.5f),
            0.04f,
            Random.Range(-1.5f, 1.5f)
        );
        coin.transform.position = position + randomOffset;
        coin.SetActive(true);
        activeCoins.Add(coin);

        Coin coinScript = coin.GetComponent<Coin>();
        if (coinScript != null)
            coinScript.goldValue = value;
        else
            Debug.LogError("金币预制体缺少 Coin 脚本！");

        Debug.Log($"SpawnCoin 生成金币，位置 {position}，价值 {value}，活跃数量 {activeCoins.Count}");
    }

    private void Update()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            if (activeCoins.Count > 0) CollectAllCoins();
            return;
        }

        Vector3 playerPos = playerTransform.position;

        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = activeCoins[i];
            if (coin == null || !coin.activeInHierarchy)
            {
                activeCoins.RemoveAt(i);
                continue;
            }

            Vector3 direction = playerPos - coin.transform.position;
            float distance = direction.magnitude;

            if (distance <= pickUpRadius)
            {
                if (distance <= collectDistance)
                {
                    Debug.Log($"进入收集距离，收集金币 index={i}");
                    CollectCoin(coin, i);
                }
                else
                {
                    coin.transform.position += direction.normalized * moveSpeed * Time.deltaTime;
                }
            }
        }
    }

    private void CollectCoin(GameObject coin, int index)
    {
        Debug.Log("CollectCoin 开始执行");
        Coin coinScript = coin.GetComponent<Coin>();
        if (coinScript == null)
        {
            Debug.LogError("CollectCoin：金币上没有 Coin 脚本！");
            ReturnCoin(coin, index);
            return;
        }

        int value = coinScript.goldValue;
        Debug.Log($"将要添加金币：{value}");

        // 直接使用静态实例，确保可靠
        if (PlayerGold.Instance != null)
        {
            PlayerGold.Instance.AddGold(value);
            Debug.Log($"成功调用 AddGold({value})");
        }
        else
        {
            Debug.LogError("PlayerGold.Instance 为 null！请检查 Player 上是否挂载 PlayerGold 脚本。");
        }

        ReturnCoin(coin, index);
    }

    private void ReturnCoin(GameObject coin, int index)
    {
        coin.SetActive(false);
        if (index >= 0 && index < activeCoins.Count && activeCoins[index] == coin)
            activeCoins.RemoveAt(index);
        coinPool.Enqueue(coin);
        Debug.Log($"ReturnCoin 完成，当前活跃金币 {activeCoins.Count}");
    }

    public void CollectAllCoins()
    {
        Debug.Log("CollectAllCoins 调用");
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = activeCoins[i];
            if (coin == null || !coin.activeInHierarchy)
            {
                activeCoins.RemoveAt(i);
                continue;
            }
            CollectCoin(coin, i);
        }
        activeCoins.Clear();
    }
}