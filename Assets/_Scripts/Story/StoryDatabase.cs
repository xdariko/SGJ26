using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Story Database")]
public class StoryDatabase : ScriptableObject
{
    [SerializeField] private StorySequenceData[] sequences;

    private Dictionary<string, StorySequenceData> cache;

    public StorySequenceData GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        BuildCacheIfNeeded();

        cache.TryGetValue(id, out var sequence);
        return sequence;
    }

    private void BuildCacheIfNeeded()
    {
        if (cache != null)
            return;

        cache = new Dictionary<string, StorySequenceData>();

        if (sequences == null)
            return;

        foreach (var sequence in sequences)
        {
            if (sequence == null)
                continue;

            if (string.IsNullOrEmpty(sequence.id))
            {
                Debug.LogWarning($"Story sequence without id: {sequence.name}", sequence);
                continue;
            }

            if (cache.ContainsKey(sequence.id))
            {
                Debug.LogWarning($"Duplicate story id: {sequence.id}", sequence);
                continue;
            }

            cache.Add(sequence.id, sequence);
        }
    }
}