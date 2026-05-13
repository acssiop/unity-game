using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("武器配置（按键 1~5 对应）")]
    public WeaponConfig[] weaponConfigs;

    [Header("布局参数")]
    public float circleRadius = 1.5f;
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0); // 武器出生点（相对玩家局部坐标）
    public float moveDuration = 0.3f;                     // 移动到环绕位置的时间

    [Header("武器上限")]
    public int maxWeapons = 6;

    private List<Weapon> activeWeapons = new List<Weapon>();
    private Coroutine moveCoroutine;

    void Update()
    {
        for (int i = 0; i < Mathf.Min(weaponConfigs.Length, 5); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                AddWeapon(weaponConfigs[i], 1);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveLastWeapon();
        }
    }

    /// <summary>
    /// 添加武器入口：模拟合成 → 实际创建/合成 → 出生 → 启动移动动画
    /// </summary>
    public void AddWeapon(WeaponConfig config, int level)
    {
        // 1. 模拟合成，检查最终数量
        var simulatedList = SimulateMerge(activeWeapons, config, level);
        if (simulatedList.Count > maxWeapons)
        {
            Debug.LogWarning($"合成后武器数将达到 {simulatedList.Count}，超过上限 {maxWeapons}，无法添加！");
            return;
        }

        // 2. 从池中获取新武器
        Weapon newWeapon = WeaponPool.Instance.Get(config, level);
        if (newWeapon == null) return;

        // 3. 执行合成（可能返回升级武器或原武器）
        newWeapon = MergeWeapon(newWeapon);

        // 4. 如果武器未被完全合并（返回了实例），则加入列表
        if (newWeapon != null)
        {
            // 确保武器被放置在出生点（合成升级时已经在 MergeWeapon 中设置过）
            if (newWeapon.transform.parent != transform)
                PlaceWeaponAtSpawn(newWeapon);

            activeWeapons.Add(newWeapon);
            RearrangeWeapons();
        }

        PrintWeaponList();
    }

    /// <summary>
    /// 将武器设为玩家子物体并放置在出生点
    /// </summary>
    private void PlaceWeaponAtSpawn(Weapon weapon)
    {
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = spawnOffset;
        weapon.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 移除最后获得的武器（不带动画）
    /// </summary>
    public void RemoveLastWeapon()
    {
        if (activeWeapons.Count == 0) return;

        int lastIndex = activeWeapons.Count - 1;
        Weapon weapon = activeWeapons[lastIndex];
        activeWeapons.RemoveAt(lastIndex);
        Debug.Log($"移除：{weapon.config.weaponName} {GetLevelString(weapon.currentLevel)}");
        WeaponPool.Instance.ReturnWeapon(weapon);
        RearrangeWeapons();
    }

    /// <summary>
    /// 触发所有武器平滑移动到新位置
    /// </summary>
    private void RearrangeWeapons()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveWeaponsToTargets());
    }

    /// <summary>
    /// 协程：将所有武器从当前位置平滑移动到环形目标位置
    /// </summary>
    private IEnumerator MoveWeaponsToTargets()
    {
        int count = activeWeapons.Count;
        if (count == 0) yield break;

        // 计算每个武器的目标局部位置
        Vector3[] targetPositions = new Vector3[count];
        float angleStep = 360f / count;
        Vector3 center = new Vector3(0, spawnOffset.y, 0);   // 环形圆心保持与出生点同高度

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * circleRadius;
            targetPositions[i] = center + offset;
        }

        // 记录起始位置
        Vector3[] startPositions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            startPositions[i] = activeWeapons[i].transform.localPosition;
        }

        // 插值移动
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float ease = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < count; i++)
            {
                activeWeapons[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], ease);
            }
            yield return null;
        }

        // 精确到位
        for (int i = 0; i < count; i++)
        {
            activeWeapons[i].transform.localPosition = targetPositions[i];
            activeWeapons[i].transform.localRotation = Quaternion.identity;
        }
    }

    // ========== 合成相关 ==========

    /// <summary>
    /// 模拟合成，返回合并后的武器配置列表（不涉及真实对象）
    /// </summary>
    private List<(WeaponConfig cfg, int level)> SimulateMerge(List<Weapon> currentList, WeaponConfig config, int level)
    {
        var list = new List<(WeaponConfig cfg, int level)>();
        foreach (var w in currentList)
            list.Add((w.config, w.currentLevel));
        list.Add((config, level));

        bool merged;
        do
        {
            merged = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].level >= 4) continue;
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[j].level >= 4) continue;
                    if (list[i].cfg == list[j].cfg && list[i].level == list[j].level)
                    {
                        var mergedCfg = list[i].cfg;
                        var mergedLevel = list[i].level;
                        list.RemoveAt(j);      // 先移除后面，再移除前面
                        list.RemoveAt(i);
                        list.Add((mergedCfg, mergedLevel + 1));
                        merged = true;
                        break;
                    }
                }
                if (merged) break;
            }
        } while (merged);

        return list;
    }

    /// <summary>
    /// 实际合成：寻找可合并武器并返回升级后的武器，同时回收被合并的实例
    /// </summary>
    private Weapon MergeWeapon(Weapon weapon)
    {
        WeaponConfig cfg = weapon.config;
        int lv = weapon.currentLevel;
        if (lv >= 4)
            return weapon;   // 满级不合成

        Weapon match = activeWeapons.Find(w => w != weapon && w.config == cfg && w.currentLevel == lv);
        if (match != null)
        {
            Debug.Log($"合成：{cfg.weaponName} {GetLevelString(lv)} + {GetLevelString(lv)} → {GetLevelString(lv + 1)}");

            // 移除匹配武器并回收
            activeWeapons.Remove(match);
            WeaponPool.Instance.ReturnWeapon(match);
            // 回收当前武器
            WeaponPool.Instance.ReturnWeapon(weapon);

            // 创建升级武器并从出生点出现
            Weapon upgraded = WeaponPool.Instance.Get(cfg, lv + 1);
            if (upgraded != null)
            {
                PlaceWeaponAtSpawn(upgraded);  // 确保升级武器也从中心出生
                return MergeWeapon(upgraded);  // 递归（可能继续合成）
            }
        }
        return weapon;
    }

    // ========== 辅助方法 ==========

    private void PrintWeaponList()
    {
        string msg = "当前武器：";
        foreach (var w in activeWeapons)
            msg += $"[{w.config.weaponName} {GetLevelString(w.currentLevel)}] ";
        Debug.Log(msg);
    }

    private string GetLevelString(int level)
    {
        return level switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            _ => "?"
        };
    }
}