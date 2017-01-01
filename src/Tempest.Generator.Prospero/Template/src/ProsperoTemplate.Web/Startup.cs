using ProsperoTemplate.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prospero.Application.AspNet.Conventions;
using Prospero.Conventions.AspNet;
using Prospero.Conventions.AspNet.Mvc.Install;
using Prospero.DataAccess.Conventions;
using Prospero.DataAccess.EFCore.Conventions;
using Prospero.Conventions.AutoMapper;
using Prospero.Extensions.AspNet.Developer.Conventions;
using Prospero.Extensions.AspNet.StaticFiles.Conventions;
using Prospero.Extensions.Logging.Conventions;
using Prospero.Extensions.EntityFramework.Conventions;
using Prospero.Extensions.EntityFramework.Conventions.SqlServer;
using ProsperoTemplate.Core.Configuration;

namespace ProsperoTemplate.Web
{
    public class Startup : DefaultConventionAspNetApplication
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;
        }

        protected override void ConfigureApplication(AspNetConventionConfiguration conventions)
        {
            conventions.Configuration(
                    conf =>
                        conf.SetBasePath(_env.ContentRootPath)
                            .AddJsonFile("appsettings.json")
                            .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional:true)
                            .AddEnvironmentVariables())
                .AddAssemblyOf<ProsperoTemplateSettings>()
                .AddAssemblyOf<Startup>()
                .EnableLogging()
                .EnableAutomapper()
                .EnableDeveloperExceptionPage()
                .EnableBrowserLink()
                .EnableStaticFiles()
                .EnableEntityFramework(x => x.UseSqlServer(s => s.MigrationsAssembly("ProsperoTemplate.Web")))
                .EnableDataAccess(d => d.UseEntityFramework())
                .EnableMvc();
        }

        protected override void ConfigureLoggerFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug().AddConsole();
            base.ConfigureLoggerFactory(loggerFactory);
        }
    }
}
