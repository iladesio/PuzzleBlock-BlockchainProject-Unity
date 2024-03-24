using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class UserService : MonoBehaviour, IUserService
{
    public IRepository repository = new Repository();

    /// <summary>
    /// Given an address, retrieve user profile info from chain and ipfs
    /// </summary>
    /// <param name="userAddress"></param>
    /// <returns></returns>
    public async Task<UserProfile> GetUserProfile(string userAddress)
    {
        var payload = new
        {
            address = userAddress
        };

        UserProfile userProfile = await repository.RetrieveData<UserProfile>(payload, "api/user/userInfo");

        return userProfile;
    }

    /// <summary>
    /// Register a user with a username on chain and ipfs
    /// </summary>
    /// <param name="userAddress"></param>
    /// <param name="nickname"></param>
    public async void RegisterUserProfile(string userAddress, string nickname)
    {

        var payload = new
        {
            address = userAddress,
            username = nickname,
        };

        var ipfsCid = await repository.RetrieveData<string>(payload, "api/user/register");

        if (ipfsCid != null)
        {

            var transactionPayload = new
            {
                contractName = "PuzzleContract",
                contractMethod = "registerUser",
                inputParameters = new[] { userAddress, ipfsCid }
            };


            repository.DoTransaction(transactionPayload, callbackData: ipfsCid, typeOfTransaction: "REGISTRATION");
        }

    }

    /// <summary>
    /// Retrieve current dinamic difficulty
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetDifficulty()
    {
        return await repository.RetrieveData<int>(action: "api/user/getDifficulty");
    }

    /// <summary>
    /// Retrieve all registered user profiles from ipfs
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserProfile>> GetUserProfiles()
    {
        return await repository.RetrieveData<List<UserProfile>>(action: "api/user/getUserProfiles");
    }

    /// <summary>
    /// Update a user profile
    /// </summary>
    /// <param name="userProfile"></param>
    /// <param name="typeOfTransaction"></param>
    /// <returns></returns>
    public async Task UpdateUserProfile(UserProfile userProfile, string typeOfTransaction = "")
    {
        var payload = new
        {
            userJson = userProfile
        };

        var ret = await repository.RetrieveData<Tuple<string, string, string>>(payload: payload, action: "api/user/updateUser");
        var newIpfsCid = ret.Item1;

        var transactionPayload = new
        {
            contractName = "PuzzleContract",
            contractMethod = "editUser",
            inputParameters = new[] { userProfile.UserAddress, newIpfsCid}
        };

        repository.DoTransaction(transactionPayload, callbackData: JsonConvert.SerializeObject(ret), typeOfTransaction: typeOfTransaction);

    }

    /// <summary>
    /// Perform logout from metamask and application
    /// </summary>
    public void DoLogout()
    {
        JavascriptRepository.Web3Disconnect(PlayerPrefs.GetString("UserAddress"));
        PlayerPrefs.DeleteAll();

        JavascriptRepository.Web3Alert("Logged out with success!", "success");
        SceneManager.LoadScene("Login");
    }

    /// <summary>
    /// Quit game application
    /// </summary>
    public void QuitGame()
    {
        if (!PlayerPrefs.HasKey("RememberMe"))
            DoLogout();

        Application.Quit();

    }
}
