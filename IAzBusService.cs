using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureResponse
{
    public interface IAzBusService
    {
        public Task GetNewData(CancellationToken stoppingToken);
        public Task SendMessageAsync(ClienteModel modelMessage);
    }
}
