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

    void Start()
    {
        controller = GetComponent<CharacterController>();
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
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);
        ui.UpdateHealth(currentHP, maxHP);
        Debug.Log("HP: " + currentHP);
    }
}
