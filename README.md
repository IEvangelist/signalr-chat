# SignalR: Chat (Advanced)

[![Deploy App](https://github.com/IEvangelist/signalr-chat/actions/workflows/blazing-chat.yml/badge.svg)](https://github.com/IEvangelist/signalr-chat/actions/workflows/blazing-chat.yml)

## 💯 [Demo App](https://blazing-chat.azurewebsites.net)

## Run locally

To run locally, you'll need to have several environment variables created. The `configuration` object is expecting a value that doesn't exist. For the translator specifically, you'll need to create an Azure account, and corresponding Azure resource for the translator. There is a free tier, you can sign up here:

https://docs.microsoft.com/azure/cognitive-services/translator/translator-how-to-signup?wt.mc_id=dapine

#### Environment Variables

| Name | Value |
|------|-------|
| `TranslateTextOptions__ApiKey` | <Your Translator Resource's API key> |
| `TranslateTextOptions__Endpoint` | `https://api.cognitive.microsofttranslator.com/` |
| `TranslateTextOptions__Region` | <Your Translator Resource's Region> |

> After you've created the resource, and added the environment variables, close and reopen your IDE. It should then work.
