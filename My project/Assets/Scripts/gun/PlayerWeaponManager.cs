using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("武器预制体")]
    public GameObject weaponPrefab;

    [Header("最大武器数")]
    public int maxWeapons = 6;

    [Header("预设环绕位置（对应每把武器）")]
    public List<Vector3> offsetPresets = new List<Vector3>();

    private List<FloatingWeapon> activeWeapons = new List<FloatingWeapon>();

    void Start()
    {
        // 初始生成几把武器（数量可在此修改，或绑定按键/UI）
        int startCount = Mathf.Min(maxWeapons, offsetPresets.Count);
        for (int i = 0; i < startCount; i++)
        {
            AddWeapon(i);
        }
    }

    /// <summary>
    /// 按索引添加一把武器（可在外部调用，如升级时）
    /// </summary>
    public void AddWeapon(int index)
    {
        if (activeWeapons.Count >= maxWeapons || index >= offsetPresets.Count || weaponPrefab == null)
            return;

        GameObject obj = Instantiate(weaponPrefab);
        FloatingWeapon fw = obj.GetComponent<FloatingWeapon>();
        if (fw == null) return;

        fw.owner = transform;
        fw.localOffset = offsetPresets[index];
        activeWeapons.Add(fw);
    }

    /// <summary>
    /// 移除指定武器（示例：移除最后一把）
    /// </summary>
    public void RemoveWeapon()
    {
        if (activeWeapons.Count == 0) return;
        FloatingWeapon last = activeWeapons[activeWeapons.Count - 1];
        activeWeapons.Remove(last);
        Destroy(last.gameObject);
    }

    // 测试用：按键增减武器（可删除）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (activeWeapons.Count < maxWeapons)
                AddWeapon(activeWeapons.Count);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            RemoveWeapon();
        }
    }
}