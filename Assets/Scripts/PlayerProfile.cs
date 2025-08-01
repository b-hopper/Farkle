﻿using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public string playerName = "Player";
    [ReadOnly] public int highScore = 0;
    [ReadOnly] public int gamesPlayed = 0;
    [ReadOnly] public int gamesWon = 0;
    [ReadOnly] public int totalScore = 0; // for average tracking
}
