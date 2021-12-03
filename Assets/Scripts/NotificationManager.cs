using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;

public class NotificationManager : MonoBehaviour
{
    private void Start()
    {
        CreateNotificationChannel();
        SendNotification();
    }

    public void CreateNotificationChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic notifications",
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    public void SendNotification()
    {
        var notification = new AndroidNotification();
        notification.Title = "ТЫ АБОБУС!";
        notification.FireTime = System.DateTime.Now.AddSeconds(10);
    }
}
