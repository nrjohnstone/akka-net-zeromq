using System;
using NetMQ;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
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
                    var clientAddress = clientMessage[0];
                    var clientOriginalMessage = clientMessage[2].ConvertToString();
                    string response = string.Format("{0} back from server {1}",
                        clientOriginalMessage, DateTime.Now.ToLongTimeString());
                    var messageToClient = new NetMQMessage();
                    messageToClient.Append(clientAddress);
                    messageToClient.AppendEmptyFrame();
                    messageToClient.Append(response);
                    server.SendMultipartMessage(messageToClient);
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
