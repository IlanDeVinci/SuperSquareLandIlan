using UnityEngine;

public class HeroEffectRotate : MonoBehaviour
{
    [Header("Entity")][SerializeField] private HeroEntity _entity;
    [Header("Object To Rotate")][SerializeField] private Transform _objectToRotate;

    [Header("Rotate Loop")]
    [SerializeField] private float rotationPeriod;
    [SerializeField] private float rotationFactor = 10f;
    private float _rotateTimer = 0f;
    private float _currentRotation = 0f;
    [SerializeField] private bool _isRotationEnabled = false;

    private void _SetObjectToRotateDelta(float rotation)
    {
        Vector3 newEulerAngles = _objectToRotate.localEulerAngles;
        newEulerAngles.z -= _currentRotation;
        _currentRotation = rotation;
        newEulerAngles.z += _currentRotation;
        _objectToRotate.localEulerAngles = newEulerAngles;

    }

    private void Update()
    {
        if (_isRotationEnabled)
        {
            if (_entity.IsTouchingGround && !_entity.isDashing)
            {
                if (_entity.isHorizontalMoving)
                {
                    _rotateTimer += Time.deltaTime;
                    float percent = Mathf.PingPong(_rotateTimer, rotationPeriod) / rotationPeriod;
                    float newRotation = Mathf.Lerp(-rotationFactor, rotationFactor, percent);
                    _SetObjectToRotateDelta(newRotation);
                }
                else
                {
                    _SetObjectToRotateDelta(0f);
                }
            }
            else
            {
                _SetObjectToRotateDelta(0f);
            }
        }


    }

}
