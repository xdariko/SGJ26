using System.Collections;
using UnityEngine;
using DG.Tweening;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private float hitSoundVolume = 1f;

    private int hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");

    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;

    private float lerpAmount;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        materials = new Material[spriteRenderers.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
    }

    private float GetLerpValue()
    {
        return lerpAmount;
    }

    private void SetLerpValue(float newValue)
    {
        lerpAmount = newValue;
    }

    public void ApplyHitEffect()
    {
        PlayRandomHitSound();

        lerpAmount = 0f;
        DOTween.To(GetLerpValue, SetLerpValue, 1f, duration)
            .SetEase(Ease.OutExpo)
            .OnUpdate(OnLerpUpdate)
            .OnComplete(OnLerpComplete);
    }

    private void PlayRandomHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0)
            return;

        //SoundManagerSO.PlaySoundFXClip(hitSounds, transform.position, hitSoundVolume);
    }

    private void OnLerpUpdate()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(hitEffectAmount, GetLerpValue());
        }
    }

    private void OnLerpComplete()
    {
        DOTween.To(GetLerpValue, SetLerpValue, 0f, duration)
            .OnUpdate(OnLerpUpdate);
    }
}