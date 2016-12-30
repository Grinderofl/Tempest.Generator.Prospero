using System;
using Tempest.Core.Configuration.Operations.OperationBuilding;
using Tempest.Core.Generator;
using Tempest.Core.Scaffolding;
using Tempest.Generator.Prospero.Extensions;

namespace Tempest.Generator.Prospero
{
    public class ProsperoGenerator : GeneratorBase
    {
        private readonly ProsperoOptions _options;

        public ProsperoGenerator(ProsperoOptions options)
        {
            _options = options;
        }

        protected override void ConfigureOptions(OptionsFactory options)
        {
            options.Input("Welcome to Prospero generator. Please enter the name of the solution:", s => _options.SolutionName = s);
            options.List("What would you like to do?")
                .Choice("Create new solution", "solution", () => _options.IsNewProject = true)
                .Choice("Update existing solution", "existing");

            ConfigureNewSolutionOptions(options);
            ConfigureExistingOptions(options);
            ConfigureLibraries(options);
        }

        // This is a checklist.
        private void ConfigureNewSolutionOptions(OptionsFactory options)
        {
            options.Check("Please select the desired project types:")
                .When(() => _options.IsNewProject)
                .Choice("Console", "console", () => _options.UseProjectType(ProjectTypes.Console))
                .Choice("Web", "web", () => _options.UseProjectType(ProjectTypes.Web));
        }

        // This is a selectlist
        private void ConfigureExistingOptions(OptionsFactory options)
        {
            options.List("Please select the desired project type:")
                .When(() => !_options.IsNewProject)
                .Choice("Console", "console", () => _options.UseProjectType(ProjectTypes.Console))
                .Choice("Web", "web", () => _options.UseProjectType(ProjectTypes.Web));

            options.Input("Please enter the name of the sub project: ", s => _options.ProjectName = s)
                .When(() => !_options.IsNewProject);
        }

        private void ConfigureLibraries(OptionsFactory options)
        {
            options.Check("Please select the desired libraries:")
                .Choice("AutoMapper", "automapper", () => _options.UseComponent(ComponentTypes.Automapper))
                .Choice("Entity Framework", "et", () => _options.UseComponent(ComponentTypes.EntityFramework));
        }

        protected override void ConfigureGenerator(IScaffoldBuilder builder)
        {
        }
    }

    public enum ProjectTypes
    {
        Console,
        Web
    }

    public enum ComponentTypes
    {
        Automapper,
        DataAccess,
        EntityFramework
    }
}