using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Enemy : MonoBehaviour
{
    // ---------- 对象池 ----------
    public GameObject PrefabReference { get; private set; }

    public float HeightOffset { get; private set; }

    private Enemy_Spawner spawner;

    // ---------- 玩家 ----------
    private Transform player;

    // ---------- 属性 ----------
    [Header("属性")]
    public float damage = 10f;

    // ---------- 移动 ----------
    [Header("移动")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 8f;
    public float startMoveDelay = 0.5f;

    // ---------- 攻击 ----------
    [Header("攻击")]
    public float attackRange = 2f;
    public float attackInterval = 1f;

    private float attackTimer;

    private bool canMove = false;

    // ---------- 初始化 ----------
    public void Init(Enemy_Spawner owner, GameObject prefab)
    {
        spawner = owner;
        PrefabReference = prefab;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        attackTimer = 0f;

        StopAllCoroutines();
        StartCoroutine(StartMoveAfterDelay());
    }

    // ---------- 延迟开始移动 ----------
    private IEnumerator StartMoveAfterDelay()
    {
        canMove = false;

        yield return new WaitForSeconds(startMoveDelay);

        canMove = true;
    }

    // ---------- 更新 ----------
    private void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
            return;
        if (player == null) return;
        if (!canMove) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        float distance = dir.magnitude;

        // 朝向玩家
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }

        // 超出攻击范围 → 移动
        if (distance > attackRange)
        {
            transform.position +=
                transform.forward * moveSpeed * Time.deltaTime;
        }
        // 进入攻击范围 → 攻击
        else
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackInterval;
            }
        }
    }

    // ---------- 攻击 ----------
    private void Attack()
    {
        if (player == null) return;

        Health playerHealth = player.GetComponent<Health>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Debug.Log($"{gameObject.name} 攻击玩家");
    }

    // ---------- 死亡 ----------
    public void Die()
    {
        Debug.Log($"{gameObject.name} 被击杀");

        if (spawner != null)
        {
            spawner.RemoveEnemy(gameObject);
        }

        gameObject.SetActive(false);
    }

    // ---------- 高度偏移 ----------
    private void Awake()
    {
        HeightOffset = CalculateHeightOffset();
    }

    private float CalculateHeightOffset()
    {
        Bounds bounds;

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            bounds = col.bounds;
        }
        else
        {
            Renderer rend = GetComponent<Renderer>();

            if (rend != null)
                bounds = rend.bounds;
            else
                return 0f;
        }

        float lowestY = bounds.min.y;

        return transform.position.y - lowestY;
    }

    // ---------- 回收 ----------
    private void OnEnable()
    {
        // 原有初始化（如果有）保留
        // 新增：向敌人管理器注册
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(this);
    }
    private void OnDisable()
    {
        //canMove = false;
        if (spawner != null)
            spawner.RemoveEnemy(gameObject);
        // 新增：从敌人管理器注销
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this);
    }
}