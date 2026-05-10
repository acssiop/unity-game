using UnityEngine;

public class CameraFollowClamped : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("边界设置（从 PlayAreaBounds 获取）")]
    public PlayAreaBounds areaBounds;   // 拖拽挂有 PlayAreaBounds 的地面物体到这里

    [Header("垂直向下的偏移（Y 轴高度）")]
    public float heightAboveGround = 10f;

    private Camera cam;
    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 如果没有手动拖拽，尝试自动查找场景中的 PlayAreaBounds
        if (areaBounds == null)
            areaBounds = FindObjectOfType<PlayAreaBounds>();

        // 设置垂直向下视角
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 计算视野半尺寸
        if (cam.orthographic)
        {
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
        else
        {
            float halfFovRad = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            halfHeight = heightAboveGround * Mathf.Tan(halfFovRad);
            halfWidth = halfHeight * cam.aspect;
        }
    }

    void LateUpdate()
    {
        if (target == null || areaBounds == null)
            return;

        // 目标期望位置（保持摄像机高度）
        Vector3 desiredPos = target.position;
        desiredPos.y = heightAboveGround;

        // 根据视野半尺寸限制 X 和 Z 坐标
        float minX = areaBounds.MinX + halfWidth;
        float maxX = areaBounds.MaxX - halfWidth;
        float minZ = areaBounds.MinZ + halfHeight;   // 注意 MinZ 对应世界 Z 轴
        float maxZ = areaBounds.MaxZ - halfHeight;

        // 如果边界范围无效（视野比地面还大），则取地面中心
        if (minX > maxX)
            desiredPos.x = (areaBounds.MinX + areaBounds.MaxX) * 0.5f;
        else
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);

        if (minZ > maxZ)
            desiredPos.z = (areaBounds.MinZ + areaBounds.MaxZ) * 0.5f;
        else
            desiredPos.z = Mathf.Clamp(desiredPos.z, minZ, maxZ);

        // 应用位置
        transform.position = desiredPos;
    }
}