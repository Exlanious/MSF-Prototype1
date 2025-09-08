using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;

public enum LLMProviders { Gemini, OpenAI, Non_AI }

public class LLMCaller : MonoBehaviour
{

    private interface LLMService
    {
        //overridable method to send request
        public IEnumerator SendRequest(string prompt, System.Action<string> callback) { return null; }
    }

    private class Gemini : LLMService
    {
        /*
        * A NOTE ON API KEY USAGE: IF THERE ARE TOO MANMY REQUESTS, THE KEY WILL BE BLOCKED.
        * SWITCH TO NON_AI IF THIS HAPPENS
        */
        private const string apiKey = "key";
        private const string model = "gemini-2.5-flash";
        private const string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        private static bool isRequestInProgress = false;

        // Send a request to the Gemini API, if successful, invoke the callback with the response text
        public IEnumerator SendRequest(string prompt, System.Action<string> callback)
        {
            // Prevent overlapping requests
            if (isRequestInProgress)
                yield break;
            isRequestInProgress = true;

            string fullUrl = $"{apiUrl}{model}:generateContent?key={apiKey}";

            // Create the UnityWebRequest with headers
            UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
            request.SetRequestHeader("Content-Type", "application/json");

            string requestBody = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}],\"generationConfig\": {\"temperature\": 0.2,\"thinkingConfig\": {\"thinkingBudget\" : 0}}}";
            Debug.Log("Modified Request Body: " + requestBody);


            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

            //create req res objects
            request.uploadHandler = new UploadHandlerRaw(bodyRaw); //req
            request.downloadHandler = new DownloadHandlerBuffer(); //res


            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                callback?.Invoke(null);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                int startIndex = responseJson.IndexOf("\"text\": \"") + 9;
                int endIndex = responseJson.IndexOf("}") - 12;
                string responseText = responseJson.Substring(startIndex, endIndex - startIndex);
                callback?.Invoke(responseText);
            }
            request.Dispose();
            isRequestInProgress = false;
        }

    }

    private class OpenAI : LLMService
    {
        private const string apiKey = "key";
        private const string model = "gpt-4.1-mini";
        private const string apiUrl = "https://api.openai.com/v1/responses";

        private static bool isRequestInProgress = false;

        public IEnumerator SendRequest(string prompt, System.Action<string> callback)
        {
            if (isRequestInProgress)
                yield break;
            isRequestInProgress = true;

            // Create the UnityWebRequest with headers
            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // Create the JSON body
            string requestBody = $"{{\"model\": \"{model}\", \"input\": \"{prompt}\"}}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

            //create req res objects
            request.uploadHandler = new UploadHandlerRaw(bodyRaw); //req
            request.downloadHandler = new DownloadHandlerBuffer(); //res

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                callback?.Invoke(null);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log(responseJson);
                int startIndex = responseJson.IndexOf("\"text\": \"") + 9;
                int endIndex = responseJson.IndexOf("}") - 10;
                string responseText = responseJson.Substring(startIndex, endIndex - startIndex);
                callback?.Invoke(responseText);
            }
            request.Dispose();
            isRequestInProgress = false;
        }
    }

    //Singleton instance
    public static LLMCaller Instance;

    public static event Action onAIOFF;

    [SerializeField] private LLMProviders LLMProvider = LLMProviders.Non_AI;
    private LLMProviders currentLLMProvider = LLMProviders.Non_AI;
    private static LLMService llmService = null;

    private void Awake()
    {
        Instance = this;
        initializeLLMService(LLMProvider);
    }

    private void Update()
    {
        if (currentLLMProvider != LLMProvider)
        {
            initializeLLMService(LLMProvider);
        }
    }

    public LLMProviders getLLMProvider()
    {
        return LLMProvider;
    }

    private void initializeLLMService(LLMProviders providerToSet)
    {
        LLMProvider = providerToSet;
        switch (LLMProvider)
        {
            case LLMProviders.Gemini:
                llmService = new Gemini();
                break;
            case LLMProviders.OpenAI:
                llmService = new OpenAI();
                break;
            case LLMProviders.Non_AI:
                llmService = null;
                onAIOFF?.Invoke();
                break;
            default:
                llmService = null;
                onAIOFF?.Invoke();
                break;
        }
        currentLLMProvider = LLMProvider;
        Debug.Log($"LLM Service initialized to: {LLMProvider}");
    }

    public void switchLLMService(LLMProviders newLLMProvider)
    {
        initializeLLMService(newLLMProvider);
    }

    public void RequestResponse(string prompt, System.Action<string> callback)
    {
        if (llmService != null)
        {
            StartCoroutine(llmService.SendRequest(prompt, callback));
        }
        else
        {
            Debug.LogWarning("LLM Service is not initialized or set to Non_AI.");
            callback?.Invoke(null);
        }
    }

    //PUBLIC TRIGGER FUNCTIONS
    public void turnOffAI()
    {
        initializeLLMService(LLMProviders.Non_AI);
    }

    public void activateOpenAI()
    {
        initializeLLMService(LLMProviders.OpenAI);
    }

    public void activateGemini()
    {
        initializeLLMService(LLMProviders.Gemini);
    }
}