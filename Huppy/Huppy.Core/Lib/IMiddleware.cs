using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Lib
{
    public interface IMiddleware
    {
        Task BeforeAsync(ExtendedShardedInteractionContext context);
        Task AfterAsync(ExtendedShardedInteractionContext context);
    }

    public static class MiddlewareExtensions
    {
        public static void AddMiddleware<T>(this IServiceCollection services) where T : class
        {
            if (!typeof(IMiddleware).IsAssignableFrom(typeof(T)))
                throw new Exception($"Cannot assign {typeof(T)} to {typeof(IMiddleware)}");

            services.AddScoped<T>();
        }
    }
}