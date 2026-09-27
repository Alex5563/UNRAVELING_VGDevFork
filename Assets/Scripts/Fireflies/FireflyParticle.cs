using UnityEngine;
using UnityEngine.Pool;

public class FireflyParticle : MonoBehaviour
{
  // states that the fireflies are in
  // changes each frame
  private enum FireflyState
  {
    Approach,
    Hover,
    Attack,
  };

  [Header("Firefly Movement Settings")]
  [SerializeField]
  private float _moveSpeed = 3f;

  [SerializeField]
  private float _hoverRange = 4f;

  [SerializeField]
  private float _laneSmoothTime = 0.5f;

  [Header("Hover Settings")]
  [SerializeField]
  private float _hoveringTime = 3f;

  [SerializeField]
  private float _minHoverDistance = 1.5f;

  [SerializeField]
  private float _maxHoverDistance = 3f;

  [SerializeField]
  private float _bobAmp = 0.15f;

  [SerializeField]
  private float _bobFreq = 1f;

  [SerializeField]
  private float _smoothDriftTime = 0.4f;

  [Header("Lunge Settings")]
  [SerializeField]
  private float _lungeSpeed = 12f;

  [SerializeField]
  private float _hitRadius = 0.4f;

  [Header("Safety")]
  [SerializeField]
  private float _maxLifetime = 20f;

  private IObjectPool<FireflyParticle> _pool;
  private bool _released;

  private Transform _playerTarget;
  private Transform _hallway;
  private Vector2 _lane;
  private FireflyState _state;

  private Vector2 _laneVelocity;
  private float _hoverTime;
  private float _hoverDistance;
  private float _hoverSide;
  private float _bobSeed;
  private Vector3 _driftVelocity;
  private Vector3 _lungeTarget;
  private float _lifetime;

  public void SetPool(IObjectPool<FireflyParticle> owningPool) => _pool = owningPool;

  public void init(Vector2 assignedLane, Transform target, Transform hallwayRef)
  {
    _lane = assignedLane;
    _playerTarget = target;
    _hallway = hallwayRef;

    _state = FireflyState.Approach;
    _released = false;
    _laneVelocity = Vector2.zero;
    _driftVelocity = Vector2.zero;
    _hoverTime = 0f;
    _lifetime = 0f;
    _bobSeed = Random.Range(0f, 100f);
  }

  private void Despawn()
  {
    if (_released)
      return;
    _released = true;

    if (_pool != null)
      _pool.Release(this);
    else
      Destroy(gameObject);
  }

  private void Update()
  {
    if (_playerTarget == null || _hallway == null)
      return;

    _lifetime += Time.deltaTime;
    if (_lifetime >= _maxLifetime)
    {
      Despawn();
      return;
    }

    switch (_state)
    {
      case FireflyState.Approach:
        UpdateApproach();
        break;
      case FireflyState.Attack:
        UpdateAttack();
        break;
      case FireflyState.Hover:
        UpdateHover();
        break;
    }
  }

  private void UpdateApproach()
  {
    Vector3 localPosition = _hallway.InverseTransformPoint(transform.position);
    float playerZ = _hallway.InverseTransformPoint(_playerTarget.position).z;

    if (Mathf.Abs(playerZ - localPosition.z) <= _hoverRange)
    {
      EnterHover();
      return;
    }

    Vector2 xy = Vector2.SmoothDamp(
      new Vector2(localPosition.x, localPosition.y),
      _lane,
      ref _laneVelocity,
      _laneSmoothTime
    );
    float z = Mathf.MoveTowards(localPosition.z, playerZ, _moveSpeed * Time.deltaTime);

    transform.position = _hallway.TransformPoint(new Vector3(xy.x, xy.y, z));
  }

  private void EnterHover()
  {
    _state = FireflyState.Hover;
    _hoverTime = _hoveringTime;
    _hoverDistance = Random.Range(_minHoverDistance, _maxHoverDistance);

    float myZ = _hallway.InverseTransformPoint(transform.position).z;
    float playerZ = _hallway.InverseTransformPoint(_playerTarget.position).z;
    _hoverSide = Mathf.Sign(myZ - playerZ);

    _driftVelocity = Vector3.zero;
  }

  private void UpdateHover()
  {
    float playerZ = _hallway.InverseTransformPoint(_playerTarget.position).z;
    Vector3 localSpot = new Vector3(_lane.x, _lane.y, playerZ + _hoverSide * _hoverDistance);
    Vector3 spot = _hallway.InverseTransformPoint(localSpot) + GetBobOffset();

    transform.position = Vector3.SmoothDamp(
      transform.position,
      spot,
      ref _driftVelocity,
      _smoothDriftTime
    );

    _hoverTime -= Time.deltaTime;
    if (_hoverTime <= 0)
    {
      _lungeTarget = _playerTarget.position;
      _state = FireflyState.Attack;
    }
  }

  private Vector3 GetBobOffset()
  {
    float y_coord = Time.time * _bobFreq;

    float x = Mathf.PerlinNoise(_bobSeed, y_coord) * 2f - 1f;
    float y = Mathf.PerlinNoise(_bobSeed + 701.901f, y_coord) * 2f - 1f;
    float z = Mathf.PerlinNoise(_bobSeed + 673.941f, y_coord) * 2f - 1f;

    return new Vector3(x, y, z) * _bobAmp;
  }

  private void UpdateAttack()
  {
    transform.position = Vector3.MoveTowards(
      transform.position,
      _lungeTarget,
      _lungeSpeed * Time.deltaTime
    );

    if ((_playerTarget.position - transform.position).sqrMagnitude <= _hitRadius * _hitRadius)
    {
      Debug.Log($"{name} hit the player");
      Despawn();
      return;
    }

    if ((_lungeTarget - transform.position).sqrMagnitude < 0.0001f)
    {
      Debug.Log($"{name} missed");
      Despawn();
    }
  }
}
