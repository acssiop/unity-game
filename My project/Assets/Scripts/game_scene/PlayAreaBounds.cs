using UnityEngine;

/// <summary>
/// 挂载到地面 Plane 上，统一管理可移动区域的边界。
/// 坐标系：X 对应世界 X 轴，Y 对应世界 Z 轴（保持与原来 Vector2 映射一致）。
/// </summary>
public class PlayAreaBounds : MonoBehaviour
{
    [Header("移动区域边界（XZ 平面）")]
    [Tooltip("x = 世界 X 最小值，y = 世界 Z 最小值")]
    public Vector2 minBounds = new Vector2(-50f, -50f);

    [Tooltip("x = 世界 X 最大值，y = 世界 Z 最大值")]
    public Vector2 maxBounds = new Vector2(50f, 50f);

    /// <summary>
    /// 获取世界 X 轴最小/最大值
    /// </summary>
    public float MinX => minBounds.x;
    public float MaxX => maxBounds.x;

    /// <summary>
    /// 获取世界 Z 轴最小/最大值
    /// </summary>
    public float MinZ => minBounds.y;
    public float MaxZ => maxBounds.y;

    void OnDrawGizmosSelected()
    {
        // 在 Scene 视图中显示边界框
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((MinX + MaxX) * 0.5f, transform.position.y, (MinZ + MaxZ) * 0.5f);
        Vector3 size = new Vector3(MaxX - MinX, 0.1f, MaxZ - MinZ);
        Gizmos.DrawWireCube(center, size);
    }
}