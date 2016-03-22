using System;
using System.Threading;
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

            public ProcessMessage(NetMQMessage clientMessage)
            {
                ClientMessage = clientMessage;
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
            Console.WriteLine($"ThreadId {Thread.CurrentThread.ManagedThreadId}");
            var clientMessage = msg.ClientMessage;
  
            var clientAddress = clientMessage[0];
            var clientOriginalMessage = clientMessage[2].ConvertToString();

            var response = CreateTestMessage(1024 * 1024 * 10);
 
            Sender.Tell(new MessageProcessor.ProcessMessageResponse(response, clientAddress));
        }

        internal class ProcessMessageResponse
        {
            public byte[] Response { get; }
            public NetMQFrame ClientAddress { get; }

            public ProcessMessageResponse(byte[] response, NetMQFrame clientAddress)
            {
                Response = response;
                ClientAddress = clientAddress;
            }
        }

        private byte[] CreateTestMessage(int size)
        {
            byte[] data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = (byte) ((byte) i%256);
            }
            return data;
        }
    }
}