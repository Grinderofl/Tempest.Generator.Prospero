using Microsoft.Extensions.DependencyInjection;
using Tempest.Core.Conventions;

namespace Tempest.Generator.Prospero.Conventions
{
    public class ProsperoConvention : IServiceConfigurationConvention
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<ProsperoOptions>();
        }
    }
}