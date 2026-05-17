using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;
    public float timer;   // ¹¥»÷¼ÆÊ±Æ÷

    public void SetData(WeaponData newData)
    {
        data = newData;
        timer = 0;
    }
}