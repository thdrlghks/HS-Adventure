using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class P_Player : MonoBehaviour
{
    private CharacterController controller;
    public P_UI ui;
    private P_Attack attackSystem;
    private P_Move moveSystem;

    [Header("Health Settings")]
    public int maxHP = 200;
    public int currentHP;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 15f; // 초당 회복량
    public float staminaRegenDelay = 2.0f; // 행동 후 회복 대기 시간
    private float lastStaminaUseTime;

    private float lastHitTime = 0f;
    public float hitCooldown = 1f;

    private bool isDead = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        attackSystem = GetComponent<P_Attack>();
        moveSystem = GetComponent<P_Move>();

        // 초기화
        currentHP = maxHP;
        currentStamina = maxStamina;
        ui.UpdateHealth(currentHP, maxHP);
        ui.UpdateStamina(currentStamina, maxStamina);
    }
    void Update()
    {
        HandleStaminaRegen();
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            lastStaminaUseTime = Time.time;
            ui.UpdateStamina(currentStamina, maxStamina); 
            return true; // 성공
        }

        Debug.Log("스테미너 부족!");
        return false; // 실패
    }

    // ---------------------------------------------------------
    // 스테미너 지속 소모 (방패, 활 조준용)
    // ---------------------------------------------------------
    public bool UseStaminaContinuous(float amountPerSec)
    {
        if (currentStamina > 0)
        {
            currentStamina -= amountPerSec * Time.deltaTime;
            lastStaminaUseTime = Time.time;

            if (currentStamina < 0) currentStamina = 0;
            ui.UpdateStamina(currentStamina, maxStamina); 

            return true;
        }
        return false; // 스테미너 부족
    }

    void HandleStaminaRegen()
    {
        // 미사용 시 딜레이 이후 회복
        if (Time.time > lastStaminaUseTime + staminaRegenDelay && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            ui.UpdateStamina(currentStamina, maxStamina); 
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy"))
        {
            if (Time.time - lastHitTime > hitCooldown)
            {
                TakeDamage(20);
                lastHitTime = Time.time;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // 죽음

        if (moveSystem != null && moveSystem.IsInvincible) // 회피
        {
            Debug.Log("회피 무적! 데미지 무시됨");
            return;
        }
        float reduction = 0f;

        if (attackSystem != null && attackSystem.currentWeapon != null) // 실드
        {
            reduction = attackSystem.currentWeapon.GetDamageReductionRate();
        }
        int finalDamage = (int)(amount * (1f - reduction));
        currentHP -= finalDamage;
        currentHP = Mathf.Max(currentHP, 0);

        ui.UpdateHealth(currentHP, maxHP);
        Debug.Log($"받은 데미지: {amount}, 감소율: {reduction * 100}%, 최종 피해: {finalDamage}, 남은 HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die(); 
        }
    }
    void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망!");

        if (moveSystem != null) moveSystem.enabled = false;
        if (attackSystem != null) attackSystem.enabled = false;
        controller.enabled = false; // 물리 충돌 및 이동 정지

        // 커서 보이게 하기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (ui != null)
        {
            ui.ShowDeathPanel();
        }
    }
}
