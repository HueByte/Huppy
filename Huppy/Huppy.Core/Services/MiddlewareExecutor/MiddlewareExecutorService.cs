using Discord;
using Discord.Interactions;
using Huppy.Core.Lib;
using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.MiddlewareExecutor
{
    public sealed class MiddlewareExecutorService
    {
        public event Func<string, Task>? OnLogAsync;
        private readonly List<Type> _middlewaresTypes = new();
        public MiddlewareExecutorService() { }

        public void AppendMiddleware<T>() where T : class
        {
            _middlewaresTypes.Add(typeof(T));
        }
        public async Task ExecuteBeforeAsync(AsyncServiceScope scope, ExtendedShardedInteractionContext ctx)
        {
            foreach (var type in _middlewaresTypes)
            {
                if (scope.ServiceProvider.GetRequiredService(type) is not IMiddleware middlewareInstance) return;
                await middlewareInstance.BeforeAsync(ctx);
                OnLogAsync?.Invoke($"Middleware :: Executing before {type} middleware");
            }
        }

        public async Task ExecuteAfterAsync(AsyncServiceScope scope, ICommandInfo commandInfo, ExtendedShardedInteractionContext context, IResult result)
        {
            for (int i = _middlewaresTypes.Count; i-- > 0;)
            {
                if (scope.ServiceProvider.GetRequiredService(_middlewaresTypes[i]) is not IMiddleware middlewareInstance) return;
                await middlewareInstance.AfterAsync(context!, commandInfo, result);
                OnLogAsync?.Invoke($"Middleware :: Executing after {_middlewaresTypes[i]} middleware");
            }
        }

        public void FillMissingMiddlewares()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.FullName!.StartsWith("Huppy"));

            List<Type> types = new();
            foreach (var asm in assemblies)
            {
                var middlewares = asm.GetTypes()
                    .Where(type => !type.IsInterface && typeof(IMiddleware).IsAssignableFrom(type) && !_middlewaresTypes.Contains(type))
                    .ToList();

                types.AddRange(middlewares);
            }

            _middlewaresTypes.AddRange(types);
        }
    }
}