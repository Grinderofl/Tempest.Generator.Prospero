using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Tempest.Core.Configuration.Operations.OperationBuilding;
using Tempest.Core.Logging;
using Tempest.Core.Scaffolding;
using Tempest.Generator.Prospero.Extensions;

namespace Tempest.Generator.Prospero.Impl
{
    public class NewSolutionConfigurer : AbstractScaffolderConfigurer
    {
        private readonly ProsperoOptions _options;
        private readonly IEmitter _emitter;

        public NewSolutionConfigurer(ProsperoOptions options, IEmitter emitter)
        {
            _options = options;
            _emitter = emitter;
        }

        protected override void ConfigureScaffolder(IScaffoldBuilder builder)
        {
            _emitter.SetForegroundColor(ConsoleColor.Magenta);
            if (!_options.IsNewProject)
                return;
            builder.Set.TargetSubDirectory(_options.SolutionName);
            builder.Globally.TransformToken("ProsperoTemplate", _options.SolutionName);

            // Copy build scripts
            CopyBuildScripts(builder);

            // Copy Core
            CopyCore(builder);

            // Copy Web
            if(_options.HasProjectType(ProjectTypes.Web))
                CopyWeb(builder);

            // Copy Service
            if(_options.HasProjectType(ProjectTypes.Console))
                CopyConsole(builder);

        }

        protected virtual Func<string, string> BuildResource => s => $"Tempest.Generator.Prospero.Template.{s}";

        protected virtual Func<string, string> BuildCoreResource =>
            s => BuildResource($"src.ProsperoTemplate.Core.{s}");

        protected virtual Func<string, string> BuildWebResource =>
            s => BuildResource($"src.ProsperoTemplate.Web.{s}");

        protected virtual Func<string, string> BuildConsoleResource =>
            s => BuildResource($"src.ProsperoTemplate.Console.{s}");

        protected virtual Func<string, string> BuildCoreTarget => s => $"src/{_options.SolutionName}.Core/{s}";
        protected virtual Func<string, string> BuildWebTarget => s => $"src/{_options.SolutionName}.Web/{s}";
        protected virtual Func<string, string> BuildConsoleTarget => s => $"src/{_options.SolutionName}.Console/{s}";

        private string BuildCakeScriptFileName() => _options.HasProjectType(ProjectTypes.Web)
            ? "build.cake"
            : "WebJob.build.cake";

        protected virtual void CopyBuildScripts(IScaffoldBuilder builder)
        {
            builder.Copy.Resource(BuildResource(BuildCakeScriptFileName())).ToFile("build.cake");

            builder.Copy.Resource(BuildResource("build.ps1")).ToFile("build.ps1");
            builder.Copy.Resource(BuildResource("build.cmd")).ToFile("build.cmd");
            builder.Copy.Resource(BuildResource("global.json")).ToFile("global.json");
        }

        protected virtual void CopyCore(IScaffoldBuilder builder)
        {
            var projectJson = builder.Copy.Resource(BuildCoreResource("project.json"))
                .ToFile(BuildCoreTarget($"project.json"));

            UpdateProjectJson(projectJson);

            builder.Copy.Resource(BuildCoreResource("ProsperoTemplate.Core.xproj"))
                .ToFile(BuildCoreTarget($"{_options.SolutionName}.Core.xproj"));

            builder.Copy.Resource(BuildCoreResource("Configuration.ProsperoTemplateSettings.cs"))
                .ToFile(BuildCoreTarget($"Configuration/{_options.SolutionName}Settings.cs"));

            if(_options.HasComponent(ComponentTypes.EntityFramework))
            {
                _emitter.Emit("Adding EntityFramework");
                builder.Copy.Resource(BuildCoreResource("Infrastructure.EntityFramework.ProsperoTemplateModelBuilderAlteration.cs"))
                    .ToFile($"src/{_options.SolutionName}.Core/Infrastructure/EntityFramework/{_options.SolutionName}ModelBuilderAlteration.cs");
            }



        }

