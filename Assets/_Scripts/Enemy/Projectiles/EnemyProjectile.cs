using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private bool _destroyOnTriggerWithoutPlayer = false;

    private Vector2 _direction;
    private float _speed;
    private float _damage;
    private float _lifetime;
    private GameObject _owner;
    private Rigidbody2D _rb;

    public void Initialize(Vector2 direction, float speed, float damage, float lifetime, GameObject owner)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
        _lifetime = lifetime;
        _owner = owner;

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.linearVelocity = _direction * _speed;

        Collider2D projectileCollider = GetComponent<Collider2D>();
        projectileCollider.isTrigger = true;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        if (_owner != null && (other.gameObject == _owner || other.transform.IsChildOf(_owner.transform)))
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            player.TakeDamage(_damage, _owner);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Enemy") || other.GetComponentInParent<Enemy>() != null)
            return;

        if (other.isTrigger && !_destroyOnTriggerWithoutPlayer)
            return;

        Destroy(gameObject);
    }
}
