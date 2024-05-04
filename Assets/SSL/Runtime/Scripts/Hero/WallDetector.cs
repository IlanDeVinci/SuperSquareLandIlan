using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [Header("Detection")] [SerializeField] private Transform[] _detectionPointsLeft;
    [SerializeField] private Transform[] _detectionPointsRight;
    [SerializeField] private float _detectionLength = 0.1f;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private LayerMask _wallLayerMask;

    private LayerMask combinedLayerMask;

    // Start is called before the first frame update
    void Awake()
    {
        combinedLayerMask = _groundLayerMask | _wallLayerMask;
    }

    public bool DetectWallCenterLeft()
    {
        RaycastHit2D hitResult = Physics2D.Raycast(
            _detectionPointsLeft[2].position,
            Vector2.left,
            _detectionLength,
            combinedLayerMask
        );
        if (hitResult.collider != null)
        {
            return true;
        }

        return false;
    }

    public bool DetectWallCenterRight()
    {
        RaycastHit2D hitResult = Physics2D.Raycast(
            _detectionPointsRight[2].position,
            Vector2.right,
            _detectionLength,
            combinedLayerMask
        );
        if (hitResult.collider != null)
        {
            return true;
        }

        return false;
    }

    public bool DetectWallNearByLeft()
    {
        foreach (Transform detectionPoint in _detectionPointsLeft)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.left,
                _detectionLength,
                combinedLayerMask
            );
            if (hitResult.collider != null)
            {
                return true;
            }
        }

        return false;
    }

    public bool DetectWallNearByRight()
    {
        foreach (Transform detectionPoint in _detectionPointsRight)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.right,
                _detectionLength,
                combinedLayerMask
            );
            if (hitResult.collider != null)
            {
                return true;
            }
        }

        return false;
    }
}