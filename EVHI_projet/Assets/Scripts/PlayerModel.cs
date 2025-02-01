using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public float rightThreshold = 510f;
    public float leftThreshold = 510f;

    public bool isRight = true;
    public bool isLeft = false;

    public float coordinationLevel = 0.0f; //0~1
    public float dodgeLevel = 0.0f; //0~

    public bool fatigue = false;

    public int score;


}
