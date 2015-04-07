using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Autofac;
using Messages;
using Rebus;
using Rebus.Configuration;
using Rebus.DataBus;
using Rebus.DataBus.Autofac;
using Rebus.DataBus.Configuration;
using Rebus.Log4Net;
using Rebus.Transports.Sql;

namespace Producer
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Producer");

            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<FileShareDataBus>()
                .AsImplementedInterfaces()
                .WithParameter("basePath", @"c:\temp\")
                .SingleInstance();
            builder.RegisterType<BinaryDataBusSerializer>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DataBusPropertyOffloader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DataBusSettings>().AsImplementedInterfaces().SingleInstance();

            IContainer container = builder.Build();

            //todo put bus in container.
            IBus bus =
                Configure.With(new AutofacServiceLocatorContainerAdapter(container))                    
                    .Logging(l => l.Log4Net())
                    .MessageOwnership(o => o.FromRebusConfigurationSection())
                    .Transport(
                        t =>
                        {
                            t.UseSqlServerInOneWayClientMode(
                                "server=.;initial catalog=rebus_test;integrated security=true").EnsureTableIsCreated();

                            t.UseDataBus().EnableChecksums().UseServiceLocator();
                        })
                    //still deciding if i need this.
                    //.Subscriptions(
                    //  s =>
                    //       s.StoreInSqlServer(
                    //          "server=.;initial catalog=rebus_test;integrated security=true",
                    //          "RebusSubscriptions").EnsureTableIsCreated())
                    
                    .CreateBus()
                    .Start();

            //some test code, sharing a transaction a cross multiple threads.

            //using (TransactionScope scope = new TransactionScope())
            //{
            //    Console.WriteLine("Doing parallel things with dependent transactions.");

            //    Transaction currentTransaction = Transaction.Current;

            //    Func<DependentTransaction> getDependentClone =
            //        () => currentTransaction.DependentClone(DependentCloneOption.RollbackIfNotComplete);

            //    IEnumerable<Job> jobs =
            //        Enumerable.Range(0, 20)
            //            .Select(
            //                rangeIndex =>
            //                    new Job
            //                    {
            //                        JobNumber = rangeIndex,
            //                        CompressedBlob = new DataBusCompressedProperty<byte[]>(new byte[1024*1024*5]),
            //                        NotCompressedBlob = new DataBusProperty<byte[]>(new byte[1024*1024*5])
            //                    });
                
            //    Parallel.ForEach(
            //        jobs,
            //        new ParallelOptions {MaxDegreeOfParallelism = 10},
            //        (job, state, index) => SendUsingDependentTransaction(job, index, bus, getDependentClone()));

            //    scope.Complete();
            //}

            bool keepRunning = true;
            while (keepRunning)
            {
                Console.WriteLine(@"Please enter your command:");
                Console.WriteLine(@"
a) Send 1 job
b) Send 10 jobs
c) Send 100 jobs
d) Send 1000 jobs
e) Send 10000 jobs (requires large amount of free space on your hd)
-------------------------------------------------------------------
q) Quit
");
                char key = char.ToLower(Console.ReadKey(true).KeyChar);

                switch (key)
                {
                    case 'a':
                        Send(1, bus);
                        break;
                    case 'b':
                        Send(10, bus);
                        break;
                    case 'c':
                        Send(100, bus);
                        break;
                    case 'd':
                        Send(1000, bus);
                        break;
                    case 'e':
                        Send(10000, bus);
                        break;
                    case 'q':
                        Console.WriteLine("Quitting");
                        keepRunning = false;
                        break;
                    default:
                        Console.WriteLine("Unknown command. Try again." + Environment.NewLine);
                        break;
                }
            }
        }

        public static void SendUsingDependentTransaction(Job job, long index, IBus bus, DependentTransaction dependentTransaction)
        {
            try
            {
                using (TransactionScope ts = new TransactionScope(dependentTransaction))
                {

                   // if (index == 15)
                    //    throw new Exception("Blah Blah");

                    bus.Send(job);

                    ts.Complete();
                }
            }
            finally
            {
                dependentTransaction.Complete();
                dependentTransaction.Dispose();
            }
        }

        private static void Send(int numberOfJobs, IBus bus)
        {
            Console.WriteLine("Publishing {0} jobs", numberOfJobs);

            //we throttle the maximum number Jobs created in memory at once. This throtteling has nothing
            //to do with the parallelism, which is set to 10.

            int maximumNumberOfJobsAtOnce = 100;
            int maximumCeiledLoopCount = 1;
            double preciseLoopCount = 1;

            if (numberOfJobs > 100)
            {
                preciseLoopCount = (double) numberOfJobs/100;
                maximumCeiledLoopCount = (int) Math.Ceiling(preciseLoopCount);
            }
            else
            {
                maximumNumberOfJobsAtOnce = numberOfJobs;
            }

            for (int currentLoopCount = 0; currentLoopCount < maximumCeiledLoopCount; currentLoopCount++)
            {
                int copyMaxiumNumbersOfJobs = maximumNumberOfJobsAtOnce;
                int copyCurrentLoopCount = currentLoopCount;

                if (copyCurrentLoopCount != 0 && copyCurrentLoopCount + 1 == maximumCeiledLoopCount)
                {
                    copyMaxiumNumbersOfJobs =
                        (int) ((preciseLoopCount - Math.Floor(preciseLoopCount))*copyMaxiumNumbersOfJobs);
                }

                IEnumerable<Job> jobs =
                    Enumerable.Range(0, copyMaxiumNumbersOfJobs)
                        .Select(
                            rangeIndex =>
                                new Job
                                {
                                    JobNumber = copyCurrentLoopCount*maximumNumberOfJobsAtOnce + rangeIndex,
                                    CompressedBlob = new DataBusCompressedProperty<byte[]>(new byte[1024*1024*5]),
                                    NotCompressedBlob = new DataBusProperty<byte[]>(new byte[1024*1024*5])
                                });

                Parallel.ForEach(jobs, new ParallelOptions {MaxDegreeOfParallelism = 10}, bus.Send);
            }
        }
    }
}