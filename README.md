## Proof Of Concept DataBus implementation for Rebus

Not used or tested this in production. Spend roughly two days on it.

Features:

- Databus properties ala NServicebus.
- Databus transport implemented as a file share, with the possiblity of writing your own `IDataBus`.
- Default .Net binary serializer (implement your own `IDataBusSerializer` if you want).
- Possibilty to write custom databus property offloaders (`IDataBusPropertyOffloader`) and loaders (`IDataBusPropertyLoader`). Offloaders are used on the sender's side and loaders are used on the receiver's side.
- SHA256 Checksumming on databus properties.
- GZIP compression on databus properties.
- The construction of needed databus types can either be specified by hand or with a DI container (using the service locator pattern for this - I saw no other way).
    -   Autofac example included.
- Configure transaction scopes arround reading and writing from the databus. Defaults to TransactionScopeOption.Suppress.
- Logging
- Created without modifications to the Rebus source code.

The producer and consumer code uses SqlServer as the transport for the messagebus. Should work with other transports as well but that's not tested.

###Bus setup example of producer code

```csharp
IBus bus = Configure.With(new AutofacServiceLocatorContainerAdapter(container))
          .Logging(l => l.Log4Net())
          .MessageOwnership(o => o.FromRebusConfigurationSection())
          .Transport(
              t =>
              {
                  t.UseSqlServerInOneWayClientMode(
                      "server=.;initial catalog=rebus_test;integrated security=true")
                      .EnsureTableIsCreated();

                  t.UseDataBus().EnableChecksums().UseServiceLocator();
              })
          .CreateBus()
          .Start();
```
###Bus setup example of consumer code
```csharp

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
            .Behavior(behavior => behavior.SetMaxRetriesFor<Exception>(0))
            .CreateBus()
            .Start(20);
```
