using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public ProjectilePool Pool { get; set; }
    [HideInInspector] public Transform Target { get; set; }
    [HideInInspector] public float Damage { get; set; }
    [HideInInspector] public float Speed { get; set; }
    [HideInInspector] public float MaxTravelDistance { get; set; }

    private Vector3 spawnPosition;
    [SerializeField] private float hitRadius = 0.3f;

    private void OnEnable()
    {
        // 不再记录位置，等待 Init
    }

    public void Init(ProjectilePool ownerPool, Transform targetEnemy, float projectileDamage, float projectileSpeed)
    {
        Pool = ownerPool;
        Target = targetEnemy;
        Damage = projectileDamage;
        Speed = projectileSpeed;
        // ★ 修正点：在位置设置后再记录
        spawnPosition = transform.position;

        Debug.Log($"[子弹] Init成功 → 起始位置:{spawnPosition}, 目标:{targetEnemy.name}, 伤害:{Damage}");
    }

    private void Update()
    {
        // 边界检测
        if (WeaponManager.Instance != null && WeaponManager.Instance.playAreaBounds != null)
        {
            PlayAreaBounds bounds = WeaponManager.Instance.playAreaBounds;
            Vector3 pos = transform.position;
            if (pos.x < bounds.MinX || pos.x > bounds.MaxX || pos.z < bounds.MinZ || pos.z > bounds.MaxZ)
            {
                Debug.Log("[子弹] 超出地图边界，回收");
                ReturnToPool();
                return;
            }
        }

        // 目标丢失
        if (Target == null || !Target.gameObject.activeInHierarchy)
        {
            Debug.Log("[子弹] 目标已失效，回收");
            ReturnToPool();
            return;
        }

        // 飞行距离
        float traveled = Vector3.Distance(spawnPosition, transform.position);
        if (traveled >= MaxTravelDistance)
        {
            Debug.Log($"[子弹] 飞行距离超限 ({traveled} >= {MaxTravelDistance})，回收");
            ReturnToPool();
            return;
        }

        // 移动
        Vector3 direction = (Target.position - transform.position).normalized;
        transform.position += direction * Speed * Time.deltaTime;
        transform.forward = direction;

        // 命中检测
        float distToTarget = Vector3.Distance(transform.position, Target.position);
        if (distToTarget <= hitRadius)
        {
            Debug.Log($"[子弹] 命中目标！距离={distToTarget}, 伤害={Damage}");
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (Target == null) return;

        var health = Target.GetComponent<Health>();
        if (health != null)
        {
            Debug.Log($"[子弹] 目标有 Health 组件，调用 TakeDamage({Damage})");
            health.TakeDamage(Damage);
        }
        else
        {
            var enemy = Target.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("[子弹] 目标无 Health，直接调用 Enemy.Die()");
                enemy.Die();
            }
            else
            {
                Debug.LogError("[子弹] 目标既无 Health 也无 Enemy 组件，无法造成任何效果！");
            }
        }
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (Pool != null) Pool.ReturnProjectile(this);
        else Destroy(gameObject);
    }
}