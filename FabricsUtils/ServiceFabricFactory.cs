using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;

namespace FabricsUtils
{
    public class ServiceFabricFactory
    {
        public static ServiceProxyFactory GetServiceProxyFactory()
        {
            FabricTransportRemotingSettings transportSettings = new FabricTransportRemotingSettings
            {
                OperationTimeout = TimeSpan.FromSeconds(600),
                MaxMessageSize = 325236432,
                MaxConcurrentCalls = 100
            };
            var retrySettings = new OperationRetrySettings(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(3), 5);
            var clientFactory = new FabricTransportServiceRemotingClientFactory(transportSettings);
            return new ServiceProxyFactory((c) => clientFactory, retrySettings);
        }
    }
    


}
