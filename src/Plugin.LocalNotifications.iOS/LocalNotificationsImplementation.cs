﻿using System;
using Plugin.LocalNotifications.Abstractions;
using System.Linq;
using Foundation;
using UIKit;
using UserNotifications;

namespace Plugin.LocalNotifications
{
    /// <summary>
    /// Local Notifications implementation for iOS
    /// </summary>
    public class LocalNotificationsImplementation : ILocalNotifications
    {
        private const string NotificationKey = "LocalNotificationKey";

        /// <summary>
        /// Show a local notification
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        public void Show(string title, string body, int id = 0)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(.1, false);
                ShowUserNotification(title, body, id, trigger);
            }
            else
            {
                Show(title, body, id, DateTime.Now,RepeatInterval.No);
            }
        }

        /// <summary>
        /// Show a local notification at a specified time
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        /// <param name="notifyTime">Time to show notification</param>
        /// <param name="repeat">Set up repeat on notification</param>
        public void Show(string title, string body, int id, DateTime notifyTime,RepeatInterval repeat)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var trigger = UNCalendarNotificationTrigger.CreateTrigger(GetNSDateComponentsFromDateTime(notifyTime), false);
                ShowUserNotification(title, body, id, trigger);
            }
            else
            {
                var notification = new UILocalNotification
                {
                    FireDate = (NSDate)notifyTime,
                    AlertTitle = title,
                    AlertBody = body,
                    UserInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject(id), NSObject.FromObject(NotificationKey)),
                    RepeatInterval = GetRepeatInterval(repeat)
                };

                UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            }
        }

        private NSCalendarUnit GetRepeatInterval(RepeatInterval repeat)
        {
            switch (repeat)
            {
                case RepeatInterval.Year:
                    return NSCalendarUnit.Year;
                case RepeatInterval.Month:
                    return NSCalendarUnit.Month;
                case RepeatInterval.Day:
                    return NSCalendarUnit.Day;
                case RepeatInterval.Hour:
                    return NSCalendarUnit.Hour;
                case RepeatInterval.Minute:
                    return NSCalendarUnit.Minute;
                case RepeatInterval.Second:
                    return NSCalendarUnit.Second;
                default:
                    return NSCalendarUnit.Calendar;
            }

        }

        /// <summary>
        /// Cancel a local notification
        /// </summary>
        /// <param name="id">Id of the notification to cancel</param>
        public void Cancel(int id)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new string[] { id.ToString() });
                UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new string[] { id.ToString() });
            }
            else
            {
                var notifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
                var notification = notifications.Where(n => n.UserInfo.ContainsKey(NSObject.FromObject(NotificationKey)))
                    .FirstOrDefault(n => n.UserInfo[NotificationKey].Equals(NSObject.FromObject(id)));

                if (notification != null)
                {
                    UIApplication.SharedApplication.CancelLocalNotification(notification);
                }
            }
        }

        // Show local notifications using the UNUserNotificationCenter using a notification trigger (iOS 10+ only)
        void ShowUserNotification(string title, string body, int id, UNNotificationTrigger trigger)
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                return;
            }

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Body = body
                
            };
            
            var request = UNNotificationRequest.FromIdentifier(id.ToString(), content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (error) => { });
        }

        NSDateComponents GetNSDateComponentsFromDateTime(DateTime dateTime)
        {
            return new NSDateComponents
            {
                Month = dateTime.Month,
                Day = dateTime.Day,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                Second = dateTime.Second
            };
        }
    }
}
