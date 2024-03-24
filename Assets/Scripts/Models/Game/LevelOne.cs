using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LevelOne : MonoBehaviour
{
    public TMP_InputField inputNonce;
    public TextMeshProUGUI blockDataText;
    public TextMeshProUGUI timerText;
    public Image grimoirePanel;
    public TextMeshProUGUI grimoireText;
    public TextMeshProUGUI hashingText;
    public TextMeshProUGUI criterionText;

    private int difficulty;
    private int blockData = 0;
    private List<Func<int, int, int>> hashings = new List<Func<int, int, int>>(); //type input,...,input,output
    private List<Func<int, bool>> criterions = new List<Func<int, bool>>();
    private List<List<string>> rules = new List<List<string>>();
    private int criterionNumber1, criterionNumber2;

    /// <summary>
    /// Setup level one panel before the first frame. The level is set dinamically according to current computed difficulty.
    /// </summary>
    public void Start()
    {
        difficulty = PlayerPrefs.GetInt("Difficulty");

        grimoirePanel.gameObject.SetActive(false);

        //Setup Block Data value and text
        System.Random random = new System.Random();
        // Generate a random number between 1000 and 9999
        blockData = random.Next(1000, 10000);
        blockDataText.text = "BLOCK DATA\n" + blockData + " byte";

        rules.Add(new List<string>() { "The output of F is the sum of the digits of the input", "The result must be an even number" });

        rules.Add(new List<string>() { "The output of F is the input with digits reversed", "The result must be an odd number" });

        criterionNumber1 = random.Next(40, 100);
        criterionNumber2 = random.Next(criterionNumber1, 100) + 1;

        rules.Add(new List<string>() { "The output of F is the sum of the digits of the input", "The result must be greater than " + criterionNumber1 + " and lower than " + criterionNumber2 });

        hashings.Add(HashEasy);
        hashings.Add(HashMedium);
        hashings.Add(HashDifficult);

        criterions.Add(CriterionEasy);
        criterions.Add(CriterionMedium);
        criterions.Add(CriterionDifficult);

        hashingText.text = "HASH FUNCTION\n\n" + rules[difficulty][0];
        criterionText.text = "CRITERION\n\n" + rules[difficulty][1];
    }

    /// <summary>
    /// Set behaviour on grimoire usage. Checks availability and update info on screen
    /// </summary>
    public void OnGrimoire()
    {
        var grimoireNumber = PlayerPrefs.GetInt("GrimoireNumber");
        //TODO: check number of grimoire from chain ?
        if (grimoireNumber > 0)
        {
            if (ValidateNonce(inputNonce.text))
            {
                int nonce = Int32.Parse(inputNonce.text);
                switch (difficulty)
                {
                    case 0:
                        grimoireText.text = "F(" + blockData + " + " + nonce + ") = F(" + (blockData + nonce) + ") = " + HashEasy(blockData, nonce);
                        break;
                    case 1:
                        grimoireText.text = "F(" + blockData + " + " + nonce + ") = F(" + (blockData + nonce) + ") = " + HashMedium(blockData, nonce);
                        break;
                    case 2:
                        grimoireText.text = "F(" + blockData + " + " + nonce + ") = F(" + (blockData + nonce) + ") = " + HashDifficult(blockData, nonce);
                        break;
                    default:
                        break;
                }
                // Update grimoire number usage and save it
                grimoireNumber--;
                PlayerPrefs.SetInt("GrimoireNumber", grimoireNumber);

                grimoirePanel.gameObject.SetActive(true);
                Debug.Log("Used Level 1 Grimoire");

            }
            else
            {
                JavascriptRepository.Web3Alert("Please insert a valid positive nonce to use the Grimoire!", "warning");
            }
        }
        else
        {
            JavascriptRepository.Web3Alert("Not enough Grimoires!", "warning");
        }

    }

    /// <summary>
    /// Validate Nonce input to be only integer number.
    /// </summary>
    /// <param name="nonce"></param>
    /// <returns></returns>
    private bool ValidateNonce(string nonce)
    {
        string pattern = @"^\d+$"; // Matches one or more digits from the start (^) to the end ($) of the string

        return Regex.IsMatch(nonce, pattern);
    }

    /// <summary>
    /// Set behaviour of Confirm button. After validating, set level status to "Win" or "Loose" according to criterion.
    /// </summary>
    public void OnConfirm()
    {

        if (ValidateNonce(inputNonce.text))
        {
            int nonce = Int32.Parse(inputNonce.text);

            if (criterions[difficulty](hashings[difficulty](blockData, nonce)))
                timerText.gameObject.tag = "Win";
            else
                timerText.gameObject.tag = "Loose";
        }
        else
        {
            JavascriptRepository.Web3Alert("Nonce must be a positive number.", "warning");
        }
    }

    /// <summary>
    /// Hash function for easy mode. Compute sum of blockdata and nonce and sums up the result's digits.
    /// </summary>
    /// <param name="blockData"></param>
    /// <param name="nonce"></param>
    /// <returns></returns>
    private static int HashEasy(int blockData, int nonce)
    {
        return (blockData + nonce).ToString().Sum(c => c - '0');
    }

    /// <summary>
    /// Hash function for medium mode. Compute the sum of blockdata and nonce and reverses the result.
    /// </summary>
    /// <param name="blockData"></param>
    /// <param name="nonce"></param>
    /// <returns></returns>
    private static int HashMedium(int blockData, int nonce)
    {
        var input = (blockData + nonce).ToString();

        char[] charArray = input.ToCharArray();
        Array.Reverse(charArray);
        return Int32.Parse(new string(charArray));

    }

    /// <summary>
    /// Hash function for hard mode. Compute sum of blockdata and nonce and sums up the result's digits.
    /// </summary>
    /// <param name="blockData"></param>
    /// <param name="nonce"></param>
    /// <returns></returns>
    private static int HashDifficult(int blockData, int nonce)
    {
        return (blockData + nonce).ToString().Sum(c => c - '0');
    }

    /// <summary>
    /// Return hash condition and return if the input is an even number.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    private bool CriterionEasy(int hash)
    {
        return hash % 2 == 0;
    }

    /// <summary>
    /// Return hash condition and return if the input is an odd number.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    private bool CriterionMedium(int hash)
    {
        return hash % 2 != 0;
    }

    /// <summary>
    /// Return hash condition and return if the input is included in certain, randomly generated, limits..
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    private bool CriterionDifficult(int hash)
    {
        return criterionNumber1 < hash && hash < criterionNumber2;
    }


}
