using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing an ethereum transaction's information useful for callback handling
/// </summary>
[Serializable]
public class Transaction
{
    public Transaction()
    {

    }

    public Transaction(string typeOfTransaction, string result, bool isSuccess)
    {
        TypeOfTransaction = typeOfTransaction;
        Result = result;
        IsSuccess = isSuccess;
    }

    public string TypeOfTransaction { get; set; }
    public string Result { get; set; }
    public bool IsSuccess { get; set; }
}
