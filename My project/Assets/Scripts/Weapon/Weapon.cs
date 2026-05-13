using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("配置")]
    public WeaponConfig config;                 // 武器静态数据（ScriptableObject）
    public int currentLevel = 1;                // 当前等级 I~IV (1~4)

    // 快速访问当前等级数据
    public WeaponLevelData Data => config.levels[currentLevel - 1];

    [Header("漂浮跟随状态 (由 PlayerWeaponManager 维护)")]
    [HideInInspector] public Vector3 currentVelocity;    // 用于 SmoothDamp 的速度缓冲
    [HideInInspector] public Vector3 targetPosition;     // 每帧计算的目标世界坐标

    // 对象池回收回调
    [HideInInspector] public System.Action<Weapon> onReturnToPool;

    // 攻击相关状态 (预留)
    public float attackTimer;
    public int comboCount;
    public float killStreakTimer;
    public Transform currentTarget;     // 索敌目标 (未来用)

    /// <summary>
    /// 重置所有运行时状态（对象池取出时调用）
    /// </summary>
    public void ResetState()
    {
        attackTimer = 0f;
        comboCount = 0;
        killStreakTimer = 0f;
        currentTarget = null;
        currentVelocity = Vector3.zero;
        targetPosition = transform.position;
    }

    // 未来可在此添加武器开火逻辑，由 PlayerWeaponManager 或自身 Update 驱动
}