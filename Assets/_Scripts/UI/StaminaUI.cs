using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Image staminaFillImage;

    private void Start()
    {
        var player = G.player;
        if (player != null)
        {
            player.staminaEvent.OnStaminaChanged += StaminaEvent_OnStaminaChanged;
            staminaFillImage.fillAmount = player.CurrentStamina / player.MaxStamina;
        }

    }

    private void OnDestroy()
    {
        G.player.staminaEvent.OnStaminaChanged -= StaminaEvent_OnStaminaChanged;
    }

    private void StaminaEvent_OnStaminaChanged(StaminaEvent staminaEvent, StaminaEventArgs staminaEventArgs)
    {
        SetStaminaUI(staminaEventArgs);
    }

    private void SetStaminaUI(StaminaEventArgs staminaEventArgs)
    {
        staminaFillImage.fillAmount = staminaEventArgs.Current / staminaEventArgs.Max;
    }
}
