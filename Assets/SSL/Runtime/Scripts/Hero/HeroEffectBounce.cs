using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class HeroEffectBounce : MonoBehaviour
{
    [Header("Entity")][SerializeField] private HeroEntity _entity;
    [Header("Object to Bounce")][SerializeField] private Transform _objectToBounce;
    [Header("Bounce Loop")][SerializeField] private float bouncePeriod;
    [Header("Bounce Loop")][SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private bool _isBounceEnabled = false;

    private float _bounceTimer = 0f;
    private float _currentHeight = 0f;

    private void _SetObjectToBounceHeight(float height)
    {
        Vector3 newPosition = _objectToBounce.localPosition;
        newPosition.y -= _currentHeight;
        _currentHeight = height;
        newPosition.y += _currentHeight;
        _objectToBounce.localPosition = newPosition;
    }

    private void Update()
    {
        if (_isBounceEnabled)
        {
            if (_entity.IsTouchingGround && !_entity.isDashing)
            {
                if (_entity.isHorizontalMoving)
                {
                    _bounceTimer += Time.deltaTime;
                    float percent = Mathf.PingPong(_bounceTimer, bouncePeriod) / bouncePeriod;
                    float newHeight = percent * bounceHeight;
                    _SetObjectToBounceHeight(newHeight);
                }
                else
                {
                    _SetObjectToBounceHeight(0f);
                }
            }
            else
            {
                _SetObjectToBounceHeight(0f);
            }
        }

    }

}
