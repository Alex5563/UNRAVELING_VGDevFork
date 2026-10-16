using UnityEngine;
using UnityEngine.Pool;

public class CentipedeManager : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  private Centipede _centipedePrefab;

  [SerializeField]
  private Transform _spawnPoint;

  [SerializeField]
  private Transform _playerTarget;

  [SerializeField]
  private LayerMask _surfaceMask;

  [Header("Spawning")]
  [SerializeField]
  private float _spawnInterval = 3f;

  [SerializeField]
  private float _attachSearchDistance = 5f;

  [Header("Pool")]
  [SerializeField]
  private int _defaultCapacity = 8;

  [SerializeField]
  private int _maxPoolSize = 32;

  private static readonly Vector3[] _searchDirections =
  {
    Vector3.down,
    Vector3.up,
    Vector3.left,
    Vector3.right,
    Vector3.forward,
    Vector3.back,
  };

  private ObjectPool<Centipede> _pool;
  private float _spawnTimer;

  private void Awake()
  {
    _pool = new ObjectPool<Centipede>(
      CreateCentipede,
      c => c.gameObject.SetActive(true),
      c => c.gameObject.SetActive(false),
      c => Destroy(c.gameObject),
      collectionCheck: true,
      defaultCapacity: _defaultCapacity,
      maxSize: _maxPoolSize
    );
  }

  private Centipede CreateCentipede()
  {
    Centipede c = Instantiate(_centipedePrefab);
    c.gameObject.SetActive(false);
    c.SetPool(_pool);
    return c;
  }

  private void Update()
  {
    _spawnTimer -= Time.deltaTime;
    if (_spawnTimer <= 0f)
    {
      _spawnTimer = _spawnInterval;
      SpawnCentipede();
    }
  }

  private void SpawnCentipede()
  {
    if (_centipedePrefab == null || _spawnPoint == null || _playerTarget == null)
      return;

    if (!FindNearestSurface(_spawnPoint.position, out RaycastHit hit))
    {
      Debug.LogWarning($"{name}: no surface near the spawn point to attach a centipede to");
      return;
    }

    Vector3 toPlayer = _playerTarget.position - hit.point;

    Centipede c = _pool.Get();
    c.Init(_playerTarget, hit.point, hit.normal, toPlayer);
  }

  private bool FindNearestSurface(Vector3 origin, out RaycastHit best)
  {
    best = default;
    float bestDistance = float.MaxValue;
    bool found = false;

    foreach (Vector3 dir in _searchDirections)
    {
      if (
        Physics.Raycast(origin, dir, out RaycastHit hit, _attachSearchDistance, _surfaceMask)
        && hit.distance < bestDistance
      )
      {
        best = hit;
        bestDistance = hit.distance;
        found = true;
      }
    }

    return found;
  }
}
