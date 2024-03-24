using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.XR;
using static UnityEngine.Rendering.DebugUI;

public class Login : MonoBehaviour
{
    public IUserService userService = new UserService();

    public GameObject NicknamePanel;
    public Toggle RememberMe;
    public TMP_InputField InputUsername;
    public GameObject ValidationMessage;

    /// <summary>
    /// Setup scene panels. Check if user is already logged in metamask and game, if so, user is redirected to home scene.
    /// </summary>
    public void Start()
    {
        ValidationMessage.SetActive(false);
        NicknamePanel.SetActive(false);
        if (JavascriptRepository.Web3VerifyMetamask())
            JavascriptRepository.Web3VerifyConnection();

    }

    /// <summary>
    /// Checks if user is logged in game and had previously selected remember me. If true, redirect to home scene.
    /// </summary>
    /// <param name="id"></param>
    public void CheckRememberMe(string id)
    {
        if (PlayerPrefs.HasKey("RememberMe") && !string.IsNullOrWhiteSpace(id) && PlayerPrefs.HasKey("UserProfile"))
        {
            SceneManager.LoadScene("Home");
        }

    }

    /// <summary>
    /// Checks if user is logged in, retrieve profile from blockchain. Login if user has already set a username, input a username otherwise.
    /// </summary>
    /// <param name="userAddress"></param>
    public async void LoginPlayer(string userAddress)
    {
        if (string.IsNullOrWhiteSpace(userAddress))
        {
            userAddress = PlayerPrefs.GetString("UserAddress");

        }
        else
        {
            PlayerPrefs.SetString("UserAddress", userAddress);
        }

        if (string.IsNullOrWhiteSpace(userAddress))
        {
            JavascriptRepository.Web3Alert("User address missing!", "danger");
            userService.DoLogout();
        }


        UserProfile userProfile = await userService.GetUserProfile(userAddress);


        if (userProfile.Username == null)
        {
            NicknamePanel.SetActive(true);
        }
        else
        {
            Debug.Log("User retrieved, redirect to home");
            PlayerPrefs.SetString("UserProfile", JsonConvert.SerializeObject(userProfile));

            if (RememberMe.isOn)
                PlayerPrefs.SetString("RememberMe", "true");

            JavascriptRepository.Web3Alert("Logged in with success!", "success");
            SceneManager.LoadScene("Home");
        }

    }

    /// <summary>
    /// Validate and save user and username on chain
    /// </summary>
    public void OnSave()
    {
        ValidationMessage.SetActive(false);
        var username = InputUsername.text;
        string pattern = @"^[a-zA-Z0-9!@#$%&_]{1,16}$";

        if (Regex.IsMatch(username, pattern))
        {
            userService.RegisterUserProfile(PlayerPrefs.GetString("UserAddress"), username);
        }
        else
        {
            ValidationMessage.SetActive(true);
        }

    }

    /// <summary>
    /// Cancel button behaviour. Do logout and hide username input panel.
    /// </summary>
    public void OnCancel()
    {
        NicknamePanel.SetActive(false);
        ValidationMessage.SetActive(false);
        JavascriptRepository.Web3Disconnect(PlayerPrefs.GetString("UserAddress"));
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// On pressing login button checks metamask extension intallation and do metamask login.
    /// </summary>
    public void OnLogin()
    {
        if (JavascriptRepository.Web3VerifyMetamask())
            JavascriptRepository.Web3Connect();
    }

}
