using UnityEngine;

[CreateAssetMenu(fileName = "BombManData", menuName = "Scriptable Objects/BombManData")]
public class BombManData : ScriptableObject
{
    private float _hp = 10f;
    private float _explodeDamage = 20f;
    private float _crashDamage = 3f;
}
