using UnityEngine;

public class FloatingWeapon : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform owner;
    public Vector3 localOffset = new Vector3(1.5f, 0.8f, 0);
    [Tooltip("值越大延迟越明显（空气阻力感）")]
    public float followSmoothTime = 0.15f;

    [Header("索敌")]
    public float detectRange = 10f;
    // 原先：public LayerMask enemyLayer;       // ← 删除这行
    [Tooltip("敌人的 Tag 名称，例如：Enemy")]
    public string enemyTag = "Enemy";          // ← 新增这行

    [Header("射击")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireInterval = 3f;
    public int damage = 25;

    private Vector3 velocity = Vector3.zero;
    private Transform currentTarget;
    private float fireTimer;

    void Update()
    {
        if (owner == null) return;

        // 1. 跟随玩家
        Vector3 targetPos = owner.position + owner.rotation * localOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, followSmoothTime);

        // 2. 索敌
        if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            FindTarget();

        // 3. 转向敌人
        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }

        // 4. 射击冷却
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval && currentTarget != null)
        {
            Fire();
            fireTimer = 0f;
        }
    }

    void FindTarget()
    {
        // 检测周围所有带碰撞体的物体（不再限制Layer）
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange);
        float nearestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider hit in hits)
        {
            // 只保留 Tag 为指定敌人标签的物体
            if (hit.CompareTag(enemyTag))   // ← 修改点：通过Tag筛选
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.transform;
                }
            }
        }
        currentTarget = nearest;
    }

    void Fire()
    {
        if (currentTarget == null || bulletPrefab == null) return;
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        Vector3 dir = (currentTarget.position - spawnPos).normalized;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
        bullet.GetComponent<Bullet>()?.Initialize(currentTarget.gameObject, damage);
    }
}