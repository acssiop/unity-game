using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponData1 : MonoBehaviour
{
    // CreateAssetMenu 文件，用于在编辑器中配置
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "游戏数据/武器库")]
    public class WeaponDatabase : ScriptableObject
    {
        public List<WeaponData> allWeapons;
    }
}
