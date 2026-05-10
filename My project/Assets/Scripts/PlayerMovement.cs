using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 5f;

    [Header("地面边界（与摄像机脚本保持一致）")]
    public Vector2 groundMin = new Vector2(0, 0);
    public Vector2 groundMax = new Vector2(20, 20);

    void Start()
    {
        // 设置初始位置：X=0, Z=0，Y=0（忽略高度，如需匹配地面高度可修改此 Y 值）
        transform.position = new Vector3(0, 0, 0);
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // 边界限制
        newPosition.x = Mathf.Clamp(newPosition.x, groundMin.x, groundMax.x);
        newPosition.z = Mathf.Clamp(newPosition.z, groundMin.y, groundMax.y);
        newPosition.y = 0f; // 保持在地面高度

        transform.position = newPosition;
    }
}