        private void UpdateProjectJson(OperationStep projectJson)
        {
            if (!_options.HasComponent(ComponentTypes.DataAccess))
            {
                projectJson.RemoveProjectJsonDataAccess();
            }
            else
            {
                _emitter.Emit("Enabling Data Access...");
            }

            if (!_options.HasComponent(ComponentTypes.AutoMapper))
            {
                projectJson.RemoveProjectJsonAutoMapper();
            }
            else
            {
                _emitter.Emit("Enabling AutoMapper...");
            }

            if (!_options.HasComponent(ComponentTypes.EntityFramework))
            {
                projectJson.RemoveProjectJsonEntityFramework();
            }
            else
            {
                _emitter.Emit("Enabling Entity Framework...");
            }


            if (!_options.HasProjectType(ProjectTypes.Console))
            {
                projectJson.TransformToken("\"postpublish\": [ ",
                    $"\"postpublish\": [ \n\t \"dotnet publish ..\\\\{_options.SolutionName}.Console\\\\ -o %publish:OutputPath%\\\\app_data\\\\jobs\\\\continuous\\\\{_options.SolutionName}.Console\\\\\",");
            }
        }

        private void UpdateStartupCs(OperationStep startupCs)
        {
            if (!_options.HasComponent(ComponentTypes.DataAccess))
            {
                startupCs
                    .RemoveToken(".EnableDataAccess(d => d.UseEntityFramework())")
                    .RemoveToken("using Prospero.DataAccess.EFCore.Conventions;");
            }

            if (!_options.HasComponent(ComponentTypes.EntityFramework))
            {
                startupCs
                    .RemoveToken($".EnableEntityFramework(x => x.UseSqlServer(s => s.MigrationsAssembly(\"{_options.SolutionName}.Web\")))")
                    .RemoveToken(".EnableEntityFramework(e => e.UseSqlServer())")
                    .RemoveToken("using Prospero.Extensions.EntityFramework.Conventions;")
                    .RemoveToken("using Prospero.Extensions.EntityFramework.Conventions.SqlServer;");

            }

            if (!_options.HasComponent(ComponentTypes.AutoMapper))
            {
                startupCs
                    .RemoveToken(".EnableAutomapper()")
                    .RemoveToken("using Prospero.Conventions.AutoMapper;");
            }
        }


        protected virtual void CopyWeb(IScaffoldBuilder builder)
        {
            var views = new string[]
            {
                "_ViewImports.cshtml",
                "_ViewStart.cshtml",
                "Home/About.cshtml",
                "Home/Contact.cshtml",
                "Home/Index.cshtml",
                "Shared/_Layout.cshtml",
                "Shared/Error.cshtml"
            };

            foreach (var view in views)
            {
                builder.Copy.Resource(BuildWebResource("Views." + view.Replace("/", "."))).ToFile(BuildWebTarget("Views/" + view));
            }

            var wwws = new[]
            {
                "css/site.css",
                "images/banner1.svg",
                "images/banner2.svg",
                "images/banner3.svg",
                "images/banner4.svg",
                "js/site.js",
                "favicon.ico"
            };

            foreach (var www in wwws)
            {
                builder.Copy.Resource(BuildWebResource("wwwroot." + www.Replace("/", "."))).ToFile(BuildWebTarget("wwwroot/" + www));
            }

//            builder.Copy.ResourcePath(BuildWebResource("Views")).ToFiles();
//            builder.Copy.ResourcePath(BuildWebResource("wwwroot")).ToFiles();
//            builder.Copy.ResourcePath(BuildWebResource("Controllers")).ToFiles();

            builder.Copy.Resource(BuildWebResource("Controllers.HomeController.cs"))
                .ToFile(BuildWebTarget("Controllers/HomeController.cs"));
            builder.Copy.Resource(BuildWebResource("appsettings.json"))
                .ToFile(BuildWebTarget("appsettings.json"));
            builder.Copy.Resource(BuildWebResource("bundleconfig.json"))
                .ToFile(BuildWebTarget("bundleconfig.json"));
            builder.Copy.Resource(BuildWebResource("web.config"))
                .ToFile(BuildWebTarget("web.config"));
            builder.Copy.Resource(BuildWebResource("web.release.config"))
                .ToFile(BuildWebTarget("web.release.config"));

            builder.Copy.Resource(BuildWebResource("ProsperoTemplate.Web.xproj"))
                .ToFile(BuildWebTarget($"{_options.SolutionName}.Web.xproj"));

            var projectJson = builder.Copy.Resource(BuildWebResource("project.json"))
                .ToFile(BuildWebTarget("project.json"));

            UpdateProjectJson(projectJson);

            if (_options.HasComponent(ComponentTypes.EntityFramework))
            {
                builder.Copy.Resource(BuildWebResource("Core.Factories.ProsperoTemplateWebDbContextFactory.cs"))
                    .ToFile(BuildWebTarget($"Core/Factories/{_options.SolutionName}DbContextFactory.cs"));
            }
        }

