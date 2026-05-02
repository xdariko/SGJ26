using System;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 30f;
    [SerializeField] private bool _destroyOnDeath = true;
    [SerializeField] private float _destroyDelay = 0.1f;

    private float _currentHealth;
    private bool _isDead;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float HealthPercent => _maxHealth <= 0f ? 0f : _currentHealth / _maxHealth;
    public bool IsDead => _isDead;

    public event Action<float, float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead || damage <= 0f)
            return;

        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        float delta = _currentHealth - oldHealth;

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, delta);

        if (_currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (_isDead || amount <= 0f)
            return;

        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        float delta = _currentHealth - oldHealth;

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, delta);
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
            enemy.MoveEnemy(Vector2.zero);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
            collider.enabled = false;

        OnDeath?.Invoke();

        if (_destroyOnDeath)
            Destroy(gameObject, _destroyDelay);
    }
}
