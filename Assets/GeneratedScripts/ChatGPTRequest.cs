using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGPTRequest : MonoBehaviour
{
    public string ApiKey;
    public string RequestURL;
    public string RequestBody;

    public void SendRequest()
    {
        StartCoroutine(SendRequestCoroutine());
    }

    IEnumerator SendRequestCoroutine()
    {
        UnityWebRequest www = UnityWebRequest.Post(RequestURL, RequestBody);
        www.SetRequestHeader("Api-Key", ApiKey);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Request successful");
        }
    }
}