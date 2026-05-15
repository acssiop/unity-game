using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class WeaponController : MonoBehaviour
{
    [Header("子弹池")]
    public ProjectilePool projectilePool;

    [Header("攻击参数")]
    public float attackInterval = 0.5f;
    public float attackRange = 10f;
    public float damage = 20f;
    public float projectileSpeed = 12f;
    public float maxTravelDistance = 15f;

    private float attackTimer;

    private void Update()
    {
        if (GetComponent<Player>() != null && GetComponent<Player>().IsDead)
            return;
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            // 获取敌人数量（安全判空）
            int enemyCount = EnemyManager.Instance != null ? EnemyManager.Instance.activeEnemies.Count : -1;
            //Debug.Log($"[武器] 攻击间隔触发 | 敌人总数: {enemyCount}");

            if (EnemyManager.Instance == null)
            {
                //Debug.LogError("[武器] EnemyManager.Instance 为空！");
            }
            else
            {
                Enemy nearest = EnemyManager.Instance.GetNearestEnemy(transform.position, attackRange);
                if (nearest != null)
                {
                    //Debug.Log($"[武器] 找到敌人 {nearest.name}，发射子弹");
                    var proj = projectilePool.GetProjectile();
                    proj.transform.SetParent(null);
                    proj.transform.position = transform.position;
                    proj.Target = nearest.transform;
                    proj.Damage = damage;
                    proj.Speed = projectileSpeed;
                    proj.MaxTravelDistance = maxTravelDistance;
                }
                else
                {
                    //Debug.Log("[武器] 未找到敌人在攻击范围内");
                }
            }

            attackTimer = attackInterval;
        }
    }
}