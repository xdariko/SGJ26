using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamageCloud : MonoBehaviour
{
    [SerializeField] private float _damagePerTick = 5f;
    [SerializeField] private float _tickRate = 0.5f;
    [SerializeField] private float _duration = 3f;
    [SerializeField] private bool _destroyAfterDuration = true;

    private Coroutine _damageRoutine;

    public void Initialize(float damagePerTick, float tickRate, float duration)
    {
        _damagePerTick = damagePerTick;
        _tickRate = Mathf.Max(0.05f, tickRate);
        _duration = duration;
    }

    private void Awake()
    {
        Collider2D cloudCollider = GetComponent<Collider2D>();
        cloudCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        if (_destroyAfterDuration)
            Destroy(gameObject, _duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player == null || _damageRoutine != null)
            return;

        _damageRoutine = StartCoroutine(DamagePlayerLoop(player));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        if (_damageRoutine != null)
        {
            StopCoroutine(_damageRoutine);
            _damageRoutine = null;
        }
    }

    private IEnumerator DamagePlayerLoop(Player player)
    {
        while (player != null)
        {
            player.TakeDamage(_damagePerTick, gameObject);
            yield return new WaitForSeconds(_tickRate);
        }
    }
}
