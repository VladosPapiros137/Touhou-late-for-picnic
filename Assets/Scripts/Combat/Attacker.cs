using UnityEngine;

public class Attacker : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;

    public float Damage
    {
        get => _damage;
        set => _damage = value;
    }
}