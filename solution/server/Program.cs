using System;
using Akka.Actor;
using NetMQ;
using server.Actors;
using Server.Actors;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("AkkaNetMqExample");
            Props serverRouterProps = Props.Create(() => new ServerRouter());
            var serverRouter = actorSystem.ActorOf(serverRouterProps, "server");
            
            serverRouter.Tell(new ServerRouter.StartProcessing());

            
            actorSystem.AwaitTermination();
        }
    }
}
