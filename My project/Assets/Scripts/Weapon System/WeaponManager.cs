using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("武器配置")]
    public int maxWeapons = 6;
    public float orbitRadius = 2f;
    public float positionSmoothTime = 0.3f;   // 漂浮跟随平滑时间

    [Header("攻击参数")]
    public float attackRange = 10f;
    public float projectileSpeed = 12f;
    public float maxBulletDistance = 25f;     // 子弹最大飞行距离

    [Header("子弹池")]
    public ProjectilePool projectilePool;

    [Header("武器模型预制体")]
    public GameObject weaponModelPrefab;      // 拖入刚才创建的 WeaponModel

    [Header("地图边界（用于子弹消失）")]
    public PlayAreaBounds playAreaBounds;

    [System.Serializable]
    public class WeaponInstance
    {
        public WeaponData data;
        public Transform visual;
        public float currentAngle;       // 当前角度（弧度）
        public float timer;
        public Vector3 velocity;         // SmoothDamp 的速度引用
        public Vector3 visualPosition;   // 实际世界坐标
    }

    public List<WeaponInstance> weapons = new List<WeaponInstance>();

    private void Awake()
    {
        Debug.Log("WeaponManager Awake");
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // 调试信息：确认脚本运行
        Debug.Log("WeaponManager Start 开始");

        // 检查必要组件
        if (projectilePool == null)
            Debug.LogError("projectilePool 未赋值！请在 Inspector 中拖入 ProjectilePool 对象。");
        if (weaponModelPrefab == null)
            Debug.LogError("weaponModelPrefab 未赋值！请在 Inspector 中拖入武器模型预制体。");
        if (playAreaBounds == null)
            Debug.LogWarning("playAreaBounds 未赋值，子弹可能不会因边界消失。");

        // 加载初始武器数据
        WeaponData initialWeapon = Resources.Load<WeaponData>("Weapons/Pistol_White");
        if (initialWeapon != null)
        {
            Debug.Log($"成功加载初始武器: {initialWeapon.name}, 品质: {initialWeapon.quality}");
            bool added = AddWeapon(initialWeapon);
            Debug.Log($"AddWeapon 返回结果: {added}");
            if (added)
                Debug.Log($"当前武器数量: {weapons.Count}");
            else
                Debug.LogError("添加初始武器失败！可能槽位已满或发生其他错误。");
        }
        else
        {
            Debug.LogError("未找到初始武器数据！请确认 Assets/Resources/Weapons/Pistol_White.asset 存在。");
        }

        Debug.Log("WeaponManager Start 结束");
    }

    private void Update()
    {
        UpdateWeaponPositions();
        UpdateAttacks();
    }

    /// <summary>
    /// 添加武器（购买时调用），自动处理合成与数量限制
    /// </summary>
    public bool AddWeapon(WeaponData newData)
    {
        if (weapons.Count >= maxWeapons)
            return false;

        // 合成检测：相同名称、相同品质
        WeaponInstance existing = weapons.FirstOrDefault(w => w.data.weaponName == newData.weaponName && w.data.quality == newData.quality);
        if (existing != null)
        {
            Quality nextQuality = existing.data.quality + 1;
            if (nextQuality <= Quality.Purple)
            {
                string path = $"Weapons/{existing.data.weaponType}_{nextQuality}";
                WeaponData upgradedData = Resources.Load<WeaponData>(path);
                if (upgradedData != null)
                {
                    existing.data = upgradedData;
                    UpdateWeaponVisual(existing);
                    Debug.Log($"武器合成：{existing.data.weaponName} 升级为 {nextQuality}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"升级数据缺失：{path}");
                    return false;
                }
            }
            else
            {
                Debug.Log("武器已达最高品质");
                return false;
            }
        }

        // 创建新武器
        WeaponInstance newWeapon = new WeaponInstance();
        newWeapon.data = newData;
        newWeapon.timer = 0f;
        newWeapon.currentAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        newWeapon.velocity = Vector3.zero;
        newWeapon.visualPosition = transform.position;  // 初始在玩家中心

        if (weaponModelPrefab != null)
        {
            GameObject model = Instantiate(weaponModelPrefab, transform.position, Quaternion.identity, transform);
            newWeapon.visual = model.transform;
            UpdateWeaponVisual(newWeapon);
        }
        else
        {
            Debug.LogError("武器模型预制体未指定！");
        }

        weapons.Add(newWeapon);
        RedistributeAngles();

        // 平滑移动到目标位置的效果已由 Update 实现，无需额外动画
        return true;
    }

    /// <summary>
    /// 出售武器（由商店调用）
    /// </summary>
    public void SellWeapon(WeaponData data)
    {
        WeaponInstance weapon = weapons.FirstOrDefault(w => w.data == data);
        if (weapon != null)
        {
            int refund = data.basePrice / 2;
            PlayerGold.Instance.AddGold(refund);
            if (weapon.visual != null)
                Destroy(weapon.visual.gameObject);
            weapons.Remove(weapon);
            RedistributeAngles();
            Debug.Log($"出售 {data.weaponName}，获得 {refund} 金币");
        }
    }

    // 更新视觉颜色（品质）
    private void UpdateWeaponVisual(WeaponInstance weapon)
    {
        if (weapon.visual == null) return;
        Renderer rend = weapon.visual.GetComponent<Renderer>();
        if (rend != null)
        {
            switch (weapon.data.quality)
            {
                case Quality.White: rend.material.color = Color.white; break;
                case Quality.Green: rend.material.color = Color.green; break;
                case Quality.Blue: rend.material.color = Color.blue; break;
                case Quality.Purple: rend.material.color = new Color(0.5f, 0f, 0.5f); break;
            }
        }
    }

    // 将所有武器均匀分布角度
    private void RedistributeAngles()
    {
        int count = weapons.Count;
        if (count == 0) return;
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            weapons[i].currentAngle = (i * angleStep) * Mathf.Deg2Rad;
        }
    }

    // 每帧更新武器位置（弹性漂浮）
    private void UpdateWeaponPositions()
    {
        // 玩家当前 Y 轴旋转（四元数）
        Quaternion playerRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        foreach (var weapon in weapons)
        {
            if (weapon.visual == null) continue;

            // 基础偏移：武器在局部空间的径向位置
            Vector3 localOffset = new Vector3(
                Mathf.Cos(weapon.currentAngle),
                0f,
                Mathf.Sin(weapon.currentAngle)
            ) * orbitRadius;

            // 应用到世界空间，跟随玩家旋转
            Vector3 targetPos = transform.position + playerRot * localOffset;

            // 平滑漂浮移动
            weapon.visualPosition = Vector3.SmoothDamp(
                weapon.visualPosition, targetPos,
                ref weapon.velocity, positionSmoothTime
            );
            weapon.visual.position = weapon.visualPosition;

            // 武器模型面朝外（径向），同样跟随玩家旋转
            Vector3 radialDir = playerRot * localOffset.normalized;
            weapon.visual.rotation = Quaternion.LookRotation(radialDir, Vector3.up);
        }
    }

    // 每帧更新攻击计时器
    private void UpdateAttacks()
    {
        if (weapons.Count == 0) return;
        PlayerStats stats = GetComponent<PlayerStats>();

        foreach (var weapon in weapons)
        {
            weapon.timer -= Time.deltaTime;
            if (weapon.timer <= 0f)
            {
                float interval = weapon.data.attackInterval;
                if (stats != null) interval /= stats.attackSpeedMultiplier;
                weapon.timer = interval;

                switch (weapon.data.weaponType)
                {
                    case WeaponType.Pistol:
                        PistolAttack(weapon, stats);
                        break;
                    case WeaponType.Staff:
                        // 暂未实现
                        break;
                }
            }
        }
    }

    // 手枪攻击：从武器当前位置发射追踪弹
    private void PistolAttack(WeaponInstance weapon, PlayerStats stats)
    {
        // 1. 检查敌人管理器
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("[武器] EnemyManager.Instance 为 null，无法获取敌人！");
            return;
        }

        Enemy nearest = EnemyManager.Instance.GetNearestEnemy(transform.position, attackRange);
        if (nearest == null)
        {
            Debug.Log("[武器] 攻击范围内没有敌人");
            return;
        }

        // 2. 检查对象池
        if (projectilePool == null)
        {
            Debug.LogError("[武器] projectilePool 未赋值！请在 Inspector 拖入对象池。");
            return;
        }

        Vector3 spawnPos = weapon.visual != null ? weapon.visual.position : transform.position;
        Projectile proj = projectilePool.GetProjectile();
        if (proj == null)
        {
            Debug.LogError("[武器] 从对象池获取子弹失败（可能预制体为空）");
            return;
        }

        proj.transform.position = spawnPos;
        proj.transform.SetParent(null);

        // 3. 计算伤害
        float dmg = weapon.data.baseDamage * weapon.data.damageMultiplier;
        if (stats != null) dmg *= stats.attackMultiplier;
        if (stats != null && Random.value < stats.critChance) dmg *= 2f;

        // 4. 输出关键值，方便检查
        Debug.Log($"[武器] 发射子弹 → 目标:{nearest.name}, 位置:{spawnPos}, 伤害:{dmg}, 速度:{projectileSpeed}, 最大距离:{maxBulletDistance}");

        proj.Init(projectilePool, nearest.transform, dmg, projectileSpeed);
        proj.MaxTravelDistance = maxBulletDistance;
    }
}