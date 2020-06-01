# Elasticsearch Ingress off an Azure IoT Hub

Hi! In this repo I have placed my latest template for triggering of an Azure Function app from IoTHub events, then taking the data from the event, creating a basic document structure and storing it in an Elasticsearch Index.

## Requirements

- An Azure Subscription
- An Elasticsearch Server with working auth credentials
- An Azure IoTHub receiving messages (this code can backfill the previous 24hours of messages on the queue)
- Visual Studio

## local.settings.json / Azure Configuration Format

```
{
  "IsEncrypted": false,
  "Values": {
    "IoTConnectionString": "",
    "ELASTIC_APM_SERVER_URLS": "",
    "ELASTIC_APM_SERVICE_NAME": "",
    "ELASTIC_APM_SECRET_TOKEN": "",
    "ELASTIC_APM_ENVIRONMENT": "",
    "Elastic_Url": "",
    "Elastic_Username": ",
    "Elastic_Index_Name": "",
    "Elastic_Password": "",
    "APPINSIGHTS_INSTRUMENTATIONKEY": "",
    "APPLICATIONINSIGHTS_CONNECTION_STRING": "",
    "AzureWebJobsStorage": "",
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING": ""
  }
}
```