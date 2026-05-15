using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Health : MonoBehaviour
{
    [Header("生命值")]
    public float maxHealth = 100f;

    private float currentHealth;

    public bool IsDead => currentHealth <= 0;
    public event Action<float> OnHealthChanged;
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // 对象池重新启用时重置血量
    private void OnEnable()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余血量：{currentHealth}");
        OnHealthChanged?.Invoke(currentHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // 回复生命
    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    // 死亡
    private void Die()
    {
        Debug.Log($"{gameObject.name} 死亡");
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null) { enemy.Die(); return; }
        Player player = GetComponent<Player>();
        if (player != null) { player.Die(); }


    }

    // 获取当前血量
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // 获取血量百分比
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}