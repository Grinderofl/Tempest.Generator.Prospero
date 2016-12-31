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

        protected virtual string BuildProjectPath(string sub)
        {
            return $"src/{_options.ProjectName}/project.json";
        }

        protected virtual Func<string, string> BuildTemplatePath => s => $"Tempest.Generator.Prospero.Template.{s}";

        protected virtual Func<string, string> BuildCorePath => s => $"Tempest.Generator.Prospero.Template.src.ProsperoTemplate.Core.{s}";

        private string GetBuildCakeFile => _options.HasProjectType(ProjectTypes.Web)
            ? "build.cake"
            : "WebJob.build.cake";

        protected virtual void CopyBuildScripts(IScaffoldBuilder builder)
        {
            builder.Copy.Resource(BuildTemplatePath(GetBuildCakeFile)).ToFile("build.cake");

            builder.Copy.Resource(BuildTemplatePath("build.ps1")).ToFile("build.ps1");
            builder.Copy.Resource(BuildTemplatePath("build.cmd")).ToFile("build.cmd");
            builder.Copy.Resource(BuildTemplatePath("global.json")).ToFile("global.json");
        }

        protected virtual void CopyCore(IScaffoldBuilder builder)
        {
            builder.Copy.Resource(BuildCorePath("project.json")).ToFile($"src/{_options.ProjectName}/project.json");
            builder.Copy.Resource(BuildCorePath("ProsperoTemplate.Core.xproj")).ToFile($"src/{_options.SolutionName}.Core/{_options.SolutionName}.Core.xproj");
            builder.Copy.Resource(BuildCorePath("Configuration.ProsperoTemplateSettings.cs"))
                .ToFile($"src/{_options.SolutionName}/Configuration/{_options.SolutionName}Settings.cs");

            if (_options.HasComponent(ComponentTypes.EntityFramework))
                builder.Copy.Resource(BuildCorePath(
                        "Infrastructure.EntityFramework.ProsperoTemplateModelBuilderAlteration.cs"))
                    .ToFile(
                        $"src/{_options.SolutionName}/Infrastructure/EntityFramework/{_options.SolutionName}ModelBuilderAlteration.cs");

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
    }
}