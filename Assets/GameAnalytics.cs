using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using System.Collections.Generic;

public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
    }

    public void TrackGameStart(string playerName)
    {
        CustomEvent e = new CustomEvent("game_start")
        {
            { "player_name", playerName },
            { "datetime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        AnalyticsService.Instance.RecordEvent(e);
        AnalyticsService.Instance.Flush();
    }

    public void TrackEnding(string ending, string playerName)
    {
        CustomEvent e = new CustomEvent("ending_reached")
        {
            { "ending", ending },
            { "player_name", playerName },
            { "play_time_seconds", (int)Time.realtimeSinceStartup },
            { "datetime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        AnalyticsService.Instance.RecordEvent(e);
        AnalyticsService.Instance.Flush();
    }

    public void TrackNameInput(string playerName)
    {
        CustomEvent e = new CustomEvent("name_input")
        {
            { "player_name", playerName },
            { "datetime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        AnalyticsService.Instance.RecordEvent(e);
        AnalyticsService.Instance.Flush();
    }
}