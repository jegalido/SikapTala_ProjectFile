using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Dialogue/Speaker Animation Data")]
public class SpeakerAnimationData : ScriptableObject
{
    public string speakerName; // must match DialogueLine.speakerName exactly

    [Serializable]
    public class EmotionEntry
    {
        public EmotionType emotion;
        public Sprite[] frames;      // 
        [Min(1)]
        public int frameRate = 8;    // frames per second
    }

    public EmotionEntry[] emotionEntries;

    // Helper: find the entry for a given emotion, falls back to Neutral
    public EmotionEntry GetEntry(EmotionType emotion)
    {
        foreach (var e in emotionEntries)
            if (e.emotion == emotion) return e;

        // fallback to Neutral
        foreach (var e in emotionEntries)
            if (e.emotion == EmotionType.Neutral) return e;

        return emotionEntries.Length > 0 ? emotionEntries[0] : null;
    }
}