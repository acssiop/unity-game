using UnityEngine;

public class FloatingWeapon : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform owner;
    public Vector3 localOffset = new Vector3(1.5f, 0.8f, 0);
    [Tooltip("值越大延迟越明显（空气阻力感）")]
    public float followSmoothTime = 0.075f;

    [Header("索敌")]
    public float detectRange = 10f;
    public string enemyTag = "Enemy";
    [Tooltip("敌人离开锁定范围多远后脱战")]
    public float disengageRangeMultiplier = 1.5f; // 新增

    [Header("射击")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireInterval = 3f;
    public int damage = 25;

    [Header("转向")]
    public float rotationSpeed = 120f;  // 每秒旋转度数，控制转向速度

    private Vector3 velocity = Vector3.zero;
    private Transform currentTarget;
    private float fireTimer;

    void Update()
    {
        if (owner == null) return;

        // 1. 跟随玩家
        Vector3 targetPos = owner.position + owner.rotation * localOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, followSmoothTime);

        // 2. 索敌（若当前目标无效）
        if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            FindTarget();

        // 3. 脱战检测：敌人跑太远则放弃目标
        if (currentTarget != null &&
            Vector3.Distance(transform.position, currentTarget.position) > detectRange * disengageRangeMultiplier)
        {
            currentTarget = null;
        }

        // 4. 转向
        if (currentTarget != null)
        {
            // 朝向敌人
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // ★ 新增：无敌人时，缓慢转向与人物朝向一致
            Quaternion ownerRotation = Quaternion.LookRotation(owner.forward, owner.up);
            // 忽略y轴倾斜（可选，保持武器水平）
            ownerRotation = Quaternion.Euler(0, ownerRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, ownerRotation, rotationSpeed * Time.deltaTime);
        }

        // 5. 射击冷却
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval && currentTarget != null)
        {
            Fire();
            fireTimer = 0f;
        }
    }

    void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange);
        float nearestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(enemyTag))
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
        GameObject bullet = ObjectPool.Instance.GetFromPool("Bullet");
        if (bullet == null) return;

        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.GetComponent<Bullet>()?.Initialize(currentTarget.gameObject, damage);
    }
}