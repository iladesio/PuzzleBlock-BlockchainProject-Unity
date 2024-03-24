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
using UnityEngine.SocialPlatforms.Impl;

public class NftService : MonoBehaviour, INftService
{
    public IRepository repository = new Repository();

    /// <summary>
    /// Retrieve all NFT from Ipfs
    /// </summary>
    /// <param name="filterId"></param>
    /// <returns></returns>
    public async Task<List<Nft>> GetAllNft(string filterId = null)
    {
        return await repository.RetrieveData<List<Nft>>(action: "api/nft/GetAllMintedAsset");
    }

    /// <summary>
    /// Retrieve an NFT from Ipfs given its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Nft> GetNftById(int id)
    {
        var payload = new
        {
            tokenId = id
        };
        return await repository.RetrieveData<Nft>(payload, "api/nft/GetMintedAsset");
    }

    /// <summary>
    /// Do a transaction to buy an nft and update user profile
    /// </summary>
    /// <param name="tokenId"></param>
    /// <param name="amount"></param>
    /// <param name="userProfile"></param>
    /// <param name="typeOfTransaction"></param>
    /// <returns></returns>
    public async Task NftTransfer(int tokenId, int amount, UserProfile userProfile, string typeOfTransaction = "")
    {
        var payload = new
        {
            userJson = userProfile
        };

        var ret = await repository.RetrieveData<Tuple<string, string, string>>(payload: payload, action: "api/user/updateUser");
        var newIpfsCid = ret.Item1;

        var transactionPayload = new
        {
            contractName = "GameAsset",
            contractMethod = "safeTransferFromTo",
            inputParameters = new[] { tokenId.ToString(), amount.ToString(), userProfile.UserAddress, newIpfsCid }
        };

        repository.DoTransaction(transactionPayload, callbackData: JsonConvert.SerializeObject(ret), typeOfTransaction: typeOfTransaction);

    }

    /// <summary>
    /// Retrieve amount of owned nft with a certain id belonging to an address
    /// </summary>
    /// <param name="address"></param>
    /// <param name="tokenId"></param>
    /// <returns></returns>
    public async Task<int> GetOwnedNftAmount(string address, int tokenId)
    {
        var payload = new
        {
            address,
            tokenId
        };

        return await repository.RetrieveData<int>(payload: payload, action: "api/nft/getOwnedAssetAmount");
    }
}
