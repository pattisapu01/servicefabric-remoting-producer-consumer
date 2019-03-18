using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Producer.Domain;

namespace Producer
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Producer : StatefulService, IProducer
    {
        private static BufferBlock<int> _queue;
        public Producer(StatefulServiceContext context)
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
            InitQueue();
        }

        private void InitQueue()
        {
            _queue = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 20, EnsureOrdered=true }); //BoundedCapacity of 20 is assuming 5 partitions for consumerservice and 4 threads within each service calling "FetchWork()"
        }
        public async Task Produce(int itemCountToProduce)
        {
            var values = Enumerable.Range(1, itemCountToProduce);
            foreach (var value in values)
            {
                await _queue.SendAsync(value);
            }            
        }

        public async Task<int> FetchWork()
        {
            try
            {
                var i = await _queue.ReceiveAsync(new TimeSpan(0, 0, 2));
                ServiceEventSource.Current.Message($"Sending to producer: {i}");
                return i;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
