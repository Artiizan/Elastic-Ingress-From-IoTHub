# Elasticsearch Ingress off an Azure IoT Hub

Hi! In this repo I have placed my latest template for triggering of an Azure Function app from IoTHub events, then taking the data from the event, creating a basic document structure and storing it in an Elasticsearch Index.

## Requirements

- An Azure Subscription
- An Elasticsearch Server with working auth credentials
- An Azure IoTHub receiving messages (this code can backfill the previous 24hours of messages on the queue)
- Visual Studio