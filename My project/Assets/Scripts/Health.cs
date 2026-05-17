using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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
        Debug.Log($"[Health] {gameObject.name} 收到伤害 {damage}，当前血量 {currentHealth}");
        if (IsDead) return;

        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null && stats.dodgeChance > 0f)
        {
            if (Random.value < stats.dodgeChance)
            {
                Debug.Log("[Health] 闪避成功，伤害无效");
                return;
            }
        }

        currentHealth -= damage;
        Debug.Log($"[Health] 扣血后 {gameObject.name} 剩余血量: {currentHealth}");

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
        //OnHealthChanged?.Invoke(currentHealth);
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;   // 提升上限同时恢复相同数量血量
                                   // 如果有血量变化事件，触发更新
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