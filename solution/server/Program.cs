using System;
using Akka.Actor;
using NetMQ;
using Server.Actors;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("AkkaNetMqExample");

            Props messageProcessorProps = Props.Create(() => new MessageProcessor());
            IActorRef messageProcessor = actorSystem.ActorOf(messageProcessorProps, "someNonDefaultActor");

            var context = NetMQContext.Create();
            var server = context.CreateRouterSocket();
           
            server.Bind("tcp://*:5556");

            while (true)
            {
                NetMQMessage clientMessage = server.ReceiveMultipartMessage();
                Console.WriteLine("======================================");
                Console.WriteLine(" INCOMING CLIENT MESSAGE FROM CLIENT ");
                Console.WriteLine("======================================");
                PrintFrames("Server receiving", clientMessage);
                if (clientMessage.FrameCount == 3)
                {
                    messageProcessor.Tell(new MessageProcessor.ProcessMessage(clientMessage, server));                    
                }
            }
        }

        static void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }
    }
}
