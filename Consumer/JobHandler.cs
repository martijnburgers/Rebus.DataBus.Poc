using System;
using Messages;
using Rebus;

namespace Consumer
{
    public class JobHandler : IHandleMessages<Job>
    {
        public void Handle(Job message)
        {         
            Console.WriteLine("Handling job with {0}", message);
        }

    }
}