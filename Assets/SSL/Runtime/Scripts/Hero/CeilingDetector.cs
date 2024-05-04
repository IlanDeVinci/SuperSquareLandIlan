using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingDetector : MonoBehaviour
{
    [Header("Detection")] [SerializeField] private Transform[] _detectionPoints;
    [SerializeField] private float _detectionLength = 0.1f;
    [SerializeField] private LayerMask _groundLayerMask;

    public bool DetectCeilingCenter()
    {
        Transform detectionPoint = _detectionPoints[1];
        RaycastHit2D hitResult = Physics2D.Raycast(
            detectionPoint.position,
            Vector2.up,
            _detectionLength,
            _groundLayerMask
        );
        if (hitResult.collider != null)
        {
            return true;
        }

        return false;
    }

    public bool DetectCeilingNearBy()
    {
        foreach (Transform detectionPoint in _detectionPoints)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.up,
                _detectionLength,
                _groundLayerMask
            );
            if (hitResult.collider != null)
            {
                return true;
            }
        }

        return false;
    }
}