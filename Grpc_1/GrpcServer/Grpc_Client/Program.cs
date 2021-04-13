using System;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcServer;
using Grpc.Net.Client;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Grpc_Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Adja meg felhasznalonevet: ");
            var userName = Console.ReadLine();

            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ChatRoom.ChatRoomClient(channel);

            //var client = new Greeter.GreeterClient(channel);
            
            var tipus = "";

            while (tipus != "exit"){
                Console.Write("Adja meg az uzenet tipusat: ");
                tipus = Console.ReadLine();
                if (tipus == "public")
                {

                    using (var chat = client.sendPublic())
                    {
                        _ = Task.Run(async () =>
                        {
                            while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                            {
                                var response = chat.ResponseStream.Current;
                                Console.WriteLine($"{response.User}: {response.Text}");
                            }
                        });

                        //public message
                        await chat.RequestStream.WriteAsync(new PublicMessage { User = userName, Text = $"{userName} has joined the room" });
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            if (line.ToLower() == "back")
                            {
                                break;
                            }
                            await chat.RequestStream.WriteAsync(new PublicMessage { User = userName, Text = "<public>" + line });
                        }
                        await chat.RequestStream.CompleteAsync();
                    }
                }

                //private message eseten
                if (tipus == "private")
                {
                    Console.Write("Adja meg a felhasznalot akinek uzenetet szeretne kuldeni: ");
                    var felhasznaloNev = Console.ReadLine();
                    using (var chat = client.sendPrivate())
                    {
                        _ = Task.Run(async () =>
                        {
                            while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                            {
                                var response = chat.ResponseStream.Current;
                                Console.WriteLine($"{response.User}: {response.Text}");
                            }
                        });
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            if (line.ToLower() == "back")
                            {
                                break;
                            }
                            await chat.RequestStream.WriteAsync(new PrivateMessage { User = userName, ToUser = felhasznaloNev, Text = "<private>" + line });
                        }
                        await chat.RequestStream.CompleteAsync();
                    }
                }

                //group message
                if (tipus == "group")
                {
                    Console.Write("Adja meg a felhasznalot akit bevesz a csoportba (opcionalis): ");
                    var felhasznaloNev = Console.ReadLine();
                    using (var chat = client.sendToGroup())
                    {
                        _ = Task.Run(async () =>
                        {
                            while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                            {
                                var response = chat.ResponseStream.Current;
                                Console.WriteLine($"{response.FromUser}: {response.Text}");
                            }
                        });
                        string line;
                        while ((line = Console.ReadLine()) != null)
                        {
                            if (line.ToLower() == "back")
                            {
                                break;
                            }
                            await chat.RequestStream.WriteAsync(new GroupMessage { FromUser = userName, ToGroup = felhasznaloNev, Text = "<group>" + line });
                        }
                        await chat.RequestStream.CompleteAsync();
                    }
                }
            }



            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();

        }


    }
}

