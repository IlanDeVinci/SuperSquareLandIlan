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

    [Header("Wall")]
    [SerializeField] private WallDetector _wallDetector;
    public bool IsTouchingWallLeft { get; private set; } = false;

    public bool IsTouchingWallRight { get; private set; } = false;


    [Header("Jump")]

    [SerializeField] private HeroJumpSettings[] _jumpSettings;
    private int _jumpIndex = 0;
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
    public bool isJumpMinDurationReached => _jumpTimer >= _jumpSettings[_jumpIndex-1].jumpMinDuration;

    public bool isJumping => _jumpState != JumpState.NotJumping;
    public bool hasJumpsLeft => _jumpIndex < _jumpSettings.Length;
    public bool canJump => (hasJumpsLeft && ((_jumpState == JumpState.NotJumping) || (_jumpState == JumpState.Falling)));

    [Header("Dash")]
    [SerializeField] private HeroDashSettings _dashSettings;

    private DashState _dashState = DashState.NotDashing;
    private float _dashTimer = 0f;
    private float _timeSinceDash = 0f;
    private float _dashOrient = 1f;
    private bool isAirDash = false;
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
            _dashOrient = _moveDirX;
            _dashState = DashState.Dashing;
            _timeSinceDash = 0f;
            _dashTimer = 0f;
            isAirDash = !IsTouchingGround;
        }
    }

    private void ClampHorizontalSpeed()
    {
        if (isAirDash)
        {
            _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, -_airHorizontalMovementSettings.speedMax, _airHorizontalMovementSettings.speedMax);

        }
        else
        {
            _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, -_groundHorizontalMovementSettings.speedMax, _groundHorizontalMovementSettings.speedMax);
        }
    }
    private void _UpdateDash()
    {
        _dashTimer += Time.fixedDeltaTime;

        if (isAirDash)
        {
            _jumpState = JumpState.NotJumping;

            if (!IsTouchingWallRight || !IsTouchingWallLeft)
            {
                if (_dashTimer < _dashSettings.airDuration)
                {
                    _horizontalSpeed = _dashSettings.airSpeed;
                }
                else
                {
                    ClampHorizontalSpeed();
                    _dashState = DashState.NotDashing;
                }
            }
            else
            {
                _dashTimer = _dashSettings.airDuration;
                _ResetHorizontalSpeed();
                _dashState = DashState.NotDashing;
            }

        }
        else
        {
            if (!IsTouchingWallRight || !IsTouchingWallLeft)
            {
                if (_dashTimer < _dashSettings.groundDuration)
                {
                    _horizontalSpeed = _dashSettings.groundSpeed;
                }
                else
                {
                    ClampHorizontalSpeed();
                    _dashState = DashState.NotDashing;
                }

            }
            else
            {
                _dashTimer = _dashSettings.groundDuration;
                _ResetHorizontalSpeed();
                _dashState = DashState.NotDashing;
            }


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
        if(_jumpIndex < _jumpSettings.Length)
        {
            _jumpIndex += 1;
        }
    }

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    private void _UpdateJumpStateImpulsion(int index)
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings[index-1].jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings[index-1].jumpSpeed;
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
                _UpdateJumpStateImpulsion(_jumpIndex);
                break;
        }
    }
    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
        if(IsTouchingGround && _jumpState != JumpState.JumpImpulsion)
        {
            _jumpIndex = 0;
        }
    }

    private void _ApplyWallDetection()
    {
        IsTouchingWallLeft = _wallDetector.DetectWallNearByLeft();
        IsTouchingWallRight = _wallDetector.DetectWallNearByRight();

    }

    private void _ResetVerticalSpeed()
    {

        _verticalSpeed = 0f;
    }

    private void _ResetHorizontalSpeed()
    {

        _horizontalSpeed = 0f;
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

    private void _ResetSpeedOnWallCollision()
    {
        if(!isJumping)
        {
            if(IsTouchingWallLeft)
            {
                if(_orientX != 1)
                {
                    _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, _horizontalSpeed, 0);
                }
            }
            if(IsTouchingWallRight)
            {
                if (_orientX != -1)
                {
                    _horizontalSpeed = Mathf.Clamp(_horizontalSpeed, _horizontalSpeed, 0);
                }
            }
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
        _ApplyWallDetection();
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
            if (!IsTouchingGround && !isDashing)
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
        
        _ResetSpeedOnWallCollision();
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
        GUILayout.Label($"canJump = {canJump}");
        GUILayout.Label($"jumpIndex = {_jumpIndex}");
        GUILayout.Label($"hasJumpsLeft = {hasJumpsLeft}");

        GUILayout.Label($"JumpState = {_jumpState}");
        GUILayout.Label($"DashState = {_dashState}");
        GUILayout.Label($"TimeSinceDash = {_timeSinceDash}");
        GUILayout.Label($"AirDash = {isAirDash}");

        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");

        GUILayout.EndVertical();
    }
}