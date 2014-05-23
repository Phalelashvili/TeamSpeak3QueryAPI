﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;
using TeamSpeak3QueryApi.Net.Specialized.Responses;

namespace TeamSpeak3QueryApi.Net.Specialized
{
    public class TeamSpeakClient
    {
        private readonly QueryClient _client;
        public QueryClient Client { get { return _client; } }

        private readonly List<Tuple<NotificationType, object, Action<NotificationData>>> _callbacks = new List<Tuple<NotificationType, object, Action<NotificationData>>>();

        #region Ctors

        /// <summary>Creates a new instance of <see cref="TeamSpeakClient"/> using the <see cref="QueryClient.DefaultHost"/> and <see cref="QueryClient.DefaultPort"/>.</summary>
        public TeamSpeakClient()
            : this(QueryClient.DefaultHost, QueryClient.DefaultPort)
        { }

        /// <summary>Creates a new instance of <see cref="TeamSpeakClient"/> using the provided host and the <see cref="QueryClient.DefaultPort"/>.</summary>
        /// <param name="hostName">The host name of the remote server.</param>
        public TeamSpeakClient(string hostName)
            : this(hostName, QueryClient.DefaultPort)
        { }

        /// <summary>Creates a new instance of <see cref="TeamSpeakClient"/> using the provided host TCP port.</summary>
        /// <param name="hostName">The host name of the remote server.</param>
        /// <param name="port">The TCP port of the Query API server.</param>
        public TeamSpeakClient(string hostName, short port)
        {
            _client = new QueryClient(hostName, port);
        }

        #endregion

        public Task Connect()
        {
            return _client.Connect();
        }

        #region Subscriptions

        public void Subscribe<T>(Action<IReadOnlyCollection<T>> callback)
            where T : Notification
        {
            var notification = GetNotificationType<T>();

            Action<NotificationData> cb = data => callback(DataProxy.SerializeGeneric<T>(data.Payload));

            _callbacks.Add(Tuple.Create(notification, callback as object, cb));
            _client.Subscribe(notification.ToString(), cb);
        }
        public void Unsubscribe<T>()
            where T : Notification
        {
            var notification = GetNotificationType<T>();
            var cbts = _callbacks.Where(tp => tp.Item1 == notification).ToList();
            cbts.ForEach(k => _callbacks.Remove(k));
            _client.Unsubscribe(notification.ToString());
        }
        public void Unsubscribe<T>(Action<IReadOnlyCollection<T>> callback)
            where T : Notification
        {
            var notification = GetNotificationType<T>();
            var cbt = _callbacks.SingleOrDefault(t => t.Item1 == notification && t.Item2 == callback as object);
            if (cbt != null)
                _client.Unsubscribe(notification.ToString(), cbt.Item3);
        }

        private static NotificationType GetNotificationType<T>()
        {
            NotificationType notification;
            if (!Enum.TryParse(typeof(T).Name, out notification)) // This may violate the generic pattern. May change this later.
                throw new ArgumentException("The specified generic parameter is not a supported NotificationType."); // For this time, we only support class-internal types which are listed in NotificationType
            return notification;
        }

        #endregion
        #region Implented api methods

        public Task Login(string userName, string password)
        {
            return _client.Send("login", new Parameter("client_login_name", userName), new Parameter("client_login_password", password));
        }

        public Task UseServer(int serverId)
        {
            return _client.Send("use", new Parameter("sid", serverId.ToString(CultureInfo.InvariantCulture)));
        }

        public async Task<WhoAmI> WhoAmI()
        {
            var res = await _client.Send("whoami");
            var proxied = DataProxy.SerializeGeneric<WhoAmI>(res);
            return proxied.FirstOrDefault();
        }

#region Register-Notification

        public Task RegisterChannelNotification(int channelId)
        {
            return RegisterNotification(NotificationEventTarget.Channel, channelId);
        }
        public Task RegisterServerNotification()
        {
            return RegisterNotification(NotificationEventTarget.Server, -1);
        }
        public Task RegisterTextServerNotification()
        {
            return RegisterNotification(NotificationEventTarget.TextServer, -1);
        }
        public Task RegisterTextChannelNotification()
        {
            return RegisterNotification(NotificationEventTarget.TextChannel, -1);
        }
        public Task RegisterTextPrivateNotification()
        {
            return RegisterNotification(NotificationEventTarget.TextPrivate, -1);
        }
        private Task RegisterNotification(NotificationEventTarget target, int channelId)
        {
            var ev = new Parameter("event", target.ToString().ToLowerInvariant());
            if (target == NotificationEventTarget.Channel)
                return _client.Send("servernotifyregister", ev, new Parameter("id", channelId));
            return _client.Send("servernotifyregister", ev);
        }

#endregion

        #endregion

    }
}
