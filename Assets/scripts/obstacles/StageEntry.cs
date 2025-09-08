
using System;
using UnityEngine;

[System.Serializable]
public class StageEntry
{
  public ObstaclePattern pattern;
  public float duration = 5f; // ow long this pattern runs
  public float startDelay = 0f; //optional: wait before firing
}