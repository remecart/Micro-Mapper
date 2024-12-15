using System.Collections;
using System.ComponentModel;
using UnityEngine;
using Discord;

public class DiscordRPC : MonoBehaviour
{
    public Discord.Discord discord;

    public Activity activity;

    // Start is called before the first frame update
    void Start()
    {
        discord = new Discord.Discord(1317471580837646399, (ulong)Discord.CreateFlags.NoRequireDiscord);

        activity = new Activity
        {
            State = "-",
            Details = "-"
        };
        ChangeActivity(activity);
        StartCoroutine(LoadDiscordRichPresence());
    }

    void OnDisable()
    {
        discord.Dispose();
    }

    void Update()
    {
        discord.RunCallbacks();
    }

    void ChangeActivity(Activity activity)
    {
        var activityManager = discord.GetActivityManager();
        activityManager.UpdateActivity(activity, (res) => { });
    }
    
    IEnumerator LoadDiscordRichPresence()
    {
        while (true)
        {
            if (ReadMapInfo.instance)
            {
                var diff = LoadMap.instance.diff.ToString();
                var character= LoadMap.instance.beatchar.ToString();
                activity.State = diff + " " + character;
                activity.Details = EditMetaData.instance.metaData._songAuthorName + " - " + EditMetaData.instance.metaData._songName;
            }
            else if (EditMetaData.instance)
            {
                activity.State = "Editing metadata.";
                activity.Details = EditMetaData.instance.metaData._songAuthorName + " - " + EditMetaData.instance.metaData._songName;
            }
            else if (LoadMapsFromPath.instance)
            {
                activity.State = "Scrolling though song list.";
                activity.Details = "";
            }

            ChangeActivity(activity);
            yield return new WaitForSecondsRealtime(5f);
        }
    }
}