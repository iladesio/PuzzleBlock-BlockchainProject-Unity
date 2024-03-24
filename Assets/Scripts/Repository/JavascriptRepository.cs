using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Class that connects Unity WebGl and JavaScript, check Plugin file web3Connect.jslib to see body of functions.
/// </summary>
public class JavascriptRepository : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void Web3Connect();

    [DllImport("__Internal")]
    public static extern void Web3Disconnect(string useraddress);

    [DllImport("__Internal")]
    public static extern void Web3VerifyConnection();

    [DllImport("__Internal")]
    public static extern void Web3Transaction(string userAddress, string contractAddress, string data, string gameComponent, string callbackFn, string typeOfTransaction, string callbackData);

    [DllImport("__Internal")]
    public static extern void Web3Call(string contractAddress, string data, string gameComponent, string callbackFn);

    [DllImport("__Internal")]
    public static extern bool Web3VerifyMetamask();

    [DllImport("__Internal")]
    public static extern void Web3Test();

    /// <summary>
    /// type can be "success", "danger", "warning", "info"
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    [DllImport("__Internal")]
    public static extern void Web3Alert(string message, string type);

}
