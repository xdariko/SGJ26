using UnityEngine;

public class StoryStarter : MonoBehaviour
{
    [SerializeField] private StorySequenceData introSequence;

    private void Start()
    {
        if (G.storyPanel != null)
        {
            G.storyPanel.PlaySequence(introSequence, OnStoryFinished);
        }
    }

    private void OnStoryFinished()
    {
        Debug.Log("Сюжетная сцена закончилась");
    }
}