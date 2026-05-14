using UnityEngine;
using System.Collections.Generic;   // 如果你用到了 List<WeaponLevelData>

[CreateAssetMenu(fileName = "WeaponConfig_", menuName = "Game/Weapon Config")]
public class WeaponConfig : ScriptableObject
{
    public enum WeaponCategory { Dagger, Hammer, SMG, Rifle, Shotgun }

    public string weaponName;
    public WeaponCategory category;
    public GameObject weaponPrefab;   // 对应场景中实例化的预制体
    public WeaponLevelData[] levels = new WeaponLevelData[4]; // I~IV
    public enum WeaponEffectType
    {
        None = 0,
        Bleed = 1,
        Stun = 2,
        Earthquake = 3,
        Penetrate = 4,
        Burn = 5,
        FireBurst = 6
    }
}


