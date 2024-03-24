using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class SetupHome : MonoBehaviour
{
    public Image arrow;
    private int currentLevel = Utilities.LEVEL1;
    public Button GoToShop;
    private UserProfile userProfile = null;
    public TextMeshProUGUI usernameText, primaryBalance, secondaryBalance, runCompleted;
    public GameObject Arrow;
    public GameObject RankingEntry, PropicObject;
    public Button CreditToBalance;

    public IUserService userService = new UserService();

    /// <summary>
    /// Start is called before the first frame update. Setup scene elements and content retrieving from blockchain.
    /// </summary>
    private async void Start()
    {
        // Check if login status is correct
        if (!PlayerPrefs.HasKey("UserProfile") || !PlayerPrefs.HasKey("UserAddress"))
        {
            JavascriptRepository.Web3Alert("User not found.", "danger");
            userService.DoLogout();
        }

        GoToShop.onClick.AddListener(() => SceneManager.LoadScene("Marketplace"));

        // Retrieve user profile info from blockchain
        userProfile = await userService.GetUserProfile(PlayerPrefs.GetString("UserAddress"));

        // Fill user info on screen
        usernameText.text = userProfile.Username;
        primaryBalance.text = $"{userProfile.PrimaryBalance} PB";
        secondaryBalance.text = $"Temporary Balance {userProfile.SecondaryBalance} PB";
        runCompleted.text = userProfile.RunCompleted.ToString();
        currentLevel = userProfile.CurrentLevel;

        if (string.IsNullOrWhiteSpace(userProfile.PropicUrl))
        {
            PropicObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Home/default-user");
        }
        else
        {
            PropicObject.GetComponent<Image>().sprite = Utilities.Base64ToSprite(userProfile.PropicUrl);
        }



        switch (currentLevel)
        {
            case Utilities.LEVEL1:
                arrow.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(115f, 170f);

                break;

            case Utilities.LEVEL2:
                arrow.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-105.5f, 185f);

                break;

            case Utilities.LEVEL3:
                arrow.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 270f);

                break;

            default:

                break;
        }
        Arrow.GetComponent<CurrentLevelAnimation>().enabled = true;

        SetupRanking();

    }

    /// <summary>
    /// Setup the ranking table retrieving data from ipfs
    /// </summary>
    private async void SetupRanking()
    {
        var users = (await userService.GetUserProfiles()).OrderByDescending(x => x.Points).ToList();
        var players = users.Select(x => (x, users.IndexOf(x))).ToList();
        var userIndex = players.FindIndex(x => x.Item1.Username == userProfile.Username);
        var nLast = players.Skip(userIndex + 1).Count();
        players = players.SkipLast(players.Count - 1 - (userIndex + nLast)).ToList();
        players = players.TakeLast(5).ToList();


        EditRankingEntry(RankingEntry, players[0].Item2, players[0].Item1.Username, players[0].Item1.Points, players[0].Item1.PropicUrl, players[0].Item1.Username == userProfile.Username);
        for (int i = 1; i < players.Count; i++)
        {
            var entry = Instantiate(RankingEntry, RankingEntry.transform.parent);
            entry.transform.localPosition -= new Vector3(0f, i * 85f, 0f);
            EditRankingEntry(entry, players[i].Item2, players[i].Item1.Username, players[i].Item1.Points, players[i].Item1.PropicUrl, players[i].Item1.Username == userProfile.Username);
            AddMedal(entry, players[i].Item2);
        }
        AddMedal(RankingEntry, players[0].Item2);
    }

    /// <summary>
    /// Edit ranking entry with current user info
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="rank"></param>
    /// <param name="username"></param>
    /// <param name="points"></param>
    /// <param name="propicUrl"></param>
    /// <param name="isUser"></param>
    private void EditRankingEntry(GameObject entry, int rank, string username, int points, string propicUrl, bool isUser = false)
    {
        var propicObj = entry.transform.GetChild(0).GetComponent<Image>();
        var rankObj = entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        var usernameObj = entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        var pointsObj = entry.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        if (string.IsNullOrWhiteSpace(propicUrl))
        {
            propicObj.sprite = Resources.Load<Sprite>("Images/Home/default-user");
        }
        else
        {
            propicObj.sprite = Utilities.Base64ToSprite(propicUrl);
        }

        rankObj.text = $"# {rank + 1}";
        usernameObj.text = username;
        pointsObj.text = $"{points} pt";

        if (isUser)
        {
            rankObj.color = Color.white;
            usernameObj.color = Color.white;
            pointsObj.color = Color.white;
            entry.GetComponent<Image>().color = new Color32(145, 115, 209, 255);
        }
        else
        {
            rankObj.color = new Color32(168, 85, 36, 255);
            usernameObj.color = new Color32(168, 85, 36, 255);
            pointsObj.color = new Color32(168, 85, 36, 255);
            entry.GetComponent<Image>().color = Color.white;
        }

    }

    /// <summary>
    /// Show a medal in correspondence of first three in ranking.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="rank"></param>
    private void AddMedal(GameObject entry, int rank)
    {
        // Instantiate a new GameObject
        GameObject medal = new GameObject("Medal" + rank);

        // Optionally, you can add components to the instantiated game object
        medal.AddComponent<Image>(); // Example of adding a Rigidbody component
        GameObject newMedal = null;

        if (rank == 0)
        {
            medal.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Home/Gold");
            newMedal = Instantiate(medal, entry.transform);
        }
        if (rank == 1)
        {
            medal.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Home/Silver");
            newMedal = Instantiate(medal, entry.transform);
        }
        if (rank == 2)
        {
            medal.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Home/Bronze");
            newMedal = Instantiate(medal, entry.transform);
        }

        if (newMedal != null)
        {
            newMedal.transform.localPosition += new Vector3(230, -5, 0);
            newMedal.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            newMedal.GetComponent<RectTransform>().sizeDelta += new Vector2(30, 0);
        }

        DestroyImmediate(medal);
    }

    /// <summary>
    /// Set logout button behaviour. Do logout from game and metamask.
    /// </summary>
    public void OnLogout()
    {
        userService.DoLogout();
    }

    /// <summary>
    /// Change scene to level if Play button is pressed.
    /// </summary>
    public void OnPlay()
    {
        SceneManager.LoadScene("Level");
    }

    /// <summary>
    /// Set behaviour on CreditToBalance button. Sum Primary and Secondary balances, set Secondary balance to zero and update user profile. 
    /// </summary>
    public async void OnCreditToBalance()
    {
        var secondaryBalance = userProfile.SecondaryBalance;
        if (secondaryBalance > 0)
        {
            userProfile.PrimaryBalance += secondaryBalance;
            userProfile.SecondaryBalance = 0;
            userProfile.CurrentLevel = Utilities.LEVEL1;
            await userService.UpdateUserProfile(userProfile, "GAME_TO_HOME");
        }
        else
        {
            JavascriptRepository.Web3Alert("Temporary Balance is empty!", "warning");
        }
    }

}
