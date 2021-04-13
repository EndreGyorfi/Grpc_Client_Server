using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcServer.Server;
using System;

namespace GrpcServer
{
    public class ChatService : ChatRoom.ChatRoomBase
    {
        private readonly Server.ChatRoom _chatroomService;

        public ChatService(Server.ChatRoom chatRoomService)
        {
            _chatroomService = chatRoomService;
        }

        public override async Task sendPublic(IAsyncStreamReader<PublicMessage> requestStream, IServerStreamWriter<PublicMessage> responseStream, ServerCallContext context){
            if (!await requestStream.MoveNext()) return;
            string value;
            do
            { 
                value = requestStream.Current.User;
                _chatroomService.SendPublic(requestStream.Current.User, responseStream);
                await _chatroomService.BroadcastMessageAsync(requestStream.Current);
            } while (await requestStream.MoveNext());

        }

        public override async Task sendPrivate(IAsyncStreamReader<PrivateMessage> requestStream, IServerStreamWriter<PrivateMessage> responseStream, ServerCallContext context){
            if (!await requestStream.MoveNext()) return;
            string value;
            do
            {
                value = requestStream.Current.User;
                _chatroomService.SendPrivate(requestStream.Current.User, requestStream.Current.ToUser, responseStream);
                await _chatroomService.BroadcastMessagePrivateAsync(requestStream.Current);
            } while (await requestStream.MoveNext());
        }

        public override async Task sendToGroup(IAsyncStreamReader<GroupMessage> requestStream, IServerStreamWriter<GroupMessage> responseStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext()) return;
            string value;
            do
            {
                value = requestStream.Current.FromUser;
                _chatroomService.SendToGroup(requestStream.Current.FromUser, requestStream.Current.ToGroup, responseStream);
                await _chatroomService.BroadcastMessageGroupeAsync(requestStream.Current);
            } while (await requestStream.MoveNext());
        }


    }
}
