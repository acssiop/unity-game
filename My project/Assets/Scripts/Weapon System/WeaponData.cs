using UnityEngine;

public enum Quality
{
    White,
    Green,
    Blue,
    Purple
}
public enum WeaponType
{
    Pistol,
    Staff
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "游戏配置/武器数据")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "手枪";
    public WeaponType weaponType = WeaponType.Pistol;
    public Quality quality = Quality.White;
    public int basePrice = 12;
    public float baseDamage = 20f;
    public float damageMultiplier = 1.0f;   // 品质伤害倍率
    public float attackInterval = 1.0f;      // 攻击间隔（秒）
    public int projectileCount = 1;          // 手枪子弹数 / 法杖无用
    // 未来可加：弹丸数，穿透等
}