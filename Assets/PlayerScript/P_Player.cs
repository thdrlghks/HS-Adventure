using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class P_Player : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 200;
    public int currentHP;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 15f;   // 초당 회복량
    public float staminaRegenDelay = 2.0f; // 행동 후 회복 대기 시간
    private float lastStaminaUseTime;

    public P_UI ui;
    private float lastHitTime = 0f;
    public float hitCooldown = 1f;

    private bool isDead = false;

    private CharacterController controller;

    private P_Attack attackSystem;
    private P_Move moveSystem;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        attackSystem = GetComponent<P_Attack>();
        moveSystem = GetComponent<P_Move>();

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
            return true; // 사용 성공
        }

        Debug.Log("스테미너 부족!");
        return false; // 사용 실패
    }

    // ---------------------------------------------------------
    // 🔥 [핵심 2] 스테미너 지속 소모 (방패, 활 조준용)
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
        return false; // 스테미너 0 됨
    }

    void HandleStaminaRegen()
    {
        // 마지막 사용 후 딜레이가 지났고, 꽉 차있지 않다면 회복
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
        if (isDead) return;
        if (moveSystem != null && moveSystem.IsInvincible)
        {
            Debug.Log("회피 무적! 데미지 무시됨");
            return;
        }
        float reduction = 0f;

        if (attackSystem != null && attackSystem.currentWeapon != null)
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

        // 1. 더 이상 움직이거나 공격하지 못하게 컴포넌트 비활성화
        if (moveSystem != null) moveSystem.enabled = false;
        if (attackSystem != null) attackSystem.enabled = false;
        controller.enabled = false; // 물리 충돌 및 이동 정지

        // (선택사항) 사망 애니메이션 실행
        // Animator anim = GetComponentInChildren<Animator>();
        // if(anim != null) anim.SetTrigger("Die");

        // 2. 마우스 커서 보이게 하기 (버튼 클릭을 위해)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 3. UI 패널 띄우기
        if (ui != null)
        {
            ui.ShowDeathPanel();
        }
    }
}
