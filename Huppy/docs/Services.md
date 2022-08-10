# Services
## 🔰 `List of services`
- AiStabilizerService
- CommandService
- EventService
- GPTService
- HuppyCacheService
- LoggerService
- NewsService
- PaginatorService
- ReminderService
- ServerInteractionService
- TimedEventsService
- UrbanService

## 👾 `Services that provide basic functionality`
- CommandService
- EventService
- TimedEventsService
- HuppyCacheService
- PaginatorService

## ⚗️ `Descriptions`
### AiStabilizerService
> *not yet implemented*

### CommandService
> Reponsible for all commands & components event handling.
> Registers command modules and logs what command has been used.

### EventService
> Starts the event loop, which allows to enqueue events at the specific time

### GPTService
> Handles AI related commands, sends requests to OpenAI API

### HuppyCacheService
> Main cache storage, contains basic data like current paginator entries, basic user data or AI usage per user

### LoggerService
> Handles logging events, especially from discord.net wrapper

### NewsService <sup>deprecated</sup>
> Used to send news from news API

### PaginatorService
> Provides the service of pagination for embeds, it requires `HuppyCacheService` to save embeds data in cache and `PaginatorHandlers` to handle component events (commands)

### ReminderService
> Responsible for registering reminders to database and enqueuing them to `EventService`.<br>
> Fetching "fresh" reminders (that are executed in selected time period) is registered as a job in `TimedEventsService`

### ServerInteractionService
> Responsible for handling events related to guilds like `HuppyJoined`, `UserJoined`

### TimedEventsService
> Responsible for registering and job execution 

### UrbanService
> Handles Urban Dictionary related commands, sends requests to urban dictionary API