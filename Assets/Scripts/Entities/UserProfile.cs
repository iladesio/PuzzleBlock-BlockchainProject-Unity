using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Class containing all information of a user profile
/// </summary>
[Serializable]
public class UserProfile
{
    public UserProfile()
    {
    }

    public UserProfile(string userAddress, string username, int primaryBalance, int secondaryBalance, int points, int currentLevel, int runCompleted, int amethystNumber, int grimoireNumber, int potionNumber, string propicUrl)
    {
        UserAddress = userAddress;
        Username = username;
        PrimaryBalance = primaryBalance;
        SecondaryBalance = secondaryBalance;
        Points = points;
        CurrentLevel = currentLevel;
        RunCompleted = runCompleted;
        AmethystNumber = amethystNumber;
        GrimoireNumber = grimoireNumber;
        PotionNumber = potionNumber;
        PropicUrl = propicUrl;
    }

    public string UserAddress { get; set; }
    public string Username { get; set; }
    public int PrimaryBalance { get; set; }
    public int SecondaryBalance { get; set; }
    public int Points { get; set; }
    public int CurrentLevel { get; set; }
    public int RunCompleted { get; set; }
    public int AmethystNumber { get; set; }
    public int GrimoireNumber { get; set; }
    public int PotionNumber { get; set; }
    public string PropicUrl { get; set; }


}
