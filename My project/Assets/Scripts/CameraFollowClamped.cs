using UnityEngine;

public class CameraFollowClamped : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("地面边界（世界坐标 XZ 平面）")]
    public Vector2 groundMin = new Vector2(0, 0);   // 地面左下角
    public Vector2 groundMax = new Vector2(20, 20); // 地面右上角

    [Header("垂直向下的偏移（Y 轴高度）")]
    public float heightAboveGround = 10f;

    private Camera cam;
    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 设置垂直向下视角
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // 计算视野半尺寸
        if (cam.orthographic)
        {
            // 正交摄像机：orthographicSize 是垂直半高
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
        else
        {
            // 透视摄像机：根据高度和 FOV 计算
            float halfFovRad = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            halfHeight = heightAboveGround * Mathf.Tan(halfFovRad);
            halfWidth = halfHeight * cam.aspect;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 计算目标期望位置（保持摄像机当前高度）
        Vector3 desiredPos = target.position;
        desiredPos.y = heightAboveGround;

        // 2. 根据视野半尺寸限制 X 和 Z 坐标
        float minX = groundMin.x + halfWidth;
        float maxX = groundMax.x - halfWidth;
        float minZ = groundMin.y + halfHeight;   // 注意 groundMin.y 对应 Z 轴
        float maxZ = groundMax.y - halfHeight;

        // 如果边界范围无效（视野比地面还大），则取地面中心
        if (minX > maxX) desiredPos.x = (groundMin.x + groundMax.x) * 0.5f;
        else desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);

        if (minZ > maxZ) desiredPos.z = (groundMin.y + groundMax.y) * 0.5f;
        else desiredPos.z = Mathf.Clamp(desiredPos.z, minZ, maxZ);

        // 3. 应用位置
        transform.position = desiredPos;
    }
}