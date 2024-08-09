using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Collette.Data;

namespace WeatherAPI.Tests.Unit
{
    public class IngestionBase : IAsyncLifetime
    {
        protected IServiceProvider _serviceProvider;
        protected readonly IServiceCollection _serviceCollection;
        protected readonly IDbConnectionWrapper _dbConnectionWrapper;
        protected readonly DatabaseFixture _fixture;
        public IngestionBase(DatabaseFixture fixture) : this(fixture, _ => { }) { }

        public IngestionBase(DatabaseFixture fixture, Action<IServiceCollection> serviceAction)
        {
            _dbConnectionWrapper = fixture.DbConnection;

            var services = new ServiceCollection();

            services.AddScoped<IDbConnectionWrapper, DbConnectionWrapper>();

            services.AddSingleton(provider => fixture.Configuration);

            _fixture = fixture;

            serviceAction(services);
            ConfigureServices(services);

            _serviceCollection = services;
            _serviceProvider = services.BuildServiceProvider();
        }

        public virtual IServiceCollection ConfigureServices(IServiceCollection services) { return services; }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task DisposeAsync()
        {
            if (_serviceProvider is ServiceProvider)
            {
                await ((ServiceProvider)_serviceProvider).DisposeAsync();
            }
        }
    }
}
