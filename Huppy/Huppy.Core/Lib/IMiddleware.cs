using Discord;
using Discord.Interactions;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.MiddlewareExecutor;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Lib
{
    public interface IMiddleware
    {
        Task BeforeAsync(ExtendedShardedInteractionContext context);
        Task AfterAsync(ExtendedShardedInteractionContext context, ICommandInfo commandInfo, IResult result);
    }

    public static class MiddlewareExtensions
    {
        public static void UseMiddleware<T>(this IServiceCollection services, MiddlewareExecutorService middlewareExecutor) where T : class
        {
            if (!typeof(IMiddleware).IsAssignableFrom(typeof(T)))
                throw new Exception($"Cannot assign {typeof(T)} to {typeof(IMiddleware)}");

            services.AddScoped<T>();
            middlewareExecutor.AppendMiddleware<T>();
        }
    }
}