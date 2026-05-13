using UnityEngine;
using System;                        

public class Weapon : MonoBehaviour
{
    public WeaponConfig config;
    public int currentLevel = 1;      // 1~4
    public WeaponLevelData Data => config.levels[currentLevel - 1];

    // 运行时状态
    public float attackTimer;
    public int comboCount;            // 用于地震锤计数等
    public float killStreakTimer;     // 用于毁灭者机枪

    // 对象池回收回调
    public System.Action<Weapon> onReturnToPool;

    public void ResetState()
    {
        attackTimer = 0;
        comboCount = 0;
        killStreakTimer = 0;
    }
}


