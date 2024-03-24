using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class SetupGame : MonoBehaviour
{
    public IUserService userService = new UserService();

    private UserProfile userProfile = null;
    private bool isGameRunning = false;
    private float timeRemaining = 59f;
    private int currentLevel = Utilities.LEVEL1;
    private int difficulty = Utilities.EASY;
    private double multiplier = 1f;
    private bool isGameOver = false;
    private bool isPotionUsed = false;
    private float amethystCounter = 0f;
    public Image descriptionPanel;
    public TextMeshProUGUI timerText;
    public GameObject powerUpsGroup;
    public GameObject lvlOne;
    public GameObject lvlTwo;
    public GameObject lvlThree;
    public Image badgeCurrentLevel;
    public Image boilingEffectLv1;
    public Image boilingEffectLv2;
    public Image gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Image gameOverImage;
    public TextMeshProUGUI continueText;
    public Image moneyPic;
    public TextMeshProUGUI temporaryBalanceText;
    public Button grimoireButton;
    private bool gameStarted = false;


    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private async void Start()
    {
        // Check if user is correctly logged in
        if (!PlayerPrefs.HasKey("UserProfile") || !PlayerPrefs.HasKey("UserAddress"))
        {
            JavascriptRepository.Web3Alert("User not found.", "danger");
            userService.DoLogout();
        }

        // Retrieve user profile info from blockchain and Ipfs
        userProfile = await userService.GetUserProfile(PlayerPrefs.GetString("UserAddress"));

        // Save grimoire number
        PlayerPrefs.SetInt("GrimoireNumber", userProfile.GrimoireNumber);

        // Compute difficulty for current level from node.js server and blockchain
        difficulty = await userService.GetDifficulty();
        PlayerPrefs.SetInt("Difficulty", difficulty);

        // Save current level 
        currentLevel = userProfile.CurrentLevel;

        // activate grimoire script according to current level
        // Each level has diffent grimoire effect
        grimoireButton.onClick.SetPersistentListenerState(currentLevel, UnityEngine.Events.UnityEventCallState.RuntimeOnly);

        // Prepare scene frontend filling with user information
        var temporaryBalance = userProfile.SecondaryBalance;
        temporaryBalanceText.text = temporaryBalance + " PB";

        powerUpsGroup.gameObject.SetActive(false);
        lvlOne.gameObject.SetActive(false);
        lvlTwo.gameObject.SetActive(false);
        lvlThree.gameObject.SetActive(false);
        gameOverPanel.gameObject.SetActive(false);

        SetLevelDescription();

        // Set current duration on screen and on countdown logic
        switch (difficulty)
        {
            case Utilities.EASY:
                timerText.text = "01:00";
                timeRemaining = 59f;
                break;
            case Utilities.MEDIUM:
                timerText.text = "00:40";
                timeRemaining = 39f;
                break;
            case Utilities.HARD:
                timerText.text = "00:20";
                timeRemaining = 19f;
                break;
            default:
                break;

        }

        // Edit level appearance
        switch (currentLevel)
        {
            case Utilities.LEVEL1:
                badgeCurrentLevel.sprite = Resources.Load<Sprite>("Images/Common/BadgeLv1");
                boilingEffectLv1.gameObject.SetActive(false);
                boilingEffectLv2.gameObject.SetActive(false);
                lvlOne.transform.GetChild(0).GetComponent<LevelOne>().enabled = true;
                lvlOne.gameObject.SetActive(true);
                multiplier = Utilities.MULTIPLIER_LEVEL1;
                break;
            case Utilities.LEVEL2:
                badgeCurrentLevel.sprite = Resources.Load<Sprite>("Images/Common/BadgeLv2");
                boilingEffectLv1.gameObject.SetActive(true);
                boilingEffectLv2.gameObject.SetActive(false);
                lvlTwo.transform.GetChild(0).GetComponent<LevelTwo>().enabled = true;
                lvlTwo.gameObject.SetActive(true);
                multiplier = Utilities.MULTIPLIER_LEVEL2;
                break;
            case Utilities.LEVEL3:
                badgeCurrentLevel.sprite = Resources.Load<Sprite>("Images/Common/BadgeLv3");
                boilingEffectLv1.gameObject.SetActive(true);
                boilingEffectLv2.gameObject.SetActive(true);
                lvlThree.transform.GetChild(0).GetComponent<LevelThree>().enabled = true;
                lvlThree.gameObject.SetActive(true);
                multiplier = Utilities.MULTIPLIER_LEVEL3;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Set behaviour for Resume Game button. Update user profile on chain with temporary penalty.
    /// If update succeed player can start game, player got penalty if refuses to complete the transaction. 
    /// </summary>
    /// <param name="gameStart"></param>
    public async void OnBackToGame(bool gameStart)
    {
        if (!gameStarted)
        {
            gameStarted = true;
            var slashingUserProfile = JsonConvert.DeserializeObject<UserProfile>(JsonConvert.SerializeObject(userProfile));
            // Reset temporarly current level and Secondary Balance
            slashingUserProfile.CurrentLevel = Utilities.LEVEL1;
            slashingUserProfile.SecondaryBalance = 0;

            await userService.UpdateUserProfile(slashingUserProfile, typeOfTransaction: "GAME_START");
        }
        
        if(gameStart)
        {
            descriptionPanel.gameObject.SetActive(false);
            powerUpsGroup.gameObject.SetActive(true);
            isGameRunning = true;
        }

    }

    /// <summary>
    /// Set behaviour on pressing info button which shows level rules and blockchain info.
    /// </summary>
    public void OnInfoButton()
    {
        descriptionPanel.gameObject.SetActive(true);
        powerUpsGroup.gameObject.SetActive(false);

    }

    /// <summary>
    /// Set behaviour of "Back to the Home" button. Check if player win and update user with winning info, such as updating balances, current level and power-up usage.
    /// Load home scene otherwise
    /// </summary>
    public async void OnBackToTheHome()
    {
        if (timerText.gameObject.tag == "Win")
        {
            await userService.UpdateUserProfile(userProfile, "GAME_TO_HOME");
        }
        else
        {
            SceneManager.LoadScene("Home");
        }

    }

    /// <summary>
    /// Set behaviour of Continue button. Check if player win and update user with winning info, such as updating balances, current level and power-up usage and load next level.
    /// Reload current game scene otherwise
    /// </summary>
    public async void OnContinue()
    {
        if (timerText.gameObject.tag == "Win")
        {
            await userService.UpdateUserProfile(userProfile, "CONTINUE_PLAYING");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /// <summary>
    /// Set behavious on using amethyst. 
    /// Checks availability and decrease number of owned amethysts.
    /// </summary>
    public void OnAmethyst()
    {
        if (userProfile.AmethystNumber > 0)
        {
            amethystCounter += 10;
            userProfile.AmethystNumber--;
        }
        else
        {
            JavascriptRepository.Web3Alert("Not enough Amethysts!", "warning");
        }

    }

    /// <summary>
    /// Set behaviour on using magic potion. Checks availability and decrease number of owned potions. 
    /// </summary>
    public void OnMagicPotion()
    {
        if (userProfile.PotionNumber > 0)
        {

            if (!isPotionUsed)
            {
                isPotionUsed = true;
                userProfile.PotionNumber--;
            }
            else
            {
                JavascriptRepository.Web3Alert("PotionX5 already used!", "warning");
            }
        }
        else
        {
            JavascriptRepository.Web3Alert("Not enough PotionX5!", "warning");

        }

    }

    /// <summary>
    /// Set the description of the current level
    /// </summary>
    private void SetLevelDescription()
    {
        string title = "";
        string description = "";
        switch (currentLevel)
        {
            case Utilities.LEVEL1:
                description = "Welcome to Puzzle Block - the game where blockchain technology turns into an exciting adventure! As a player, you're about to dive into the digital world of blocks and chains, where each move is a step towards understanding the fascinating workings of blockchain.\r\n\r\nIn Puzzle Block, you'll learn about hash functions – these are like secret codes that secure each block. Think of it as a unique digital fingerprint for every block you create. But there's a twist! To successfully add a block to the chain, you need to find a special number called the 'nonce.' It's like the missing piece of a puzzle that makes the hash just right.\r\n\r\nYour mission is to arrange digital blocks in a way that the hash function produces a specific pattern. It's a race against time and complexity, where finding the right nonce unlocks the next level. As you progress, you'll witness how blocks are securely linked together, forming an unbreakable chain – a core principle of blockchain technology.\r\nGet ready to solve, link, and learn in Puzzle Block – your gateway to mastering the world of blockchain!";
                title = "INTRODUCTION";
                break;
            case Utilities.LEVEL2:
                description = "In the world of blockchain, a transaction is like a digital promise, where one party agrees to send something of value (like cryptocurrency) to another. Imagine it as a secure, online exchange of goods, recorded for everyone to see.\r\n\r\nBut there's a challenge known as 'double spending.' Imagine you have $10 and you try to buy two things from different people at the same time with the same $10 bill. In the digital world, this could mean attempting to spend the same digital currency twice. Blockchain technology cleverly prevents this by maintaining a public ledger that tracks every transaction. Once a transaction is confirmed, it can't be used again, ensuring that each digital coin is spent only once, maintaining the integrity and trust in the system.";
                title = "TRANSACTIONS AND DOUBLE SPENDING";
                break;
            case Utilities.LEVEL3:
                description = "LMD-GHOST (Last Minimal Difficult Chain - Greedy Heaviest Observed SubTree) is a consensus algorithm for blockchains that aims to improve branch selection compared to simply following the longest chain. Instead of only considering the chain's length, it also takes into account orphaned blocks (those not included in the main chain) and their difficulty. It assigns them a weight based on how close they are to the main chain and how difficult they are. When deciding which branch to follow, it looks for the heaviest and greediest subtree, considering both the main chain and the orphans. The goal is to make the system more secure and efficient by addressing the issue of orphaned blocks. It's worth noting that Ethereum has chosen a different consensus mechanism called Proof of Stake (PoS) with the implementation of Casper, abandoning the use of LMD-GHOST and Proof of Work (PoW).";
                title = "BLOCKCHAIN AND CONSENSUS";
                break;
            default:
                break;

        }

        descriptionPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = title;
        descriptionPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = description;

    }

    /// <summary>
    /// Updates timer every second and checks timeout. Assign Loose status if timer reaches zero.
    /// </summary>
    public void UpdateTimer()
    {
        if (amethystCounter > 0)
        {
            amethystCounter -= Time.deltaTime;
        }
        else
        {
            timeRemaining -= Time.deltaTime;
        }

        // Ensure the timer doesn't go below zero
        if (timeRemaining < 0f)
        {
            timeRemaining = 0f;
            timerText.gameObject.tag = "Loose";
            Debug.Log("Timer reached zero!");
        }

        if (timeRemaining < 10f)
        {
            timerText.color = UnityEngine.Color.red;
        }


        // Format the time and update the UI text
        string minutes = Mathf.Floor(timeRemaining / 60).ToString("00");
        string seconds = (timeRemaining % 60).ToString("00");

        timerText.text = minutes + ":" + seconds;

    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

        if ((timerText.gameObject.tag == "Win" || timerText.gameObject.tag == "Loose") && isGameRunning)
        {
            isGameRunning = false;
            isGameOver = true;
        }

        if (isGameRunning)
        {
            UpdateTimer();
        }

        // Game over occurs if timer reaches zero, player input correct or wrong solution .
        if (isGameOver)
        {
            isGameOver = false;
            GameObject.FindGameObjectsWithTag("Info").FirstOrDefault().SetActive(false);
            powerUpsGroup.gameObject.SetActive(false);
            lvlOne.gameObject.SetActive(false);
            lvlTwo.gameObject.SetActive(false);
            lvlThree.gameObject.SetActive(false);
            descriptionPanel.gameObject.SetActive(false);


            if (timerText.gameObject.tag == "Win")
            {
                var temporaryCoins = Convert.ToInt32(timeRemaining * multiplier);
                temporaryCoins = isPotionUsed ? Convert.ToInt32(temporaryCoins * Utilities.POTION_MULTIPLIER) : temporaryCoins;

                gameOverText.text = "You have earned " + temporaryCoins + " PB!";

                userProfile.SecondaryBalance += temporaryCoins;
                if (currentLevel == Utilities.LEVEL3)
                {
                    userProfile.Points += userProfile.SecondaryBalance;
                    userProfile.PrimaryBalance += userProfile.SecondaryBalance;
                    userProfile.SecondaryBalance = 0;
                    userProfile.CurrentLevel = Utilities.LEVEL1;
                    userProfile.RunCompleted++;
                }
                else
                {
                    userProfile.CurrentLevel++;

                }

                Debug.Log("You Win!");
            }
            else
            {
                gameOverText.text = "Try again!";
                gameOverImage.sprite = Resources.Load<Sprite>("Images/Game/Defeat");
                continueText.text = "Restart Run";
                moneyPic.gameObject.SetActive(false);
                userProfile.SecondaryBalance = 0;
                userProfile.CurrentLevel = Utilities.LEVEL1;

                Debug.Log("You Loose!");

            }

            userProfile.GrimoireNumber = PlayerPrefs.GetInt("GrimoireNumber");


            PlayerPrefs.SetString("UserProfile", JsonConvert.SerializeObject(userProfile));

            gameOverPanel.gameObject.SetActive(true);

        }


    }
}
