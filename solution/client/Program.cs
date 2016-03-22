using System;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace client
{
    class Program
    {
        private static Guid _uuid;

        static void Main(string[] args)
        {
            var delay = 2000;

            using (var context = NetMQContext.Create())
            {
                using (var dealer = context.CreateDealerSocket())
                {
                    using (var poller = new Poller())
                    {
                        poller.AddSocket(dealer);

                        _uuid = Guid.NewGuid();
                        dealer.Options.Identity =
                            Encoding.Unicode.GetBytes(_uuid.ToString());

                        dealer.Connect("tcp://127.0.0.1:5556");
                        dealer.ReceiveReady += Client_ReceiveReady;

                        poller.PollTillCancelledNonBlocking();


                        while (true)
                        {
                            var messageToServer = new NetMQMessage();
                            messageToServer.AppendEmptyFrame();
                            messageToServer.Append(_uuid.ToString());
                            Console.WriteLine("======================================");
                            Console.WriteLine(" OUTGOING MESSAGE TO SERVER ");
                            Console.WriteLine("======================================");
                            PrintFrames("Client Sending", messageToServer);
                            dealer.SendMultipartMessage(messageToServer);

                            Thread.Sleep(delay);
                        }
                    }
                }
            }
        }

        static void Client_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            bool hasmore = false;
            e.Socket.ReceiveFrameBytes(out hasmore);
            if (hasmore)
            {
                var result = e.Socket.ReceiveMultipartMessage();

                byte[] data = result[0].ToByteArray();

                Console.WriteLine("REPLY length = {0}", data.Length);

                //if (!data.SequenceEqual(new byte[] {1,2,3,4}))
                //    throw new Exception();
            
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