        protected virtual void CopyConsole(IScaffoldBuilder builder)
        {
            var projectJson = builder.Copy.Resource(BuildConsoleResource("project.json"))
                .ToFile(BuildConsoleTarget("project.json"));
            UpdateProjectJson(projectJson);

            builder.Copy.Resource(BuildConsoleResource("appsettings.json"))
                .ToFile(BuildConsoleTarget("appsettings.json"));
            builder.Copy.Resource(BuildConsoleResource("run.cmd"))
                .ToFile(BuildConsoleTarget("run.cmd"));
            builder.Copy.Resource(BuildConsoleResource("Program.cs"))
                .ToFile(BuildConsoleTarget("Program.cs"));
            var startupCs = builder.Copy.Resource(BuildConsoleResource("ProsperoTemplateConsole.cs"))
                .ToFile(BuildConsoleTarget($"{_options.SolutionName}Console.cs"));
            UpdateStartupCs(startupCs);

            builder.Copy.Resource(BuildConsoleResource("ProsperoTemplate.Console.xproj"))
                .ToFile(BuildConsoleTarget($"{_options.SolutionName}.Console.xproj"));
        }

        public override int Order => 0;
    }

    public static class BuilderExtensions
    {
        public static OperationStep Resource(this CopyOperationBuilder builder, string resourcePath)
        {
            return builder.ResourceOf<ProsperoGenerator>(resourcePath);
        }

        public static OperationStep ResourcePath(this CopyOperationBuilder builder, string resourcePath)
        {
            return builder.ResourcePathOf<ProsperoGenerator>(resourcePath);
        }

        public static OperationStep RemoveToken(this OperationStep step, string token)
        {
            return step.TransformToken(token, "");
        }

        public static OperationStep RemoveProjectJsonAutoMapper(this OperationStep step)
        {
            return step
                .RemoveToken("\"AutoMapper\": \"5.0.2\",")
                .RemoveToken("\"Prospero.Extensions.AutoMapper.Conventions\": \"1.0.0-*\",");
        }

        public static OperationStep RemoveProjectJsonDataAccess(this OperationStep step)
        {
            return step
                .RemoveToken("\"Prospero.DataAccess.Abstractions\": \"1.0.0-*\",")
                .RemoveToken("\"Prospero.DataAccess.EFCore\": \"1.0.0-*\",");
        }

        public static OperationStep RemoveProjectJsonEntityFramework(this OperationStep step)
        {
            return step
                .RemoveToken("\"FluentModelBuilder\": \"1.5.1\",")
                .RemoveToken("\"FluentModelBuilder.Relational\": \"1.5.1\",");
        }

    }
}