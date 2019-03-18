using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading.Tasks;

namespace Producer.Domain
{
    public interface IProducer : IService
    {
        Task Produce(int itemCountToProduce);
        Task<int> FetchWork();
    }
}
