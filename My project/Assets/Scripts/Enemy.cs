using System;
using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    // ---------- 生成器 ----------
    public GameObject PrefabReference { get; private set; }
    public float HeightOffset { get; private set; }
    private Enemy_Spawner spawner;

    // ---------- 目标 ----------
    private Transform player;

    // ---------- 伤害 ----------
    [Header("伤害")]
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

    [Header("攻击前摇")]
    public float attackWindupTime = 0.5f;          // 前摇时间
    public GameObject warningMarkPrefab;           // 可选：头顶感叹号

    private float attackTimer;
    private bool canMove = false;

    private bool isWindingUp = false;              // 正在前摇
    private float windupTimer = 0f;
    private GameObject warningMarkInstance;

    // ---------- 初始化 ----------
    public void Init(Enemy_Spawner owner, GameObject prefab)
    {
        spawner = owner;
        PrefabReference = prefab;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        attackTimer = 0f;
        StopAllCoroutines();
        StartCoroutine(StartMoveAfterDelay());

        // 创建警告标记
        CreateWarningMark();
    }

    private void CreateWarningMark()
    {
        if (warningMarkPrefab == null) return;
        warningMarkInstance = Instantiate(warningMarkPrefab, transform);
        warningMarkInstance.transform.localPosition = new Vector3(0, 2f, 0); // 头顶
        warningMarkInstance.SetActive(false);
    }

    // ---------- 延迟开始移动 ----------
    private IEnumerator StartMoveAfterDelay()
    {
        canMove = false;
        yield return new WaitForSeconds(startMoveDelay);
        canMove = true;
    }

    // ---------- 每帧更新 ----------
    private void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy) return;
        if (!canMove) return;

        // ----- 如果正在前摇（不可打断）-----
        if (isWindingUp)
        {
            // 面向玩家（即使玩家跑远也面朝）
            Vector3 dirToPlayer = player.position - transform.position;
            dirToPlayer.y = 0f;
            if (dirToPlayer != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            windupTimer -= Time.deltaTime;
            if (windupTimer <= 0f)
            {
                ExecuteAttack();   // 前摇结束，强制攻击
            }
            return;                 // 前摇期间完全不移动
        }

        // ----- 正常移动与检查 -----
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        float distance = dir.magnitude;

        // 旋转面向玩家
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (distance > attackRange)
        {
            // 移动追击
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f && !isWindingUp)
            {
                StartWindup();      // 进入攻击前摇
            }
        }
    }

    // ---------- 前摇相关 ----------
    private void StartWindup()
    {
        isWindingUp = true;
        windupTimer = attackWindupTime;

        if (warningMarkInstance != null)
            warningMarkInstance.SetActive(true);
    }

    private void CancelWindup()     // 保留，但当前逻辑中不再使用
    {
        isWindingUp = false;
        windupTimer = 0f;
        if (warningMarkInstance != null)
            warningMarkInstance.SetActive(false);
    }

    private void ExecuteAttack()
    {
        // 隐藏警告标记
        if (warningMarkInstance != null)
            warningMarkInstance.SetActive(false);

        isWindingUp = false;
        // 无论命中与否, 攻击冷却都重置 (攻击动作已完成)
        attackTimer = attackInterval;

        if (player == null) return;

        // 补充攻击范围检测: 玩家跑出范围则攻击落空
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange)
        {
            Debug.Log($"{gameObject.name} 攻击落空 - 玩家已逃出攻击范围");
            return;
        }

        // 在范围内则造成伤害
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        Debug.Log($"{gameObject.name} 攻击命中（前摇结束）");
    }

    // ---------- 死亡 ----------
    public void Die()
    {
        Debug.Log($"{gameObject.name} 被击杀");

        // 掉落金币
        if (CoinManager.Instance != null)
        {
            int coinCount = Random.Range(1, 4);
            for (int i = 0; i < coinCount; i++)
            {
                CoinManager.Instance.SpawnCoin(transform.position, 1);
            }
        }

        // 掉落经验球（固定1个，价值1）
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.SpawnExperienceBall(transform.position, 1);
        }

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

    // ---------- 启用/禁用时注册/注销管理器 ----------
    private void OnEnable()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(this);
    }

    private void OnDisable()
    {
        if (spawner != null)
            spawner.RemoveEnemy(gameObject);

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this);
    }
}