using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float radius = 2f;
    public float damage = 10f;
    public LayerMask enemyLayer = 1 << 7;   // 假设敌人为 Layer 7

    private void Start()
    {
        // 范围检测
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        foreach (Collider col in hits)
        {
            Health health = col.GetComponent<Health>();
            if (health) health.TakeDamage(damage);
        }
        Destroy(gameObject, 1f); // 视觉效果后销毁
    }

    public void SetDamage(float dmg) { damage = dmg; }
}