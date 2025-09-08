using UnityEngine;
using System.Collections.Generic;

public class StageSpawner : MonoBehaviour
{
    public BulletPool bulletPool; // 🔗 reference the pool in Inspector
    public StageSequence stageSequence;

    private class RunningPattern
    {
        public StageEntry entry;
        public float timer = 0f;
        public float fireTimer = 0f;
        public float delayTimer = 0f;
    }

    private List<RunningPattern> running = new List<RunningPattern>();
    private int nextEntryIndex = 0;

    void Update()
    {
        if (stageSequence == null || stageSequence.entries.Length == 0) return;

        float dt = Time.deltaTime;

        // Start new entries
        if (nextEntryIndex < stageSequence.entries.Length)
        {
            StageEntry next = stageSequence.entries[nextEntryIndex];
            var rp = new RunningPattern { entry = next, delayTimer = next.startDelay };
            running.Add(rp);
            nextEntryIndex++;
        }

        // Update all running patterns
        for (int i = running.Count - 1; i >= 0; i--)
        {
            RunningPattern rp = running[i];
            rp.timer += dt;

            if (rp.delayTimer > 0f)
            {
                rp.delayTimer -= dt;
                continue;
            }

            rp.fireTimer += dt;

            if (rp.fireTimer >= rp.entry.pattern.fireRate)
            {
                // 🔥 Tell pattern to use pool
                rp.entry.pattern.Fire(transform, bulletPool, Time.time);
                rp.fireTimer = 0f;
            }

            if (rp.timer >= rp.entry.duration + rp.entry.startDelay)
            {
                running.RemoveAt(i);
            }
        }
    }
}