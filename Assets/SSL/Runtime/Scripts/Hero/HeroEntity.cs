using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_movementSettings")]
    [SerializeField] private HeroHorizontalMovementSettings _groundHorizontalMovementSettings;
    [SerializeField] private HeroHorizontalMovementSettings _airHorizontalMovementSettings;

    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround { get; private set; } = false;

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling
    }
    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;
    public bool isJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool isJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;

    public bool isJumping => _jumpState != JumpState.NotJumping;


    [Header("Dash")]
    [SerializeField] private HeroDashSettings _dashSettings;

    private DashState _dashState = DashState.NotDashing;
    private float _timeSinceDash = 0f;
    private float _dashOrient = 1f;
    public bool canDash => _timeSinceDash >= _dashSettings.cooldown;
    public bool isDashing => _dashState != DashState.NotDashing;

    enum DashState
    {
        Dashing,
        NotDashing
    }

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    public void DashStart()
    {
        if (canDash)
        {
            _dashOrient = _orientX;
            _dashState = DashState.Dashing;
            _timeSinceDash = 0f;
        }
    }

    private void ClampHorizontalSpeed()
    {
        _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, -_airHorizontalMovementSettings.speedMax, _airHorizontalMovementSettings.speedMax);
    }
    private void _UpdateDash()
    {
        if (_timeSinceDash < _dashSettings.duration)
        {
            _horizontalSpeed = _dashSettings.speed;
        }
        else
        {
            ClampHorizontalSpeed();
            _dashState = DashState.NotDashing;
        }
    }
    private HeroHorizontalMovementSettings _GetCurrentHorizontalMovementSettings()
    {
        return IsTouchingGround ? _groundHorizontalMovementSettings : _airHorizontalMovementSettings;
    }
    public void JumpStart()
    {
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings.jumpSpeed;
        }
        else
        {
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if (!IsTouchingGround)
        {
            _ApplyFallGravity(_jumpFallSettings);
        }
        else
        {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    private void _UpdateJump()
    {
        switch (_jumpState)
        {
            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;
        }
    }
    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }

    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }
    private void _Accelerate(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax)
        {
            _horizontalSpeed = settings.speedMax;
        }
    }

    private void _TurnBack(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFrictions * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }
    private bool _AreOrientAndMovementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }
    private void _Decelerate(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
        }
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementSettings settings)
    {
        if (_moveDirX != 0f)
        {
            _Accelerate(settings);
        }
        else
        {
            _Decelerate(settings);
        }
    }

    #region Functions Move Dir

    public float GetMoveDirX()
    {
        return _moveDirX;
    }
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    #endregion
    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        HeroHorizontalMovementSettings horizontalMovementSettings = _GetCurrentHorizontalMovementSettings();
        _timeSinceDash += Time.fixedDeltaTime;
        if (isDashing)
        {
            _UpdateDash();
        }
        else
        {
            if (_AreOrientAndMovementOpposite())
            {
                _TurnBack(horizontalMovementSettings);
            }
            else
            {
                _UpdateHorizontalSpeed(horizontalMovementSettings);
                _ChangeOrientFromHorizontalMovement();
            }
        }


        if (isJumping)
        {
            _UpdateJump();
        }
        else
        {
            if (!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            }
            else
            {
                _ResetVerticalSpeed();
            }
        }

        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();

    }

    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax)
        {
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }

    private void _ApplyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }




    private void Update()
    {
        _UpdateOrientVisual();

    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }
    private void _ChangeOrientFromHorizontalMovement()
    {
        if (_moveDirX == 0) return;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        GUILayout.Label($"Dash Orient = {_dashOrient}");

        if (IsTouchingGround)
        {
            GUILayout.Label($"On Ground");

        }
        else
        {
            GUILayout.Label($"In Air");

        }
        GUILayout.Label($"JumpState = {_jumpState}");
        GUILayout.Label($"DashState = {_dashState}");
        GUILayout.Label($"canDash = {canDash}");
        GUILayout.Label($"TimeSinceDash = {_timeSinceDash}");


        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");

        GUILayout.EndVertical();
    }
}