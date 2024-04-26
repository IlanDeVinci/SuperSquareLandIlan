using Unity.Mathematics;
using UnityEngine;
using CameraProfileType = CameraProfile.CameraProfileType;

public class CameraManager : MonoBehaviour
{


    public static CameraManager Instance { get; private set; }
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Profile System")]
    [SerializeField] private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;
    //Transition
    private float _profileTransitionTimer = 0f;
    private float _profileTransitionDuration = 0f;
    private Vector3 _profileTransitionStartPosition;
    private float _profileTransitionStartSize = 0f;
    //Follow
    private Vector3 _profileLastFollowDestination;
    //Damping
    private Vector3 _dampedPosition;
    //Offset
    private float previousOrient = 1f;
    private float changeOrientTimer = 0f;
    private float currentOffset = 0f;
    private float startingOffset = 0f;
    //Autoscroll
    private bool autoScrollStart = false;
    private Vector3 autoScrollStartPosition = Vector3.zero;
    private float timeSinceAutoScrollStart = 0f;

    private Vector3 _AutoScrollCamera(Vector3 position)
    {
        if (_currentCameraProfile.ProfileType != CameraProfileType.AutoScroll)
        {
            autoScrollStart = false;
        }
        else
        {
            if (!autoScrollStart)
            {
                autoScrollStart = true;
                timeSinceAutoScrollStart = 0f;
                autoScrollStartPosition = position;
            }
            timeSinceAutoScrollStart += Time.deltaTime;
            position.x = autoScrollStartPosition.x + _currentCameraProfile.CameraAutoScroll.horizontalSpeed * timeSinceAutoScrollStart;
            position.y = autoScrollStartPosition.y + _currentCameraProfile.CameraAutoScroll.verticalSpeed * timeSinceAutoScrollStart;
        }
        return position;
    }
    private Vector3 _OffsetCameraPosition(Vector3 position)
    {
        if (!_currentCameraProfile.UseFollowOffset && !(_currentCameraProfile.ProfileType == CameraProfileType.FollowTarget)) return position;

        if (previousOrient != _currentCameraProfile.TargetToFollow.FollowDirection)
        {
            Debug.Log(_currentCameraProfile.BoundsRect.xMin + 2*_currentCameraProfile.CameraSize);
            Debug.Log(_currentCameraProfile.BoundsRect.xMax - 2* _currentCameraProfile.CameraSize);

            changeOrientTimer = 0f;
            startingOffset = -currentOffset;
            previousOrient = _currentCameraProfile.TargetToFollow.FollowDirection;
            if (position.x + startingOffset < _currentCameraProfile.BoundsRect.xMin + 2 * _currentCameraProfile.CameraSize)
            {
                if (previousOrient == 1)
                {
                    startingOffset = (_currentCameraProfile.BoundsRect.xMin + 2 * _currentCameraProfile.CameraSize) - position.x;
                }
            }

            if (position.x - startingOffset > _currentCameraProfile.BoundsRect.xMax - 2 * _currentCameraProfile.CameraSize)
            {
                if (previousOrient == -1)
                {
                    startingOffset = position.x - (_currentCameraProfile.BoundsRect.xMax - 2 * _currentCameraProfile.CameraSize);
                }

            }

            
            /*

            Vector3 testPos1, testPos2;
            testPos1 = new Vector3(position.x - startingOffset, position.y, position.z);
            testPos2 = new Vector3(position.x + startingOffset, position.y, position.z);
            testPos1 = _ClampPositionIntoBounds(testPos1);
            testPos2 = _ClampPositionIntoBounds(testPos2);
            if (previousOrient == 1)
            {
                startingOffset = position.x - Mathf.Max(position.x - startingOffset, testPos1.x);
            }
            else
            {
                startingOffset = Mathf.Max(position.x + startingOffset, testPos2.x) - position.x;

            }
            
            //startingOffset = position.x - Mathf.Clamp(position.x + startingOffset, testPos1.x, testPos2.x);
            Debug.Log((position.x));
            
            Debug.Log(testPos1);
            Debug.Log(testPos2);
            */
            /*
            if (previousOrient == 1)
            {
                startingOffset += position.x - testPos.x;
            }
            else
            {
                startingOffset += position.x - testPos.x;
                //startingOffset = -startingOffset;
            }
            */

            Debug.Log($"offset = {currentOffset}");
            Debug.Log($"start = {startingOffset}");
        }

        if (changeOrientTimer <= _currentCameraProfile.FollowOffset.followOffsetDamping)
        {
            float percent = changeOrientTimer / _currentCameraProfile.FollowOffset.followOffsetDamping;
            changeOrientTimer += Time.deltaTime;
            currentOffset = Mathf.Lerp(startingOffset, _currentCameraProfile.FollowOffset.followOffsetX, percent);
            currentOffset = Mathf.Clamp(currentOffset, -_currentCameraProfile.FollowOffset.followOffsetX,
                _currentCameraProfile.FollowOffset.followOffsetX);
            if (_currentCameraProfile.TargetToFollow.FollowDirection == 1)
            {
                position.x += currentOffset;
            }
            else
            {
                position.x -= currentOffset;

            }

        }
        else
        {
            if (_currentCameraProfile.TargetToFollow.FollowDirection == 1)
            {
                position.x += _currentCameraProfile.FollowOffset.followOffsetX;
            }
            else
            {
                position.x -= _currentCameraProfile.FollowOffset.followOffsetX;

            }
        }

        return position;
    }
    private Vector3 _ClampPositionIntoBounds(Vector3 position)
    {
        if (!_currentCameraProfile.HasBounds) return position;

        Rect boundsRect = _currentCameraProfile.BoundsRect;
        Vector3 worldBottomLeft = _camera.ScreenToWorldPoint(new Vector3(0f, 0f));
        Vector3 worldTopRight = _camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, _camera.pixelHeight));
        Vector3 worldScreenSize = new Vector2(worldTopRight.x - worldBottomLeft.x, worldTopRight.y - worldBottomLeft.y);
        Vector3 worldHalfScreenSize = worldScreenSize / 2;

        if (position.x > boundsRect.xMax - worldHalfScreenSize.x)
        {
            position.x = boundsRect.xMax - worldHalfScreenSize.x;
        }
        if (position.x < boundsRect.xMin + worldHalfScreenSize.x)
        {
            position.x = boundsRect.xMin + worldHalfScreenSize.x;
        }
        if (position.y > boundsRect.yMax - worldHalfScreenSize.y)
        {
            position.y = boundsRect.yMax - worldHalfScreenSize.y;
        }
        if (position.y < boundsRect.yMin + worldHalfScreenSize.y)
        {
            position.y = boundsRect.yMin + worldHalfScreenSize.y;
        }
        return position;
    }
    private void _SetCameraDampedPosition(Vector3 position)
    {
        _dampedPosition = position;
    }
    private Vector3 _ApplyDamping(Vector3 position)
    {
        if (_currentCameraProfile.UseDampingHorizontally)
        {
            _dampedPosition.x = Mathf.Lerp(_dampedPosition.x, position.x, _currentCameraProfile.HorizontalDampingFactor * Time.deltaTime);
        }
        else
        {
            _dampedPosition.x = position.x;
        }

        if (_currentCameraProfile.UseDampingVertically)
        {
            _dampedPosition.y = Mathf.Lerp(_dampedPosition.y, position.y, _currentCameraProfile.VerticalDampingFactor * Time.deltaTime);
        }
        else
        {
            _dampedPosition.y = position.y;
        }

        return _dampedPosition;
    }
    private Vector3 _FindNextCameraPosition()
    {
        if (_currentCameraProfile.ProfileType == CameraProfileType.FollowTarget)
        {
            if (_currentCameraProfile.TargetToFollow != null)
            {
                CameraFollowable targetToFollow = _currentCameraProfile.TargetToFollow;
                _profileLastFollowDestination.x = targetToFollow.FollowPositionX;
                _profileLastFollowDestination.y = targetToFollow.FollowPositionY;
                return _profileLastFollowDestination;
            }
        }
        return _currentCameraProfile.Position;
    }
    private float _CalculateProfileTransitionCameraSize(float endSize)
    {
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        percent = Mathf.Clamp01(percent);
        float startSize = _profileTransitionStartSize;
        return Mathf.Lerp(startSize, endSize, percent);
    }
    private Vector3 _CalculateProfileTransitionPosition(Vector3 destination)
    {
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        percent = Mathf.Clamp01(percent);
        Vector3 origine = _profileTransitionStartPosition;
        return Vector3.Lerp(origine, destination, percent);
    }


    private void _PlayProfileTransition(CameraProfileTransition transition)
    {
        _profileTransitionStartPosition = _camera.transform.position;
        _profileTransitionStartSize = _camera.orthographicSize;
        _profileTransitionTimer = 0f;
        _profileTransitionDuration = transition.duration;
    }

    private bool _IsPlayingProfileTransition()
    {
        return _profileTransitionTimer < _profileTransitionDuration;
    }
    private void Update()
    {
        Vector3 nextPosition = _FindNextCameraPosition();
        nextPosition = _AutoScrollCamera(nextPosition);
        nextPosition = _OffsetCameraPosition(nextPosition);
        nextPosition = _ClampPositionIntoBounds(nextPosition);
        nextPosition = _ApplyDamping(nextPosition);
        if (_IsPlayingProfileTransition())
        {
            _profileTransitionTimer += Time.deltaTime;
            Vector3 transitionPosition = _CalculateProfileTransitionPosition(nextPosition);
            _SetCameraPosition(transitionPosition);
            float transitionSize = _CalculateProfileTransitionCameraSize(_currentCameraProfile.CameraSize);
            _SetCameraSize(transitionSize);
        }
        else
        {
            _SetCameraPosition(nextPosition);
            _SetCameraSize(_currentCameraProfile.CameraSize);
        }
    }

    public void EnterProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        _currentCameraProfile = cameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
        _SetCameraDampedPosition(_FindNextCameraPosition());

    }

    public void ExitProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        if (_currentCameraProfile != cameraProfile) return;
        _currentCameraProfile = _defaultCameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
        _SetCameraDampedPosition(_FindNextCameraPosition());

    }

    private void Awake()
    {
        Instance = this;
    }
    private void _SetCameraPosition(Vector3 position)
    {
        Vector3 newCameraPosition = _camera.transform.position;
        newCameraPosition.x = position.x;
        newCameraPosition.y = position.y;
        _camera.transform.position = newCameraPosition;
    }

    private void _SetCameraSize(float size)
    {
        _camera.orthographicSize = size;
    }

    private void _InitToDefaultProfile()
    {
        _currentCameraProfile = _defaultCameraProfile;
        _SetCameraPosition(_currentCameraProfile.Position);
        _SetCameraSize(_currentCameraProfile.CameraSize);
        _SetCameraDampedPosition(_ClampPositionIntoBounds(_FindNextCameraPosition()));

    }
    private void Start()
    {
        _InitToDefaultProfile();
    }
}