using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager Instance { get; private set; }
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Profile System")]
    [SerializeField] private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;

    private void Update()
    {
        SetCameraPosition(_currentCameraProfile.Position);
        SetCameraSize(_currentCameraProfile.CameraSize);

    }

    public void EnterProfile(CameraProfile cameraProfile)
    {
        _currentCameraProfile = cameraProfile;
    }

    public void ExitProfile(CameraProfile cameraProfile)
    {
        if (_currentCameraProfile != cameraProfile) return;
        _currentCameraProfile = _defaultCameraProfile;
    }

    private void Awake()
    {
        Instance = this;
    }
    private void SetCameraPosition(Vector3 position)
    {
        Vector3 newCameraPosition = _camera.transform.position;
        newCameraPosition.x = position.x;
        newCameraPosition.y = position.y;
        _camera.transform.position = newCameraPosition;
    }

    private void SetCameraSize(float size)
    {
        _camera.orthographicSize = size;
    }

    private void _InitToDefaultProfile()
    {
        _currentCameraProfile = _defaultCameraProfile;
        SetCameraPosition(_currentCameraProfile.Position);
        SetCameraSize(_currentCameraProfile.CameraSize);
    }
    private void Start()
    {
        _InitToDefaultProfile();
    }
}