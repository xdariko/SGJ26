using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro _text;

    [Header("Animation")]
    [SerializeField] private float _moveDuration = 0.65f;
    [SerializeField] private float _moveUpDistance = 1.2f;
    [SerializeField] private float _horizontalRandomRange = 0.35f;
    [SerializeField] private float _startScale = 0.6f;
    [SerializeField] private float _endScale = 1f;

    [Header("Visual")]
    [SerializeField] private Color _damageColor = Color.white;

    private Sequence _sequence;

    public void Setup(float damage)
    {
        if (_text == null)
            _text = GetComponentInChildren<TextMeshPro>();

        if (_text == null)
        {
            Destroy(gameObject);
            return;
        }

        int roundedDamage = Mathf.RoundToInt(damage);
        _text.text = roundedDamage.ToString();
        _text.color = _damageColor;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(
            Random.Range(-_horizontalRandomRange, _horizontalRandomRange),
            _moveUpDistance,
            0f
        );

        transform.localScale = Vector3.one * _startScale;

        _sequence?.Kill();

        _sequence = DOTween.Sequence();
        _sequence.Append(transform.DOMove(endPosition, _moveDuration).SetEase(Ease.OutQuad));
        _sequence.Join(transform.DOScale(_endScale, 0.15f).SetEase(Ease.OutBack));
        _sequence.Join(_text.DOFade(0f, _moveDuration));
        _sequence.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
}