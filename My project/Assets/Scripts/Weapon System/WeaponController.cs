using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("子弹池")]
    public ProjectilePool projectilePool;

    [Header("攻击参数")]
    public float attackInterval = 0.5f;
    public float attackRange = 10f;
    public float damage = 20f;
    public float projectileSpeed = 12f;
    public float maxTravelDistance = 25f;

    private float attackTimer;

    private void Update()
    {
        // 死亡检测（如果玩家挂载了Player脚本）
        Player player = GetComponent<Player>();
        if (player != null && player.IsDead) return;

        float effectiveInterval = attackInterval;
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
            effectiveInterval /= stats.attackSpeedMultiplier;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            AttackNearestEnemy();
            attackTimer = effectiveInterval;
        }
    }

    private void AttackNearestEnemy()
    {
        if (EnemyManager.Instance == null) return;

        Enemy nearest = EnemyManager.Instance.GetNearestEnemy(transform.position, attackRange);
        if (nearest == null) return;

        Projectile projectile = projectilePool.GetProjectile();
        projectile.transform.SetParent(null);
        projectile.transform.position = transform.position;

        float finalDamage = damage;
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null) finalDamage *= stats.attackMultiplier;

        projectile.Target = nearest.transform;
        projectile.Damage = finalDamage;
        projectile.Speed = projectileSpeed;
        projectile.MaxTravelDistance = maxTravelDistance;
    }
}