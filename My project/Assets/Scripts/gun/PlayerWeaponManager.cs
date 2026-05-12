using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("武器预制体")]
    public GameObject weaponPrefab;

    [Header("最大武器数")]
    public int maxWeapons = 6;

    [Header("环绕设置")]
    public float orbitRadius = 1.5f;      // 环绕半径
    public float weaponHeight = 0.8f;     // 武器高度偏移
    public float startAngle = 0f;         // 第一把武器的起始角度（0° = 角色前方）
    public int initialWeaponCount = 1;    // 初始武器数量

    private List<FloatingWeapon> activeWeapons = new List<FloatingWeapon>();

    void Start()
    {
        int startCount = Mathf.Min(maxWeapons, initialWeaponCount);
        for (int i = 0; i < startCount; i++)
        {
            AddWeapon();
        }
    }

    /// <summary>
    /// 添加一把武器（均匀环绕）
    /// </summary>
    public void AddWeapon()
    {
        // 调试日志（确认是否被调用）
        Debug.Log($"[AddWeapon] 当前数量={activeWeapons.Count}, 上限={maxWeapons}, prefab={(weaponPrefab ? "已赋值" : "空")}");

        if (activeWeapons.Count >= maxWeapons || weaponPrefab == null)
        {
            Debug.LogWarning("无法添加武器：已达上限或未指定 prefab");
            return;
        }

        GameObject obj = Instantiate(weaponPrefab);
        FloatingWeapon fw = obj.GetComponent<FloatingWeapon>();
        if (fw == null)
        {
            Debug.LogError("武器预制体上未找到 FloatingWeapon 组件，已销毁生成物");
            Destroy(obj);
            return;
        }

        fw.owner = transform;
        activeWeapons.Add(fw);

        // 重新分配所有武器的环绕位置
        RepositionWeapons();

        Debug.Log($"武器已添加，当前数量：{activeWeapons.Count}");
    }

    /// <summary>
    /// 移除最后一把武器
    /// </summary>
    public void RemoveWeapon()
    {
        if (activeWeapons.Count == 0) return;
        FloatingWeapon last = activeWeapons[activeWeapons.Count - 1];
        activeWeapons.Remove(last);
        Destroy(last.gameObject);

        RepositionWeapons();
    }

    /// <summary>
    /// 按 360°/n 均匀分布所有武器
    /// </summary>
    void RepositionWeapons()
    {
        int count = activeWeapons.Count;
        if (count == 0) return;

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 offset = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad),
                weaponHeight,
                Mathf.Cos(angle * Mathf.Deg2Rad)
            ) * orbitRadius;

            activeWeapons[i].localOffset = offset;
        }
    }

    // 测试用按键（可删除）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (activeWeapons.Count < maxWeapons)
                AddWeapon();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            RemoveWeapon();
        }
    }
}