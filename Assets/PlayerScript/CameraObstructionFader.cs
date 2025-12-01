using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraObstructionFader : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget; // 플레이어의 머리나 가슴 쪽 Transform
    public LayerMask obstacleLayer; // 투명하게 만들 장애물 레이어 (예: Default, Wall)

    [Header("Settings")]
    public float fadeSpeed = 5f; // 투명해지는 속도
    public float targetAlpha = 0.2f; // 목표 투명도 (0~1)

    // 현재 투명해진 오브젝트들을 기억하는 리스트
    private List<Renderer> _fadedRenderers = new List<Renderer>();

    void LateUpdate()
    {
        FadeObstacles();
    }

    void FadeObstacles()
    {
        // 1. 카메라에서 플레이어까지의 거리와 방향 계산
        Vector3 direction = playerTarget.position - transform.position;
        float distance = direction.magnitude;

        // 2. RaycastAll로 플레이어와 카메라 사이의 모든 장애물 감지
        // (Raycast 대신 SphereCast를 쓰면 얇은 기둥도 잘 감지합니다)
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, obstacleLayer);

        // 이번 프레임에 감지된 렌더러들을 담을 임시 리스트
        HashSet<Renderer> currentHitRenderers = new HashSet<Renderer>();

        // 3. 감지된 장애물 투명하게 만들기
        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();

            // 플레이어 자신은 투명해지면 안 됨 & 렌더러가 있어야 함
            if (rend != null && hit.collider.transform != playerTarget.root)
            {
                currentHitRenderers.Add(rend);

                // 기존 리스트에 없던 녀석이면 투명화 코루틴 시작
                if (!_fadedRenderers.Contains(rend))
                {
                    _fadedRenderers.Add(rend);
                    StartCoroutine(FadeObject(rend, targetAlpha));
                }
            }
        }

        // 4. 더 이상 가리지 않는 장애물 원상복구
        // 리스트를 역순으로 순회해야 삭제 시 오류가 안 남
        for (int i = _fadedRenderers.Count - 1; i >= 0; i--)
        {
            Renderer rend = _fadedRenderers[i];

            // 이번 레이캐스트에 안 걸렸다면? -> 다시 불투명하게
            if (!currentHitRenderers.Contains(rend))
            {
                StartCoroutine(FadeObject(rend, 1.0f));
                _fadedRenderers.RemoveAt(i);
            }
        }
    }

    // 알파값을 부드럽게 조절하는 코루틴
    IEnumerator FadeObject(Renderer rend, float targetAlpha)
    {
        Material mat = rend.material;
        Color initialColor = mat.color;
        float currentAlpha = initialColor.a;

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            if (rend == null) yield break; // 오브젝트가 파괴되었을 경우 방어

            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            Color newColor = new Color(initialColor.r, initialColor.g, initialColor.b, currentAlpha);
            mat.color = newColor;

            yield return null;
        }

        // 최종 값 확정
        mat.color = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);
    }
}