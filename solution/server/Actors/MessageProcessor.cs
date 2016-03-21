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
        }

        private void HandleProcessMessage(ProcessMessage msg)
        {
            var clientMessage = msg.ClientMessage;
            var server = msg.Server;

            var clientAddress = clientMessage[0];
            var clientOriginalMessage = clientMessage[2].ConvertToString();

            string response = string.Format("{0}| back from server {1}",
                clientOriginalMessage, DateTime.Now.ToLongTimeString());

            var messageToClient = new NetMQMessage();
            messageToClient.Append(clientAddress);
            messageToClient.AppendEmptyFrame();
            messageToClient.Append(response);
            server.SendMultipartMessage(messageToClient);
        }
    }
}