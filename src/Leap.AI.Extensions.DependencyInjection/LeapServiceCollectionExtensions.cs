using Leap.AI.Core;
using Leap.AI.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Leap.AI.Extensions.DependencyInjection;

/// <summary>
/// Provides IServiceCollection extension methods for registering <see cref="LeapClient"/>
/// and its dependencies into the ASP.NET Core / generic host DI container.
/// </summary>
public static class LeapServiceCollectionExtensions
{
    /// <summary>
    /// Registers a <see cref="LeapClient"/> as a singleton using a fluent builder delegate.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    ///   Delegate that configures the <see cref="LeapClientBuilder"/>.
    ///   You must call at least one provider extension (e.g. <c>.UseOpenAi("key")</c>).
    /// </param>
    /// <returns>The service collection, for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddLeap(leap => leap
    ///     .UseOpenAi(builder.Configuration["OpenAI:ApiKey"]!)
    ///     .UseLogging()
    ///     .UseRetry(3));
    /// </code>
    /// </example>
    public static IServiceCollection AddLeap(
        this IServiceCollection services,
        Action<LeapClientBuilder> configure)
    {
        services.TryAddSingleton<LeapClient>(_ =>
        {
            var builder = LeapClient.Create();
            configure(builder);
            return builder.Build();
        });

        return services;
    }

    /// <summary>
    /// Registers a named <see cref="ILeapProvider"/> as a singleton, alongside a <see cref="LeapClient"/>
    /// pre-wired to that provider. Useful when you want to resolve the provider directly for testing.
    /// </summary>
    public static IServiceCollection AddLeapProvider<TProvider>(
        this IServiceCollection services,
        Func<IServiceProvider, TProvider> factory)
        where TProvider : class, ILeapProvider
    {
        services.TryAddSingleton<ILeapProvider>(factory);
        services.TryAddSingleton<LeapClient>(sp =>
        {
            var provider = sp.GetRequiredService<ILeapProvider>();
            return LeapClient.Create().WithProvider(provider).Build();
        });

        return services;
    }
}
