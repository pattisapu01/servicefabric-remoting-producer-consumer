using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consumer.Domain;
using FabricsUtils;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Producer.Domain;

namespace Consumer
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Consumer : StatefulService, IConsumer
    {
        private static List<Task> _taskList;
        private static int _taskCount = 4;
        private static IProducer _proxy;
        private static ServiceProxyFactory _factory = ServiceFabricFactory.GetServiceProxyFactory();
        private static CancellationToken _ct;
        public Consumer(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var settings = new FabricTransportRemotingListenerSettings
            {
                MaxMessageSize = 325236432,
                MaxConcurrentCalls = 100
            };

            return new[]
            {
                new ServiceReplicaListener((c) => new FabricTransportServiceRemotingListener(c, this, settings))
            };
        }


        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _ct = cancellationToken;
            _proxy = _factory.CreateServiceProxy<IProducer>(new Uri("fabric:/ServiceFabric_Remoting_DuplicatesIssue/Producer"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey("0"));
            _taskList = new List<Task>();
            //start polling
            for (var i = 0; i <= _taskCount - 1; i++)
            {
                _taskList.Add(StartPolling());
            }
            await Task.WhenAll(_taskList);
        }
        private async Task StartPolling()
        {
            while (true)
            {
                try
                {
                    if (_ct.IsCancellationRequested)
                        break;
                    var i = await _proxy.FetchWork();
                    if (i > 0)
                        ServiceEventSource.Current.Message($"Received: {i}");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.Message("ERROR:" + ex.ToString());
                }
            }
        }
    }
}
