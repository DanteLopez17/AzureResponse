using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureResponse
{
    public class AzBusService : IAzBusService
    {
        private readonly ServiceBusClient _serviceBusClient;
        public AzBusService(ServiceBusClient service)
        {
            _serviceBusClient = service;
        }
        public async Task SendMessageAsync(ClienteModel modelMessage)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender("primerazure");

            var body = System.Text.Json.JsonSerializer.Serialize(modelMessage);

            var sbMessage = new ServiceBusMessage(body);

            await sender.SendMessageAsync(sbMessage);
        }

        public async Task GetNewData(CancellationToken stoppingToken)
        {
            string cadena = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;

            SqlConnection conn = new SqlConnection(cadena);
            conn.Open();
            SqlCommand command = new SqlCommand("Select Id, Nombre, Apellido, NroDocumento, ClienteId, EsNuevo FROM [ClienteNuevo] WHERE EsNuevo = 1", conn);
            var clientes = new List<ClienteModelId>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var cli = new ClienteModel()
                    {
                        Nombre = reader["Nombre"]?.ToString(),
                        Apellido = reader["Apellido"]?.ToString(),
                        NroDocumento = (int)reader["NroDocumento"]
                    };

                    await SendMessageAsync(cli);

                    var cliId = new ClienteModelId()
                    {
                        Nombre = reader["Nombre"]?.ToString(),
                        Apellido = reader["Apellido"]?.ToString(),
                        Id = Int32.Parse(reader["Id"]?.ToString())
                    };
                    clientes.Add(cliId);
                }
            }
            using (SqlCommand command2 = conn.CreateCommand())
            {
                foreach (var item in clientes)
                {
                    command2.CommandText = $"UPDATE ClienteNuevo SET EsNuevo = 0 WHERE Id = {item.Id}";
                    command2.ExecuteNonQuery();
                }
            }
            conn.Close();
        }
    }
}
