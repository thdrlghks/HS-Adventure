using UnityEngine;

public class P_Camara : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform cameraPivot;        // 카메라 중심 (플레이어 머리 근처)
    public float mouseSensitivity = 3f;  // 마우스 감도
    public float distance = 4f;          // 기본 거리
    public float minDistance = 1.2f;     // 최소 거리
    public float maxDistance = 6f;       // 최대 거리
    public float zoomSpeed = 2f;         // 줌 속도 (마우스 휠)

    [Header("회전 제한")]
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("부드러움")]
    public float smoothSpeed = 10f;

    [Header("충돌 감지")]
    public LayerMask collisionMask;

    private float pitch = 10f;
    private float yaw;
    private float targetDistance;
    private float currentDistance;

    void Start()
    {
        targetDistance = distance;
        currentDistance = distance;
        if (cameraPivot == null)
            Debug.LogWarning("⚠️ TPS_Camera: cameraPivot을 지정하세요!");
    }

    void LateUpdate()
    {
        if (!cameraPivot) return;

        HandleRotation();
        HandleZoom();
        HandleCollision();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 회전 적용
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleZoom()
    {
        // 🌀 마우스 휠 입력
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // 부드럽게 보간
        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * 8f);
    }

    void HandleCollision()
    {
        // 충돌 전 목표 위치
        Vector3 desiredPos = cameraPivot.position - cameraPivot.forward * distance;

        // 충돌 감지
        RaycastHit hit;
        if (Physics.Linecast(cameraPivot.position, desiredPos, out hit, collisionMask))
        {
            float hitDist = Vector3.Distance(cameraPivot.position, hit.point) - 0.3f;
            currentDistance = Mathf.Clamp(hitDist, minDistance, distance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * 5f);
        }

        // 최종 카메라 위치
        Vector3 finalPos = cameraPivot.position - cameraPivot.forward * currentDistance;
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);

        // 피벗 바라보기
        transform.LookAt(cameraPivot.position + Vector3.up * 0.5f);
    }
}
