using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth _enemyHealth;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Canvas _canvas;

    [Header("Behaviour")]
    [SerializeField] private bool _hideWhenFull = true;
    [SerializeField] private bool _faceCamera = true;

    private Camera _camera;

    private void Awake()
    {
        if (_enemyHealth == null)
            _enemyHealth = GetComponentInParent<EnemyHealth>();

        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        _camera = Camera.main;
    }

    private void OnEnable()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnHealthChanged += HandleHealthChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        if (!_faceCamera || _camera == null)
            return;

        transform.rotation = _camera.transform.rotation;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth, float deltaHealth)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_enemyHealth == null || _fillImage == null)
            return;

        float percent = _enemyHealth.HealthPercent;
        _fillImage.fillAmount = percent;

        if (_canvas != null && _hideWhenFull)
            _canvas.enabled = percent < 1f && percent > 0f;
        else if (_canvas != null)
            _canvas.enabled = percent > 0f;
    }
}
