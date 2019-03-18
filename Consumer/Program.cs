﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.EventFlow.ServiceFabric;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Runtime;
[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace Consumer
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {

                using (var diagnosticsPipeline = ServiceFabricDiagnosticPipelineFactory.CreatePipeline("consumer-events"))
                {

                    // The ServiceManifest.XML file defines one or more service type names.
                    // Registering a service maps a service type name to a .NET type.
                    // When Service Fabric creates an instance of this service type,
                    // an instance of the class is created in this host process.

                    ServiceRuntime.RegisterServiceAsync("ConsumerType",
                    context => new Consumer(context)).GetAwaiter().GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Consumer).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
