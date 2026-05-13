[System.Serializable]
public class WeaponLevelData
{
    public int level;                 // I=1, II=2, III=3, IV=4
    public float damage;              // 基础伤害
    public float attackSpeed;         // 每秒攻击次数
    public float range;               // 射程/攻击范围
    public float critChance;          // 暴击率（0~1）
    public int knockback;             // 击退距离（0为无）

    // 特殊效果参数
    public WeaponEffectType effectType;
    public float effectValue1;        // 根据类型含义不同（流血伤害/晕眩秒数/弹片数等）
    public float effectValue2;
    public int effectValueInt;        // 叠加层数/穿透敌人数等
}