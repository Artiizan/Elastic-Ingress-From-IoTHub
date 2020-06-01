using System;
using System.Text;
using Elastic.Apm;
using Elastic.Apm.Api;
using ElasticIngress.Models;
using Elasticsearch.Net;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Nest;
using Newtonsoft.Json;
using Polly;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;
using Policy = Polly.Policy;

namespace ElasticIngress
{
    public static class Function
    {
        private static readonly string Today = DateTime.UtcNow.ToString("yyyy.MM.dd");
        private static readonly Uri Node = new Uri(Environment.GetEnvironmentVariable("Elastic_Url"));

        private static readonly ConnectionSettings Settings = new ConnectionSettings(Node).BasicAuthentication(
            Environment.GetEnvironmentVariable("Elastic_Username"),
            Environment.GetEnvironmentVariable("Elastic_Password"));

        private static readonly ElasticClient ElasticClient = new ElasticClient(Settings);

        [FunctionName("ElasticIngress")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IoTConnectionString",
                ConsumerGroup = "elasticingress")]
            EventData[] messages)
        {
            foreach (var message in messages)
            {
                var transaction = Agent.Tracer.StartTransaction("Process IoT Hub Message", ApiConstants.TypeRequest);
                try
                {
                    var plateRead = DeserializeAndMapData(transaction, message);
                    SendReadToElastic(transaction, plateRead);
                }
                catch (Exception e)
                {
                    transaction.CaptureException(e, "Process IoT Hub Message");
                }
                finally
                {
                    transaction.End();
                }
            }
        }

        private static ElasticDocument DeserializeAndMapData(IExecutionSegment transaction, EventData message)
        {
            var span =
                transaction.StartSpan("Deserialize and Map Data in Message", "Data Transformation");
            try
            {
                var readData = JsonConvert.DeserializeObject<ElasticDocument>(Encoding.UTF8.GetString(message.Body));
                return readData;
            }
            catch (Exception e)
            {
                span.CaptureException(e, "DeserializeAndMapData");
                return null;
            }
            finally
            {
                span.End();
            }
        }

        private static async void SendReadToElastic(IExecutionSegment transaction, ElasticDocument message)
        {
            var span =
                transaction.StartSpan("Send message to Elastic", "Elastic");
            try
            {
                var indexSpan = span.StartSpan("Index to Elastic", ApiConstants.TypeDb);
                try
                {
                    var res = await Policy
                        .Handle<ElasticsearchClientException>()
                        .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(3))
                        .ExecuteAsync(() => ElasticClient.IndexAsync(message,
                            idx => idx.Index(Environment.GetEnvironmentVariable("Elastic_Index_Name"))));
                }
                catch (Exception e)
                {
                    ElasticClient.Index(message, idx => idx.Index($"proof-reads-{Today}"));
                    indexSpan.CaptureException(e, "Index to Elastic");
                }
                finally
                {
                    indexSpan.End();
                }
            }
            catch (Exception e)
            {
                span.CaptureException(e, "SendDataToElastic");
            }
            finally
            {
                span.End();
            }
        }
    }
}