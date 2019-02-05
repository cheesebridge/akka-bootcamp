using System;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor: UntypedActor
    {
        public class StartTail
        {
            public string FilePath { get; }
            public IActorRef ReporterActor { get; }

            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }
        }

        public class StopTail
        {
            public string FilePath { get; }

            public StopTail(string filePath)
            {
                FilePath = filePath;
            }
        }

        /// <inheritdoc />
        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                
                // Here we are creating our first parent/child relationship
                // the TailActor instance created gere is a child 
                // of this instance of TailCoordinatorActor

                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        /// <inheritdoc />
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(10, TimeSpan.FromSeconds(30), x =>
            {
                //Maybe we consider ArithmeticException to not be application critical
                //so we just ignore the error and keep going.
                if (x is ArithmeticException) return Directive.Resume;

                //Error that we cannot recover from, stop the failing actor
                else if (x is NotSupportedException) return Directive.Stop;

                //In all other cases, just restart the failing actor
                else return Directive.Restart;
            } );
        }
    }
}