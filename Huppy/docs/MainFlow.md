# Significant Locations
- `#Program`: entry point, creates/gets config and configures IoC container then runs the bot
- `#AppSettings`: responsible for `appsettings.json` creation and deserialization
- `#SerilogConfigurator`: Configures the logger with proper logging levels
- `#ModuleConfigurator`: responsible for configurating the IoC container and `#IServiceProvider` creation
<br>Adds services like:
  - `#AppSettings`
  - Logger
  - Discord Client
  - Database context
  - Named HttpClients
  - Other services
- `#Creator`:  responsible for starting the bot loop<br>
Tasks:
    - `#Creator.CreateDatabase()`: creates database if it doesn't exist already from migrations
    - `#Creator.PopulateSingletons()`: populates singleton lifescope services with data
    - `#Creator.CreateCommands()`: registers the command modules via reflection to `IServiceProvider` from `ICommandHandlerService`
    - `#Creator.CreateEvents()`: creates the events
    - `#Creator.CreateBot()`: logins the bot to discrod and starts it
    - `#Creator.StartTimedEvents`: starts the timed events loop
    - `#Creator.CreateSlashCommands`: submits the slash commands list to discord
- `#ICommandHandlerService`: registers command modules and handles command events like:
  - *Command*: event responsible for every command type execution
  - *SlashCommandExecuted*: invoked after command gets executed
  - *ComponentCommandExecuted*: invoked after command gets executed
- `#ITimedEventsService`: runs the timers for events execution 