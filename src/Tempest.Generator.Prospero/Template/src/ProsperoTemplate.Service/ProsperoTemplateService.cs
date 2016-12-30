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

namespace ProsperoTemplate.Service
{
    public class ProsperoTemplateService : DefaultConventionConsoleApplication
    {
        protected override void ConfigureApplication(ConsoleConventionConfiguration applicationConventions)
        {
            applicationConventions.Configuration(
                    conf =>
                        conf.AddJsonFile("appsettings.json")
                            .AddJsonFile("appsettings.Production.json", optional: true)
                            .AddEnvironmentVariables())
                .AddAssemblyOf<ProsperoTemplateService>()
                .AddAssemblyOf<ProsperoTemplateModelBuilderAlteration>()
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
