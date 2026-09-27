using UnityEngine;

public class FireflyParticle : MonoBehaviour
{
  // states that the fireflies are in
  // changes each frame
  private enum FireflyState
  {
    Approach,
    Hover,
    Attack,
    Dead,
  };

  [Header("References")]
  [SerializeField]
  private Transform _FireflyTarget;

  [SerializeField]
  private Transform _playerView;

  [Header("Firefly Movement Settings")]
  [SerializeField]
  private float _turnSpeed = 3f;

  [SerializeField]
  private float _moveSpeed = 3f;

  [SerializeField]
  private float _hoverRange = 4f;

  [SerializeField]
  private float _approachSpread = 1.5f;

  [SerializeField]
  private float _approachHeightSpread = 0.8f;

  [Header("Hover Settings")]
  [SerializeField]
  private float _hoverTime = 3f;

  [SerializeField]
  private float _hoverArcAngle = 40f;

  [SerializeField]
  private float _minHoverDistance = 1.5f;

  [SerializeField]
  private float _maxHoverDistance = 3f;

  [SerializeField]
  private float _minHeightOffset = -0.3f;

  [SerializeField]
  private float _maxHeightOffset = 0.5f;

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

  private FireflyState _currentState = FireflyState.Approach;
  private Vector3 _currentDirection;

  private float _hoverTimer;
  private float _hoverAngle;
  private float _hoverDistance;
  private float _hoverHeight;
  private float _bobSeed;
  private Vector3 _driftVelocity;

  private Vector3 _lungeTarget;
  private Vector3 _approachOffset;

  void Start()
  {
    Vector3 randomOffset = Random.insideUnitSphere;
    _approachOffset = new Vector3(
      randomOffset.x * _approachSpread,
      randomOffset.y * _approachHeightSpread,
      randomOffset.z * _approachSpread
    );
    _bobSeed = Random.Range(0f, 100f);
    if (_FireflyTarget != null)
    {
      _currentDirection = (_FireflyTarget.position - transform.position).normalized;
    }
    else
    {
      _currentDirection = transform.forward;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (_FireflyTarget == null || _playerView == null)
      return;

    switch (_currentState)
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
    Vector3 toTarget = _FireflyTarget.position - transform.position;

    if (toTarget.sqrMagnitude <= _hoverRange * _hoverRange)
    {
      EnterHover();
      return;
    }

    Vector3 aimPoint = _FireflyTarget.position + _approachOffset;
    Vector3 aimTo = aimPoint - transform.position;

    _currentDirection = Vector3.RotateTowards(
      _currentDirection,
      aimTo.normalized,
      _turnSpeed * Time.deltaTime,
      0f
    );

    transform.position += _currentDirection * _moveSpeed * Time.deltaTime;
  }

  private void EnterHover()
  {
    _currentState = FireflyState.Hover;
    _hoverTimer = _hoverTime;
    _hoverAngle = Random.Range(-_hoverArcAngle, _hoverArcAngle);
    _hoverDistance = Random.Range(_minHoverDistance, _maxHoverDistance);
    _hoverHeight = Random.Range(_minHeightOffset, _maxHeightOffset);
    _driftVelocity = Vector3.zero;
  }

  private void UpdateHover()
  {
    Vector3 forwardPlane = Vector3.ProjectOnPlane(_playerView.forward, Vector3.up);
    if (forwardPlane.sqrMagnitude < 0.0001f)
      forwardPlane = Vector3.ProjectOnPlane(_playerView.up, Vector3.up);
    forwardPlane.Normalize();

    Vector3 spotDirection = Quaternion.AngleAxis(_hoverAngle, Vector3.up) * forwardPlane;

    Vector3 hoverSpot =
      _FireflyTarget.position
      + spotDirection * _hoverDistance
      + Vector3.up * _hoverHeight
      + GetBobOffset();

    transform.position = Vector3.SmoothDamp(
      transform.position,
      hoverSpot,
      ref _driftVelocity,
      _smoothDriftTime
    );

    _hoverTimer -= Time.deltaTime;
    if (_hoverTimer <= 0f)
    {
      _lungeTarget = _FireflyTarget.position;
      _currentState = FireflyState.Attack;
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

    if ((_FireflyTarget.position - transform.position).sqrMagnitude <= _hitRadius * _hitRadius)
    {
      Debug.Log($"{name} hit the player");
      gameObject.SetActive(false);
      return;
    }

    if ((_lungeTarget - transform.position).sqrMagnitude < 0.0001f)
    {
      Debug.Log($"{name} missed");
      gameObject.SetActive(false);
    }
  }
}
