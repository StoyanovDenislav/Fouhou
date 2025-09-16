using System;
using UnityEngine;

[System.Serializable]
public class StageEntry
{
    public ObstaclePattern pattern;
    public float duration = 5f;
    public float startDelay = 0f; // Individual pattern delay within the group
}