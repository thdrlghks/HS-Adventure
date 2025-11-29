using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class P_Player : MonoBehaviour
{
    public int maxHP = 200;
    public int currentHP;
    public P_UI ui;

    private float lastHitTime = 0f;
    public float hitCooldown = 1f;

    private CharacterController controller;

    private P_Attack attackSystem;
    private P_Move moveSystem;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        attackSystem = GetComponent<P_Attack>();
        moveSystem = GetComponent<P_Move>();

        currentHP = maxHP;
        ui.UpdateHealth(currentHP, maxHP);
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

    void TakeDamage(int amount)
    {
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
            // 사망 처리 (필요시 추가)
            // Die(); 
        }
    }
}
