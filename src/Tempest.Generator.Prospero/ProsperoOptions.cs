using System.Collections.Generic;
using Tempest.Generator.Prospero.Extensions;

namespace Tempest.Generator.Prospero
{
    public class ProsperoOptions
    {
        public string SolutionName { get; set; }

        public bool HasProjectType(ProjectTypes type)
        {
            return ProjectTypes.Contains(type);
        }

        public bool HasComponent(ComponentTypes type)
        {
            return Components.Contains(type);
        }

        public ProsperoOptions UseProjectType(ProjectTypes type)
        {
            ProjectTypes.Add(type);
            return this;
        }

        public ProsperoOptions UseComponent(ComponentTypes type)
        {
            Components.Add(type);
            return this;
        }

        public List<ProjectTypes> ProjectTypes { get; } = new List<ProjectTypes>();
        public List<ComponentTypes> Components { get; } = new List<ComponentTypes>();
        public string ProjectName { get; set; }
        public bool IsNewProject { get; set; }
    }
}