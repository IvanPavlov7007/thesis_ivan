using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Out-of-Bounds Detector respawns the player at the spawn point
/// </summary>
public class BackToSpawnResetter : MonoBehaviour
{
    [SerializeField]
    private float minY, maxY;
    [SerializeField]
    Transform player;
    [SerializeField]
    Vector3 spawnPoint;


    private void Update()
    {
        float y = player.transform.position.y;
        if (y > maxY || y < minY)
            player.transform.position = spawnPoint;
    }
}
