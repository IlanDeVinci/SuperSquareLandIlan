﻿using UnityEngine;
using CameraProfileType = CameraProfile.CameraProfileType;
public class CameraManager : MonoBehaviour
{


    public static CameraManager Instance { get; private set; }
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Profile System")]
    [SerializeField] private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;
    private float _profileTransitionTimer = 0f;
    private float _profileTransitionDuration = 0f;
    private Vector3 _profileTransitionStartPosition;
    private float _profileTransitionStartSize = 0f;

    private Vector3 _FindNextCameraPosition()
    {
        if(_currentCameraProfile.ProfileType == CameraProfileType.FollowTarget)
        {
            if(_currentCameraProfile.TargetToFollow != null)
            {
                Vector3 destination = _currentCameraProfile.TargetToFollow.position;
                return destination;
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
        if(_IsPlayingProfileTransition())
        {
            _profileTransitionTimer += Time.deltaTime;
            Vector3 transitionPosition = _CalculateProfileTransitionPosition(nextPosition);
            SetCameraPosition(transitionPosition);
            float transitionSize = _CalculateProfileTransitionCameraSize(_currentCameraProfile.CameraSize);
            SetCameraSize(transitionSize);
        }
        else
        {
            SetCameraPosition(nextPosition);
            SetCameraSize(_currentCameraProfile.CameraSize);
        }
    }

    public void EnterProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        _currentCameraProfile = cameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
    }

    public void ExitProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        if (_currentCameraProfile != cameraProfile) return;
        _currentCameraProfile = _defaultCameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
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