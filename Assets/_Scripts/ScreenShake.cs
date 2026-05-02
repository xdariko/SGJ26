using Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        G.screenShake = this;

        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float force = 1f)
    {
        if (impulseSource == null)
            return;

        impulseSource.GenerateImpulse(force);
    }
}