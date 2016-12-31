using System;
using Tempest.Core.Configuration.Operations.OperationBuilding;
using Tempest.Core.Scaffolding;
using Tempest.Generator.Prospero.Extensions;

namespace Tempest.Generator.Prospero.Impl
{
    public class NewSolutionConfigurer : AbstractScaffolderConfigurer
    {
        private readonly ProsperoOptions _options;

        public NewSolutionConfigurer(ProsperoOptions options)
        {
            _options = options;
        }

        protected override void ConfigureScaffolder(IScaffoldBuilder builder)
        {
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

        protected virtual Func<string, string> BuildCoreResource => s => $"Tempest.Generator.Prospero.Template.src.ProsperoTemplate.Core.{s}";

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
            var projectJson = builder.Copy.Resource(BuildCoreResource("project.json")).ToFile($"src/{_options.SolutionName}.Core/project.json");

            Console.WriteLine(_options.Components.Count);
            Console.WriteLine(_options.ProjectTypes.Count);
            builder.Copy.Resource(BuildCoreResource("ProsperoTemplate.Core.xproj")).ToFile($"src/{_options.SolutionName}.Core/{_options.SolutionName}.Core.xproj");
            builder.Copy.Resource(BuildCoreResource("Configuration.ProsperoTemplateSettings.cs"))
                .ToFile($"src/{_options.SolutionName}.Core/Configuration/{_options.SolutionName}Settings.cs");

            if (!_options.HasComponent(ComponentTypes.DataAccess))
            {
                Console.WriteLine("Removing Data Access");
                projectJson.RemoveProjectJsonDataAccess();
            }

            if (!_options.HasComponent(ComponentTypes.AutoMapper))
            {
                Console.WriteLine("Removing AutoMapper");
                projectJson.RemoveProjectJsonAutoMapper();
            }

            if (!_options.HasComponent(ComponentTypes.EntityFramework))
            {
                Console.WriteLine("Removing EntityFramework");
                projectJson.RemoveProjectJsonEntityFramework();
            }
            else
            {
                Console.WriteLine("Adding EntityFramework");
                builder.Copy.Resource(BuildCoreResource("Infrastructure.EntityFramework.ProsperoTemplateModelBuilderAlteration.cs"))
                    .ToFile($"src/{_options.SolutionName}.Core/Infrastructure/EntityFramework/{_options.SolutionName}ModelBuilderAlteration.cs");
            }
        }

        protected virtual void CopyWeb(IScaffoldBuilder builder)
        {

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
            return step.RemoveToken("\"AutoMapper\": \"5.0.2\",");
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