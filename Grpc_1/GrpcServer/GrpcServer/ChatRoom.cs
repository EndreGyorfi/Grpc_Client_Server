using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Server
{
    public class ChatRoom
    {

        private ConcurrentDictionary<string, IServerStreamWriter<PublicMessage>> publicMessageContainer = new ConcurrentDictionary<string, IServerStreamWriter<PublicMessage>>();

        private List<privateMessageStruct> privateMessageContainer = new List<privateMessageStruct>();

        private List<groupMessageStruct> groupMessageConttainer = new List<groupMessageStruct>();


        struct privateMessageStruct
        {
            public string userS;
            public string toUserS;
            public IServerStreamWriter<PrivateMessage> response;
        }

        struct groupMessageStruct
        {
            public string fUser;
            public string tGroup;
            public IServerStreamWriter<GroupMessage> responseGroup;
        }


        public void SendPublic(string name, IServerStreamWriter<PublicMessage> response)
        {
            publicMessageContainer.TryAdd(name, response);
        }

        public void SendPrivate(string name,string toName, IServerStreamWriter<PrivateMessage> res)
        {
            privateMessageStruct ps;
            ps.userS = name;
            ps.toUserS = toName;
            ps.response = res;


            if (privateMessageContainer.Count == 0)
            {
                privateMessageContainer.Add(ps);
            }
            else
            {
                if (!privateMessageContainer.Any(x => (x.userS.Equals(ps.userS) && x.userS.Equals(ps.userS))))
                {
                    privateMessageContainer.Add(ps);
                }
            }
         
        }

        public void SendToGroup(string name, string toName, IServerStreamWriter<GroupMessage> res)
        {
            groupMessageStruct gms;
            gms.fUser = name;
            gms.tGroup = toName;
            gms.responseGroup = res;

            if (groupMessageConttainer.Count == 0)
            {
                groupMessageConttainer.Add(gms);
            }
            else
            {
                if (!groupMessageConttainer.Any(x => (x.fUser.Equals(gms.fUser) && x.fUser.Equals(gms.fUser))))
                {
                    groupMessageConttainer.Add(gms);
                }
            }
        }

        public async Task BroadcastMessageAsync(PublicMessage message) => await BroadcastMessages(message);
        private async Task BroadcastMessages(PublicMessage message)
        {

            foreach (var user in publicMessageContainer)
            {
                var item = await SendMessageToSubscriber(user, message);
            }


            PrivateMessage pm = new PrivateMessage();
            pm.User = message.User;
            pm.Text = message.Text;
            foreach (var user in privateMessageContainer)
            {
                var item = await SendMessageToSubscriberPrivate(user, pm);
            }


            GroupMessage gm = new GroupMessage();
            gm.FromUser = message.User;
            gm.Text = message.Text;
            foreach (var user in groupMessageConttainer)
            {
                var item = await SendMessageToSubscriberGroup(user, gm);
            }
        }

        public async Task BroadcastMessagePrivateAsync(PrivateMessage message) => await BroadcastMessagePrivate(message);
        private async Task BroadcastMessagePrivate(PrivateMessage message)
        {
            foreach(var user in privateMessageContainer)
            {
                if( message.ToUser.Equals(user.userS))
                {
                    await SendMessageToSubscriberPrivate(user, message);
                }
            }
        }

        public async Task BroadcastMessageGroupeAsync(GroupMessage message) => await BroadcastMessageGroup(message);
        private async Task BroadcastMessageGroup(GroupMessage message)
        {
            foreach (var user in groupMessageConttainer)
            {
                //if (message.ToUser.Equals(user.userS))
               // {
                    await SendMessageToSubscriberGroup(user, message);
               // }
            }
        }

        private async Task<Nullable<KeyValuePair<string, IServerStreamWriter<PublicMessage>>>> SendMessageToSubscriber(KeyValuePair<string, IServerStreamWriter<PublicMessage>> user, PublicMessage message)
        {
            try
            {
                await user.Value.WriteAsync(message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return user;
            }
        }

        private async Task<Nullable<privateMessageStruct>> SendMessageToSubscriberPrivate(privateMessageStruct user, PrivateMessage message)
        {
            try
            {
                await user.response.WriteAsync(message);

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return user;
            }
        }

        private async Task<Nullable<groupMessageStruct>> SendMessageToSubscriberGroup(groupMessageStruct user, GroupMessage message)
        {
            try
            {
                await user.responseGroup.WriteAsync(message);

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return user;
            }
        }


    }
}
