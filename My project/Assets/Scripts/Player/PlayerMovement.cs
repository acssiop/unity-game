using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 5f;

    [Header("边界设置（从 PlayAreaBounds 获取）")]
    public PlayAreaBounds areaBounds;   // 拖拽挂有 PlayAreaBounds 的地面物体到这里

    [Header("旋转设置")]
    public float rotationSpeed = 10f;

    [Header("初始位置")]                       // 新增
    public Vector3 startPosition = new Vector3(0, 0.5f, 0);   // 新增：可在 Inspector 中修改

    void Start()
    {
        if (areaBounds == null)
            areaBounds = FindObjectOfType<PlayAreaBounds>();

        // 使用 Inspector 中设置的初始位置
        transform.position = startPosition;
    }

    void Update()
    {
        if (areaBounds == null)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // 边界限制（使用 PlayAreaBounds 提供的范围）
        newPosition.x = Mathf.Clamp(newPosition.x, areaBounds.MinX, areaBounds.MaxX);
        newPosition.z = Mathf.Clamp(newPosition.z, areaBounds.MinZ, areaBounds.MaxZ);
        newPosition.y = startPosition.y;   // 改为保持初始高度，而非写死 0

        transform.position = newPosition;

        // 旋转：面向移动方向
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}