# DiscordLogs
SCP: Escape Together plugin that logs actions and sends them to a Discord server via Webhook.

## Requirements
* Current Version: V0.1.0
* Requires SCP:ET V0.2.0-alpha (MUST be on public beta)

## Config
| Config          | Description                                                                                               | Default Value |
|-----------------|-----------------------------------------------------------------------------------------------------------|---------------|
| IsEnabled       | Whether or not Discord Logs are enabled.                                                                  | true          |
| WebhookUrl      | Determines the webhook to send logs to. This will error if not provided.                                  | string.Empty  |
| ChatWebhookUrl  | Determines the webhook to send chat logs to. If not provided, this will use the value of the WebhookUrl.  | string.Empty  |
| AdminWebhookUrl | Determines the webhook to send admin logs to. If not provided, this will use the value of the WebhookUrl. | string.Empty  |
  
Configuration not listed in the above table defines which logs that are sent. For example, "OnJoin" config determines whether or not join logs are sent.