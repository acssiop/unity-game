using UnityEngine;

public enum UpgradeType
{
    AttackDamage,
    AttackSpeed,
    MoveSpeed,
    MaxHealth,
    HealthRegen       // 预留，暂不实现效果
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "游戏配置/升级卡片")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName = "新卡片";
    [TextArea] public string description = "效果说明";
    public Sprite icon;                  // 卡片图标（可不设置）
    public UpgradeType type;

    public int baseCost = 50;            // 购买价格
}