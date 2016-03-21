using System;
using Akka.Actor;
using NetMQ;
using NetMQ.Sockets;

namespace Server.Actors
{
    internal class MessageProcessor : ReceiveActor
    {
        public class ProcessMessage
        {
            public NetMQMessage ClientMessage { get;}
            public RouterSocket Server { get; }

            public ProcessMessage(NetMQMessage clientMessage, RouterSocket server)
            {
                ClientMessage = clientMessage;
                Server = server;
            }
        }

        public MessageProcessor()
        {
            Receive<ProcessMessage>(msg => HandleProcessMessage(msg));
            ActorId = Guid.NewGuid().ToString();
        }

        public string ActorId { get; set; }

        private void HandleProcessMessage(ProcessMessage msg)
        {
            Console.WriteLine($"Message handled by actor {ActorId}");
            var clientMessage = msg.ClientMessage;
            var server = msg.Server;

            var clientAddress = clientMessage[0];
            var clientOriginalMessage = clientMessage[2].ConvertToString();

            var response = new byte[] {1, 2, 3, 4};

            var messageToClient = new NetMQMessage();
            messageToClient.Append(clientAddress);
            messageToClient.AppendEmptyFrame();
            messageToClient.Append(response);
            server.SendMultipartMessage(messageToClient);
        }
    }
}