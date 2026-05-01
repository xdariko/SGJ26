using UnityEngine;

[CreateAssetMenu(menuName = "Game/Story Sequence")]
public class StorySequenceData : ScriptableObject
{
    public string id;
    public StoryLine[] lines;
}