using UnityEngine;

public enum UpgradeType
{
    AttackDamage,
    AttackSpeed,
    MoveSpeed,
    MaxHealth,
    HealthRegen,
    CritChance,
    DodgeChance,
    ExpGain,
    WeaponCardWhite,
    WeaponCardGreen,
    WeaponCardBlue,
    WeaponCardPurple,
    WeaponMutation,
    SellWeapon
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "游戏配置/升级卡片")]
public class UpgradeData : ScriptableObject
{
    [Header("基本信息")]
    public string upgradeName = "攻击力";
    [TextArea]
    public string description = "增加 10% 攻击力";
    public Sprite icon;
    public UpgradeType type;

    [Header("属性升级数值")]
    public float effectValuePerLevel = 0.1f;

    [Header("武器卡专用")]
    public WeaponData weaponData;      // 若为武器卡，拖入对应武器数据资产

    [Header("价格")]
    public int baseCost = 50;          // 购买价格
    public float costMultiplier = 1.5f;
    public int maxLevel = 10;          // 属性卡用，武器卡通常用不到
}