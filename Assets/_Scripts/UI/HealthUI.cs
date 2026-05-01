using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;

    private void Start()
    {
        var player = G.player;
        if (player != null)
        {
            player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
            healthFillImage.fillAmount = player.CurrentHealth / player.MaxHealth;
        }
    }

    private void OnDestroy()
    {
        G.player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthUI(healthEventArgs);
    }

    private void SetHealthUI(HealthEventArgs healthEventArgs)
    {
        healthFillImage.fillAmount = healthEventArgs.Current / healthEventArgs.Max;
    }
}