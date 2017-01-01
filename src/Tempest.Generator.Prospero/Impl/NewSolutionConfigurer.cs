using System;
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

        protected virtual Func<string, string> BuildCoreTarget => s => $"src/{_options.SolutionName}.Core/{s}";
        protected virtual Func<string, string> BuildWebTarget => s => $"src/{_options.SolutionName}.Web/{s}";


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
        }

        protected virtual void CopyWeb(IScaffoldBuilder builder)
        {
            var projectJson = builder.Copy.Resource(BuildWebResource("project.json"))
                .ToFile(BuildWebTarget("project.json"));
             UpdateProjectJson(projectJson);

            builder.Copy.Resource(BuildWebResource("appsettings.json"))
                .ToFile(BuildWebTarget("appsettings.json"));
            builder.Copy.Resource(BuildWebResource("bundleconfig.json"))
                .ToFile(BuildWebTarget("bundleconfig.json"));

        }

        protected virtual void CopyConsole(IScaffoldBuilder builder)
        {

        }

        public override int Order => 0;
    }

    public static class BuilderExtensions
    {
        public static OperationStep Resource(this CopyOperationBuilder builder, string resourcePath)
        {
            return builder.ResourceOf<ProsperoGenerator>(resourcePath);
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