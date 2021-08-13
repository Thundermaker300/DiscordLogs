# DiscordLogs
SCP: Escape Together plugin that logs actions and sends them to a Discord server via Webhook.

## Requirements
* Current Version: V0.1.0
* Requires SCP:ET V0.2.0-alpha to function

## Config
| Config         | Description                                  | Default Value |
|----------------|----------------------------------------------|---------------|
| IsEnabled      | Whether or not Discord Logs are enabled.     | true          |
| WebhookUrl     | Determines the webhook to send logs to.      | string.Empty  |
| ChatWebhookUrl | Determines the webhook to send chat logs to. | string.Empty  |
  
Configuration not listed in the above table defines which logs that are sent. For example, "OnJoin" config determines whether or not join logs are sent.