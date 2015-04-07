using System;
using Autofac;
using Rebus;
using Rebus.Configuration;
using Rebus.DataBus;
using Rebus.DataBus.Autofac;
using Rebus.DataBus.Configuration;
using Rebus.Log4Net;
using Rebus.Transports.Sql;

namespace Consumer
{
    internal class Program
    {
        private static void Main()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<FileShareDataBus>()
                .AsImplementedInterfaces()
                .WithParameter("basePath", @"c:\temp\")
                .SingleInstance();

            builder.RegisterType<JobHandler>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<BinaryDataBusSerializer>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DataBusPropertyLoader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DataBusSettings>().AsImplementedInterfaces().SingleInstance();

            IContainer container = builder.Build();

            //todo put bus in container.
            IBus bus = Configure.With(new AutofacServiceLocatorContainerAdapter(container))
                .Logging(l => l.Log4Net())
                .Transport(
                    t =>
                    {
                        t.UseSqlServer(
                            "server=.;initial catalog=rebus_test;integrated security=true",
                            "consumer",
                            "error").EnsureTableIsCreated();

                        t.UseDataBus().EnableChecksums().UseServiceLocator();
                    })
                
                //still deciding if i need this.
                //.Subscriptions(
                //    s =>
                //        s.StoreInSqlServer(
                //            "server=.;initial catalog=rebus_test;integrated security=true",
                //            "RebusSubscriptions").EnsureTableIsCreated())
                .Behavior(behavior => behavior.SetMaxRetriesFor<Exception>(0))
                .CreateBus()
                .Start(20);            

            Console.WriteLine("Consumer listening - press ENTER to quit");
            Console.ReadLine();
            
            container.Dispose();
        }
    }
}