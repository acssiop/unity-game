using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("波次基础")]
    public float waveDuration = 30f;            // 波次持续时长（秒）
    public int enemiesPerSpawn = 1;             // 每次生成几个敌人
    public float spawnInterval = 1.5f;          // 生成间隔（秒）

    [Header("敌人类型（可多选，随机抽）")]
    public GameObject[] enemyPrefabs;           // 可选敌人prefab数组

    [Header("难度递增（简单线性）")]
    public float enemySpeedMultiplier = 1f;     // 敌人移动速度倍率
    public float enemyHealthMultiplier = 1f;    // 敌人血量倍率
    public float enemyDamageMultiplier = 1f;    // 敌人伤害倍率
}
