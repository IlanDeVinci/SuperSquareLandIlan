using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class HeroDashSettings
{
    public float cooldown = 1f;
    [Header("Ground Dash")]
    public float groundSpeed = 20f;
    public float groundDuration = 0.3f;
    [Header("Air Dash")]
    public float airSpeed = 20f;
    public float airDuration = 0.3f;
}
