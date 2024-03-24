using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;
using static Utilities;
using UnityEngine.UI;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using static System.Collections.Specialized.BitVector32;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Linq;

public class Repository : MonoBehaviour, IRepository
{
    public void CallbackFn(string value)
    {
        Debug.Log(value);
        JavascriptRepository.Web3Alert(value, "success");
    }

    /// <summary>
    /// Execute metamask transaction
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="callbackData"></param>
    /// <param name="action"></param>
    /// <param name="typeOfTransaction"></param>
    public async void DoTransaction(object payload, string callbackData, string action = "api/contracts/getContractInfo", string typeOfTransaction = "")
    {
        var response = await HttpPost(action, payload);
        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
        var userAddress = PlayerPrefs.GetString("UserAddress");
        JavascriptRepository.Web3Transaction(userAddress, result["contractAddress"], result["data"], gameComponent: "ScriptManager", callbackFn: "FinalizeTransaction", typeOfTransaction: typeOfTransaction, callbackData: callbackData);
    }

    /// <summary>
    /// Execute a Post call with a payload and return a certain type
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <param name="payload"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task<W> RetrieveData<W>(object payload = null, string action = "")
    {
        Debug.Log("Retrieving data");
        var ret = await HttpPost(action, payload);

        if (ret != null)
        {
            return JsonConvert.DeserializeObject<W>(ret);
        }
        else
        {
            return default(W);
        }


    }

    /// <summary>
    /// Execute a Get call with header and return a certain type
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <param name="action"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public async Task<W> RetrieveDataGet<W>(string action = "", List<(string, string)> headers = null)
    {
        var ret = await HttpGet(action, headers);
        Debug.Log("Retrieving data");
        if (ret != null)
        {
            return JsonConvert.DeserializeObject<W>(ret);
        }
        else
        {
            return default(W);
        }
    }

    /// <summary>
    /// Perform an HttpPost call with payload to a certain action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="requestData"></param>
    /// <returns></returns>
    private async Task<string> HttpPost(string action = "", object requestData = null)
    {
        string url = LOCALSERVER + action;

        if (requestData == null)
            requestData = new { };

        UnityWebRequest www = UnityWebRequest.Post(url, JsonConvert.SerializeObject(requestData), "application/json");
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();


        var loading = Instantiate(Resources.Load<GameObject>("Prefabs/Loading"), GameObject.Find("Canvas").transform);

        while (!operation.isDone)
            await Task.Yield();

        DestroyImmediate(loading);

        if (www.result != UnityWebRequest.Result.Success)
        {
            string errorMessage = www.downloadHandler.text;
            Debug.Log(errorMessage);
            JavascriptRepository.Web3Alert(errorMessage, "danger");
            return null;

        }
        else
        {
            return www.downloadHandler.text;
        }

    }

    /// <summary>
    /// Perform an HttpGet call with payload to a certain action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    private async Task<string> HttpGet(string action = "", List<(string, string)> headers = null)
    {
        string url = LOCALSERVER + action;

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader(headers.First().Item1, headers.First().Item2);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();

        var loading = Instantiate(Resources.Load<GameObject>("Prefabs/Loading"), GameObject.Find("Canvas").transform);

        while (!operation.isDone)
            await Task.Yield();

        DestroyImmediate(loading);

        if (www.result != UnityWebRequest.Result.Success)
        {
            string errorMessage = www.downloadHandler.text;
            JavascriptRepository.Web3Alert(errorMessage, "danger");
            Debug.Log(errorMessage);
            return null;

        }
        else
        {
            return www.downloadHandler.text;
        }

    }

    /// <summary>
    /// Transaction manager function. Handle callback and rollback when needed.
    /// </summary>
    /// <param name="transaction"></param>
    public async void FinalizeTransaction(string transaction)
    {

        Transaction t = JsonConvert.DeserializeObject<Transaction>(transaction);

        if (t.IsSuccess)
        {
            if (t.TypeOfTransaction == "REGISTRATION")
                gameObject.SendMessage("LoginPlayer", String.Empty);
            else if (t.TypeOfTransaction == "GAME_TO_HOME")
                SceneManager.LoadScene("Home");
            else if (t.TypeOfTransaction == "CONTINUE_PLAYING")
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            else if (t.TypeOfTransaction == "GAME_START")
            {
                gameObject.SendMessage("OnBackToGame", true);
                JavascriptRepository.Web3Alert("Transaction completed", "success");
            }
            else if (t.TypeOfTransaction == "PURCHASE_NFT")
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            else
            {
                JavascriptRepository.Web3Alert("Transaction completed", "success");
            }

        }
        else
        {
            if (t.TypeOfTransaction == "REGISTRATION")
            {
                var ret = await HttpPost("api/user/deleteUser", new { ipfsCid = t.Result });
                Debug.Log("Rollback: " + ret);
            }
            else if (t.TypeOfTransaction == "GAME_TO_HOME" || t.TypeOfTransaction == "CONTINUE_PLAYING" || t.TypeOfTransaction == "PURCHASE_NFT")
            {
                var callbacks = JsonConvert.DeserializeObject<Tuple<string, string, string>>(t.Result);
                var newIpfsCid = callbacks.Item1;
                var oldIpfsCid = callbacks.Item2;
                if (newIpfsCid != oldIpfsCid)
                {
                    var retPin = await HttpPost("api/user/pinUserByHash", new { ipfsCid = oldIpfsCid, username = callbacks.Item3 });
                    Debug.Log($"Rollback action 1: {retPin}");
                    var retDeletion = await HttpPost("api/user/deleteUser", new { ipfsCid = newIpfsCid });
                    Debug.Log($"Rollback action 2: {retDeletion}");
                }
                SceneManager.LoadScene("Home");
            }
            else if (t.TypeOfTransaction == "GAME_START")
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else { }
        }
    }


}