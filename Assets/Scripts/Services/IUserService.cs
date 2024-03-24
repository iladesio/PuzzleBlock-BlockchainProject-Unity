using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IUserService 
{
    public Task<UserProfile> GetUserProfile(string address);
    public void RegisterUserProfile(string address, string username);
    public Task UpdateUserProfile(UserProfile userProfile, string typeOfTransaction = "");
    public Task<int> GetDifficulty();
    public Task<List<UserProfile>> GetUserProfiles();
    public void DoLogout();
    public void QuitGame();


}
