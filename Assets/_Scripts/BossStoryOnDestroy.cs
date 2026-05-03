using UnityEngine;

public class BossStoryOnDestroy : MonoBehaviour
{
    [SerializeField] private int bossIndex = 1;

    private bool wasCalled;
    private bool applicationQuitting;

    private void OnApplicationQuit()
    {
        applicationQuitting = true;
    }

    private void OnDestroy()
    {
        if (applicationQuitting)
            return;

        if (wasCalled)
            return;

        wasCalled = true;

        if (G.main == null)
        {
            Debug.LogWarning("BossStoryOnDestroy: G.main is null. Cannot start boss story.");
            return;
        }

        switch (bossIndex)
        {
            case 1:
                G.main.OnBoss1Killed();
                break;

            case 2:
                G.main.OnBoss2Killed();
                break;

            case 3:
                G.main.OnBoss3Killed();
                break;

            default:
                Debug.LogWarning($"BossStoryOnDestroy: Unknown boss index {bossIndex}.", this);
                break;
        }
    }
}