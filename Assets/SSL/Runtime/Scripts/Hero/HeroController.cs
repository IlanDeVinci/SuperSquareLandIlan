using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    [Header("Jump Buffer")]
    [SerializeField] private float _jumpBufferDuration = 0.2f;
    private float _jumpBufferTimer = 0f;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void Start()
    {
        _CancelJumpBuffer();
    }
    private void Update()
    {
        _UpdateJumpBuffer();
        _entity.SetMoveDirX(GetInputMoveX());
        if (_GetInputDownJump())
        {
            if(_entity.IsTouchingGround && !_entity.isJumping)
            {
                _entity.JumpStart();
            }
            else
            {
                _ResetJumpBuffer();
            }
        }

        if(IsJumpBufferActive())
        {
            if(_entity.IsTouchingGround && !_entity.isJumping)
            {
                _entity.JumpStart();
            }
        }

        if (_entity.isJumpImpulsing)
        {
            if (!_GetInputJump() && _entity.isJumpMinDurationReached)
            {
                _entity.StopJumpImpulsion();
            }
        }

        if (_GetInputDownShift())
        {
            if (!_entity.isDashing)
            {
                _entity.DashStart();
            }
        }


    }

    private void _CancelJumpBuffer()
    {
        _jumpBufferTimer = _jumpBufferDuration;
    }

    private void _UpdateJumpBuffer()
    {
        if (!IsJumpBufferActive()) return;
        _jumpBufferTimer += Time.deltaTime;
    }
    private bool IsJumpBufferActive()
    {
        return _jumpBufferTimer < _jumpBufferDuration;
    }
    private void _ResetJumpBuffer()
    {
        _jumpBufferTimer = 0f;
    }
    private bool _GetInputDownShift()
    {
        return Input.GetKeyDown(KeyCode.LeftShift);
    }
    private bool _GetInputJump()
    {
        return Input.GetKey(KeyCode.Space);
    }
    private bool _GetInputDownJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
    private bool IsInputDash()
    {
        bool inputDash = false;
        if(Input.GetKeyDown(KeyCode.E)) {
            inputDash = true;
        }
        return inputDash;
    }


    private float GetInputMoveX()
    {
        float inputMoveX = 0f;
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q)){
            inputMoveX = -1f;
        }

        if(Input.GetKey(KeyCode.D))
        {
            inputMoveX = 1f;
        }

        return inputMoveX;
    }
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"Jump Buffer Timer = {_jumpBufferTimer}");
        GUILayout.EndVertical();
    }
}