using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void Update()
    {
        _entity.SetMoveDirX(GetInputMoveX());
        if (_GetInputDownJump())
        {
            if(_entity.IsTouchingGround && !_entity.isJumping)
            {
                _entity.JumpStart();
            }
        }
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
        GUILayout.EndVertical();
    }
}