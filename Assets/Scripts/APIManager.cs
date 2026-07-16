using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }

    private const string BASE_URL = "https://game-backend-pv2m.onrender.com/api";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class AuthRequest
    {
        public string name;
        public string registrationNumber;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public bool success;
        public AuthData data;
    }

    [System.Serializable]
    public class AuthData
    {
        public string token;
        public string name;
        public string registrationNumber;
    }

    [System.Serializable]
    public class ScoreRequest
    {
        public int score;
    }

    [System.Serializable]
    public class LeaderboardResponse
    {
        public bool success;
        public LeaderboardData data;
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public LeaderboardEntry[] leaderboard;
        public int total;
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;
        public string registrationNumber;
    }

    public void RegisterUser(string name, string regNo, Action<bool, string> onComplete)
    {
        StartCoroutine(RegisterCoroutine(name, regNo, onComplete));
    }

    private IEnumerator RegisterCoroutine(string name, string regNo, Action<bool, string> onComplete)
    {
        string url = BASE_URL + "/auth/register";
        
        AuthRequest requestData = new AuthRequest { name = name, registrationNumber = regNo };
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                if (response != null && response.success && response.data != null)
                {
                    PlayerPrefs.SetString("AuthToken", response.data.token);
                    PlayerPrefs.Save();
                    onComplete?.Invoke(true, "Successfully logged in!");
                }
                else
                {
                    onComplete?.Invoke(false, "Login failed. Server returned success=false.");
                }
            }
            else
            {
                onComplete?.Invoke(false, "Connection error: " + request.error + " | " + request.downloadHandler.text);
            }
        }
    }

    public void SubmitScore(int score, Action<bool, string> onComplete)
    {
        StartCoroutine(SubmitScoreCoroutine(score, onComplete));
    }

    private IEnumerator SubmitScoreCoroutine(int score, Action<bool, string> onComplete)
    {
        string token = PlayerPrefs.GetString("AuthToken", "");
        if (string.IsNullOrEmpty(token))
        {
            onComplete?.Invoke(false, "No Auth Token found. Please login first.");
            yield break;
        }

        string url = BASE_URL + "/score/submit-score";
        
        ScoreRequest requestData = new ScoreRequest { score = score };
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(true, "Score submitted successfully!");
            }
            else
            {
                onComplete?.Invoke(false, "Connection error: " + request.error + " | " + request.downloadHandler.text);
            }
        }
    }

    public void GetLeaderboard(Action<bool, LeaderboardEntry[]> onComplete)
    {
        StartCoroutine(GetLeaderboardCoroutine(onComplete));
    }

    private IEnumerator GetLeaderboardCoroutine(Action<bool, LeaderboardEntry[]> onComplete)
    {
        string url = BASE_URL + "/leaderboard?limit=10";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(request.downloadHandler.text);
                if (response != null && response.success && response.data != null)
                {
                    onComplete?.Invoke(true, response.data.leaderboard);
                }
                else
                {
                    onComplete?.Invoke(false, null);
                }
            }
            else
            {
                onComplete?.Invoke(false, null);
            }
        }
    }
}
