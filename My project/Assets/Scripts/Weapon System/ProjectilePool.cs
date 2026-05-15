using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public Projectile projectilePrefab;
    public int initialSize = 30;

    private readonly Queue<Projectile> pool = new Queue<Projectile>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            CreateNewProjectile();
    }

    private Projectile CreateNewProjectile()
    {
        var proj = Instantiate(projectilePrefab, transform);
        proj.gameObject.SetActive(false);
        proj.Pool = this;
        pool.Enqueue(proj);
        return proj;
    }

    public Projectile GetProjectile()
    {
        if (pool.Count == 0) CreateNewProjectile();

        var proj = pool.Dequeue();
        // 清理残留数据
        proj.Target = null;
        proj.Damage = 0f;
        proj.Speed = 0f;
        proj.MaxTravelDistance = 0f;
        proj.gameObject.SetActive(true);
        return proj;
    }

    public void ReturnProjectile(Projectile proj)
    {
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform);
        pool.Enqueue(proj);
    }
}