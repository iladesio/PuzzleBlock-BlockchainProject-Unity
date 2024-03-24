using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IRepository 
{
    public Task<W> RetrieveData<W>(object payload = null, string action = "");
    public Task<W> RetrieveDataGet<W>(string action = "", List<(string,string)> headers = null);

    public void DoTransaction(object payload, string callbackData, string action = "api/contracts/getContractInfo", string typeOfTransaction = "");

}
