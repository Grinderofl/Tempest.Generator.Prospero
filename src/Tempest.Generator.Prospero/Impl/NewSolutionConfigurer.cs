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

            builder.Globally.TransformToken("ProsperoTemplate", _options.SolutionName);

            // Copy build scripts
            CopyBuildScripts(builder);

            // Copy Core
            CopyCore(builder);

            // Copy Web
            CopyWeb(builder);

            // Copy Service
            CopyConsole(builder);

        }

        protected virtual Func<string, string> BuildTemplatePath => s => $"Tempest.Generator.Prospero.Template.{s}";

        protected virtual Func<string, string> BuildCorePath => s => $"Tempest.Generator.Prospero.Template.src.ProsperoTemplate.Core.{s}";

        protected virtual void CopyBuildScripts(IScaffoldBuilder builder)
        {
            if (_options.HasProjectType(ProjectTypes.Web))
                builder.Copy.Resource("build.cake");
            else
                builder.Copy.Resource("WebJob.build.cake");

            builder.Copy.Resource(BuildTemplatePath("build.ps1")).ToFile("build.ps1");
            builder.Copy.Resource(BuildTemplatePath("build.cmd")).ToFile("build.cmd");
            builder.Copy.Resource(BuildTemplatePath("global.json")).ToFile("global.json");

        }

        protected virtual void CopyCore(IScaffoldBuilder builder)
        {
            builder.Copy.Resource(BuildCorePath("project.json"));
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