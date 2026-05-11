using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    private int damage;
    private GameObject target;

    public void Initialize(GameObject enemy, int dmg)
    {
        target = enemy;
        damage = dmg;
        // 可记录初始方向等
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 简单直线移动（或用目标追踪）
        if (target != null)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 命中敌人后造成伤害并销毁子弹
        if (other.gameObject == target)
        {
            // enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}