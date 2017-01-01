using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProsperoTemplate.Core;
using Microsoft.Extensions.Configuration;
using Prospero.Application.Console;
using Prospero.Conventions.AutoMapper;
using Prospero.Conventions.Console;
using Prospero.DataAccess.Conventions;
using Prospero.DataAccess.EFCore.Conventions;
using Prospero.Extensions.EntityFramework.Conventions;
using Prospero.Extensions.EntityFramework.Conventions.SqlServer;
using Prospero.Extensions.Logging.Conventions;
using ProsperoTemplate.Core.Configuration;

namespace ProsperoTemplate.Console
{
    public class ProsperoTemplateConsole : DefaultConventionConsoleApplication
    {
        protected override void ConfigureApplication(ConsoleConventionConfiguration applicationConventions)
        {
            applicationConventions.Configuration(
                    conf =>
                        conf.AddJsonFile("appsettings.json")
                            .AddJsonFile("appsettings.Production.json", optional: true)
                            .AddEnvironmentVariables())
                .AddAssemblyOf<ProsperoTemplateConsole>()
                .AddAssemblyOf<ProsperoTemplateSettings>()
                .EnableAutomapper()
                .EnableLogging()
                .EnableEntityFramework(e => e.UseSqlServer())
                .EnableDataAccess(d => d.UseEntityFramework());
        }

        protected override void RunCore()
        {
            
        }
    }
}
