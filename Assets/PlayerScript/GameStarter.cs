using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [Header("UI")]
    public GameObject startPanel; // 시작 시 띄울 패널 (설명서 등)

    // 다른 스크립트에서 접근할 수 있도록 static 변수로 선언
    public static bool isGameRunning = false;

    void Awake()
    {
        // 게임 시작 전 초기화
        PauseGame();
    }

    void Update()
    {
        // 게임이 아직 시작되지 않았고, ESC를 눌렀을 때
        if (!isGameRunning && Input.GetKeyDown(KeyCode.Escape))
        {
            StartGame();
        }
    }

    void PauseGame()
    {
        isGameRunning = false;
        Time.timeScale = 0f; // 게임 시간 정지 (물리, 애니메이션 등 멈춤)

        if (startPanel != null)
            startPanel.SetActive(true); // 패널 보이기
    }

    void StartGame()
    {
        isGameRunning = true;
        Time.timeScale = 1f; // 게임 시간 정상화

        if (startPanel != null)
            startPanel.SetActive(false); // 패널 숨기기
    }
}