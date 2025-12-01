using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [Header("UI")]
    public GameObject startPanel; // 시작 패널 ( 키 설명 패널 )

    // 다른 스크립트에서 접근할 수 있도록 static 변수
    public static bool isGameRunning = false;

    void Awake()
    {
        // 게임 시작 전 멈춤 
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
        Time.timeScale = 0f; // 정지

        if (startPanel != null)
            startPanel.SetActive(true); // 시작 패널 보이기
    }

    void StartGame()
    {
        isGameRunning = true;
        Time.timeScale = 1f; // 시작

        if (startPanel != null)
            startPanel.SetActive(false); // 패널 숨기기
    }
}