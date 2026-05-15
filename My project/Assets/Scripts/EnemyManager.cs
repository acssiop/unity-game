using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public readonly List<Enemy> activeEnemies = new List<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            //Debug.Log($"[管理器] 注册敌人: {enemy.name}，当前总数: {activeEnemies.Count}");
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (activeEnemies.Remove(enemy)) ;
            //Debug.Log($"[管理器] 注销敌人: {enemy.name}，当前总数: {activeEnemies.Count}");
    }

    public Enemy GetNearestEnemy(Vector3 fromPosition, float maxRange)
    {
        Enemy nearest = null;
        float nearestSqrDist = maxRange * maxRange;

        // 倒序遍历，避免因列表删除导致的索引问题
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = activeEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                // 顺便清理无效引用
                activeEnemies.RemoveAt(i);
                continue;
            }

            float sqrDist = (enemy.transform.position - fromPosition).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearest = enemy;
            }
        }
        return nearest;
    }
}