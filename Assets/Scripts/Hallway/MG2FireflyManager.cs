using UnityEngine;

public class MG2FireflyManager : FireflyManager
{
    private int counter = 0;
    protected override void SpawnFirefly()
    {
        if (!Minigame2.getCanSpawnFirefly() || _fireflyPrefab == null || _spawnPoint == null || _playerTarget == null || _hallway == null)
        {
            Debug.Log("no spawn " + ++counter);
            return;
        }
        Vector2 lane = PickLane();
        FireflyParticle f = _pool.Get();
        f.transform.SetPositionAndRotation(_spawnPoint.position, Quaternion.identity);
        f.init(lane, _playerTarget, _hallway);
    }
}
