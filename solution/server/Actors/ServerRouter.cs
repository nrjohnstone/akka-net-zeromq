using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NetMQ;
using NetMQ.Sockets;
using Server.Actors;

namespace server.Actors
{
    internal class ServerRouter : ReceiveActor
    {
        private int _managedThreadId;
        private RouterSocket _server;

        public class StartProcessing
        {
        }

        public ServerRouter()
        {
            Receive<StartProcessing>(msg => HandleStartProcessing());
            Receive<WaitForNextMessage>(msg => HandleWaitForNextMessage());
            Receive<MessageProcessor.ProcessMessageResponse>(msg => HandleProcessMessageResponse(msg));
            _managedThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void HandleWaitForNextMessage()
        {
            NetMQMessage clientMessage = _server.ReceiveMultipartMessage();
            Console.WriteLine("======================================");
            Console.WriteLine(" INCOMING CLIENT MESSAGE FROM CLIENT ");
            Console.WriteLine("======================================");
            //if (Thread.CurrentThread.ManagedThreadId != _managedThreadId)
            //    throw new Exception("Thread ID has changed");

            Console.WriteLine($"ThreadId {Thread.CurrentThread.ManagedThreadId}");
            PrintFrames("Server receiving", clientMessage);
            if (clientMessage.FrameCount == 3)
            {
                Props messageProcessorProps = Props.Create(() => new MessageProcessor());
                IActorRef messageProcessor = Context.ActorOf(messageProcessorProps);
                messageProcessor.Tell(new MessageProcessor.ProcessMessage(clientMessage));
            }
            Self.Tell(new WaitForNextMessage());
        }

        private void HandleProcessMessageResponse(MessageProcessor.ProcessMessageResponse msg)
        {
            var messageToClient = new NetMQMessage();
            messageToClient.Append(msg.ClientAddress);
            messageToClient.AppendEmptyFrame();
            messageToClient.Append(msg.Response);
            _server.SendMultipartMessage(messageToClient);
        }

        private void HandleStartProcessing()
        {
            var context = NetMQContext.Create();

            _server = context.CreateRouterSocket();

            _server.Bind("tcp://*:5556");
            
            Self.Tell(new WaitForNextMessage());    
        }

        internal class WaitForNextMessage
        {
        }

        private static void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }
    }
}