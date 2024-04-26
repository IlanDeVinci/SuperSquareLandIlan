using UnityEngine;

public class CameraProfile : MonoBehaviour
{

    [Header("Type")]
    [SerializeField] private CameraProfileType _profileType = CameraProfileType.Static;

    [Header("Follow")]
    [SerializeField] private CameraFollowable _targetToFollow = null;
    public CameraFollowable TargetToFollow => _targetToFollow;
    public CameraProfileType ProfileType => _profileType;

    [Header("Damping")]
    [SerializeField] private bool _useDampingHorizontally = false;
    [SerializeField] private bool _useDampingVertically = false;
    [SerializeField] private float _horizontalDampingFactor = 5f;
    [SerializeField] private float _verticalDampingFactor = 5f;

    public bool UseDampingHorizontally => _useDampingHorizontally;
    public bool UseDampingVertically => _useDampingVertically;

    public float HorizontalDampingFactor => _horizontalDampingFactor;

    public float VerticalDampingFactor => _verticalDampingFactor;

    private Camera _camera;
    public float CameraSize => _camera.orthographicSize;
    public Vector3 Position => _camera.transform.position;
    public enum CameraProfileType
    {
        Static = 0,
        FollowTarget
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if ( _camera != null)
        {
            _camera.enabled = false;
        }
    }
}

