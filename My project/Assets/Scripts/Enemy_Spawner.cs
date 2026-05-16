using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Enemy_Spawner : MonoBehaviour
{
    [Header("地图边界")]
    public PlayAreaBounds playAreaBounds;

    [Header("敌人预制体")]
    public GameObject normalEnemyPrefab;
    public GameObject bossEnemyPrefab;              // 第20波Boss

    [Header("对象池预热数量")]
    public int normalPoolSize = 50;
    public int bossPoolSize = 10;

    [Header("波次参数")]
    public float baseWaveDuration = 20f;
    public float waveIncrement = 5f;
    public float maxWaveDuration = 60f;
    public int totalWaves = 20;
    public int bossWave = 20;
    public float spawnInterval = 2f;
    public int maxEnemiesOnField = 100;

    [Header("地图边界（XZ 平面）")]
    [Tooltip("地面高度（Y 坐标）")]
    public float groundHeight = 0f;
    [Tooltip("边缘外侧偏移距离")]
    public float edgeOffset = 1f;

    [Header("UI")]
    public GameObject shopPanel;

    private enum WaveState { Fighting, Shop }
    private WaveState currentState = WaveState.Fighting;

    private int currentWave = 1;
    private float waveTimer;
    private float nextSpawnTime;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private bool isGameOver = false;

    private Dictionary<GameObject, Queue<GameObject>> pool =
        new Dictionary<GameObject, Queue<GameObject>>();

    private void Start()
    {

        // 预热对象池
        PreparePool(normalEnemyPrefab, normalPoolSize);
        if (bossEnemyPrefab != null)
            PreparePool(bossEnemyPrefab, bossPoolSize);

        if (shopPanel != null) shopPanel.SetActive(false);
        StartWave(1);
    }

    private void Update()
    {
        if (isGameOver) return;
        if (currentState != WaveState.Fighting) return;

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            EndWave();
            return;
        }

        while (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime += spawnInterval;
        }

        if (activeEnemies.Count > maxEnemiesOnField)
        {
            RemoveRandomEnemy();
        }
    }

    // ---------- 对象池 ----------
    private void PreparePool(GameObject prefab, int count)
    {
        if (prefab == null) return;

        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool[prefab].Enqueue(obj);
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        GameObject obj;
        if (pool[prefab].Count > 0)
            obj = pool[prefab].Dequeue();
        else
            obj = Instantiate(prefab);

        obj.SetActive(true);
        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        Enemy enemy = obj.GetComponent<Enemy>();
        GameObject prefab = enemy != null ? enemy.PrefabReference : null;

        obj.SetActive(false);

        if (prefab != null && pool.ContainsKey(prefab))
        {
            pool[prefab].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("归还敌人时找不到对应预制体，已销毁实例。");
            Destroy(obj);
        }
    }

    // ---------- 波次控制 ----------
    public void StartWave(int wave)
    {
        currentWave = wave;
        float duration = baseWaveDuration + (wave - 1) * waveIncrement;
        waveTimer = Mathf.Min(duration, maxWaveDuration);
        nextSpawnTime = Time.time + spawnInterval;

        currentState = WaveState.Fighting;
        Time.timeScale = 1f;

        Debug.Log($"第 {currentWave} 波开始，时长 {waveTimer:F1} 秒");
    }

    private void EndWave()
    {
        if (isGameOver) return;

        // --- 新增：收集所有未拾取的金币 ---
        if (CoinManager.Instance != null)
            CoinManager.Instance.CollectAllCoins();
        // --- 新增结束 ---

        // 原有清空敌人逻辑...
        while (activeEnemies.Count > 0)
        {
            GameObject enemy = activeEnemies[0];
            activeEnemies.RemoveAt(0);
            if (enemy != null)
                ReturnToPool(enemy);
        }

        Time.timeScale = 0f;

        if (shopPanel != null && !shopPanel.activeSelf)
        {
            shopPanel.SetActive(true);
        }

        currentState = WaveState.Shop;
        Debug.Log($"第 {currentWave} 波结束，进入商店");
    }

    public void StartNextWave()
    {
        if (currentState != WaveState.Shop) return;

        if (shopPanel != null) shopPanel.SetActive(false);
        Time.timeScale = 1f;
        currentWave++;

        if (currentWave > totalWaves)
        {
            Debug.Log("全部波次完成！");
            return;
        }

        StartWave(currentWave);
    }

    // ---------- 敌人管理 ----------
    private void SpawnEnemy()
    {
        GameObject prefab = (currentWave == bossWave && bossEnemyPrefab != null)
            ? bossEnemyPrefab : normalEnemyPrefab;

        if (prefab == null)
        {
            Debug.LogError("敌人预制体未指定！");
            return;
        }

        GameObject enemy = GetFromPool(prefab);
        Enemy enemyComp = enemy.GetComponent<Enemy>();
        if (enemyComp != null)
            enemyComp.Init(this, prefab);

        Vector3 edgePos = GetRandomEdgePosition();
        float yOffset = enemyComp != null ? enemyComp.HeightOffset : 0f;
        enemy.transform.position =
    new Vector3(edgePos.x, groundHeight + yOffset, edgePos.z);

        // 朝向地图中心
        Vector3 center = new Vector3(
    (playAreaBounds.MinX + playAreaBounds.MaxX) * 0.5f,
    groundHeight,
    (playAreaBounds.MinZ + playAreaBounds.MaxZ) * 0.5f
 );

        Vector3 dir = center - enemy.transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.LookRotation(dir);
        }

        activeEnemies.Add(enemy);
    }

    private void RemoveRandomEnemy()
    {
        if (activeEnemies.Count == 0) return;

        int index = Random.Range(0, activeEnemies.Count);
        GameObject target = activeEnemies[index];
        activeEnemies.RemoveAt(index);

        if (target != null)
            ReturnToPool(target);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    /// <summary>
    /// 玩家死亡时调用，立刻清空所有活跃敌人并停止计时
    /// </summary>
    public void ClearAllEnemies()
    {
        // 标记游戏结束（阻止以后的所有行为）
        isGameOver = true;

        // 清空场上所有敌人
        var snapshot = new List<GameObject>(activeEnemies);
        foreach (var enemy in snapshot)
        {
            if (enemy != null)
                enemy.SetActive(false);   // 通过 OnDisable 自动回池、注销
        }
        activeEnemies.Clear();

        // 关闭可能已经打开的商店面板
        if (shopPanel != null) shopPanel.SetActive(false);

        // 恢复正常时间流速
        Time.timeScale = 1f;
    }

    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);
        float x, z;

        switch (side)
        {
            case 0:
                x = Random.Range(playAreaBounds.MinX, playAreaBounds.MaxX);
                z = playAreaBounds.MaxZ + edgeOffset;
                break;

            case 1:
                x = Random.Range(playAreaBounds.MinX, playAreaBounds.MaxX);
                z = playAreaBounds.MinZ - edgeOffset;
                break;

            case 2:
                x = playAreaBounds.MinX - edgeOffset;
                z = Random.Range(playAreaBounds.MinZ, playAreaBounds.MaxZ);
                break;

            default:
                x = playAreaBounds.MaxX + edgeOffset;
                z = Random.Range(playAreaBounds.MinZ, playAreaBounds.MaxZ);
                break;
        }

        return new Vector3(x, 0, z);
    }
}