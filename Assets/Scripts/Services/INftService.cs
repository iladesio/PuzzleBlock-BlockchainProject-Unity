using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface INftService
{
    public Task<List<Nft>> GetAllNft(string filterId = null);

    public Task<Nft> GetNftById(int id);
    public Task NftTransfer(int tokenId, int amount, UserProfile userProfile, string typeOfTransaction = "");
    public Task<int> GetOwnedNftAmount(string address, int tokenId);


}
