using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class Player : MonoBehaviour
{
    [HideInInspector] public bool IsDead { get; private set; } = false;
    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Debug.Log("玩家死亡！");
        Enemy_Spawner spawner = FindObjectOfType<Enemy_Spawner>();
        // 显示死亡弹窗（GameOverManager 需提前置于场景中独立 Canvas 上）
        if (GameOverManager.Instance != null)
            GameOverManager.Instance.ShowDeathPanel();
        // 玩家消失（所有组件自动停用）
        gameObject.SetActive(false);
    }
}