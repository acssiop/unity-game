using System.Diagnostics;
using UnityEngine;
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
        spawnPosition = transform.position;
        //Debug.Log($"[子弹] 激活，出生点: {spawnPosition}");
    }

    private void Update()
    {
        if (Target == null || !Target.gameObject.activeInHierarchy)
        {
            ReturnToPool();
            return;
        }

        if (Vector3.Distance(spawnPosition, transform.position) >= MaxTravelDistance)
        {
            ReturnToPool();
            return;
        }

        Vector3 direction = (Target.position - transform.position).normalized;
        transform.position += direction * Speed * Time.deltaTime;
        transform.forward = direction;

        if (Vector3.Distance(transform.position, Target.position) <= hitRadius)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        // 这里优先使用 Health 组件，这也和你的设计一致
        var health = Target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(Damage);
        }
        else
        {
            // 备用：直接调用 Enemy.Die
            var enemy = Target.GetComponent<Enemy>();
            if (enemy != null)
                enemy.Die();
        }
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        //Debug.Log("[子弹] 回收");
        if (Pool != null) Pool.ReturnProjectile(this);
        else Destroy(gameObject);
    }
}