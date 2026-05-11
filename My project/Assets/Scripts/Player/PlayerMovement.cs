using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 5f;

    [Header("边界设置（从 PlayAreaBounds 获取）")]
    public PlayAreaBounds areaBounds;

    [Header("旋转速度")]
    public float rotationSpeed = 10f;

    [Header("初始位置")]
    public Vector3 startPosition = new Vector3(0, 0.5f, 0);

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        // 获取或自动添加刚体
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // 关键：冻结旋转，防止物理碰撞导致倾斜或旋转
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        // 禁用重力（如果需要地面行走，可保留并改用其它重力处理）
        rb.useGravity = false;

        // 寻找场景中的边界脚本
        if (areaBounds == null)
            areaBounds = FindObjectOfType<PlayAreaBounds>();

        // 设置初始位置
        transform.position = startPosition;
    }

    void Update()
    {
        // 只在 Update 中收集输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        if (areaBounds == null)
            return;

        // 移动：直接设置速度（更干净、即时）
        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        // 旋转：面向移动方向
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // 停止输入时立刻消除水平速度，避免惯性滑动
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // 边界限制（通过 MovePosition 保持位置约束）
        Vector3 clampedPos = rb.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, areaBounds.MinX, areaBounds.MaxX);
        clampedPos.z = Mathf.Clamp(clampedPos.z, areaBounds.MinZ, areaBounds.MaxZ);
        clampedPos.y = startPosition.y; // 保持高度恒定
        rb.MovePosition(clampedPos);
    }
}