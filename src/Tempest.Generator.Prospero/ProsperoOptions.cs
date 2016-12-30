using Tempest.Generator.Prospero.Extensions;

namespace Tempest.Generator.Prospero
{
    public class ProsperoOptions
    {
        public string SolutionName { get; set; }

        public bool HasProjectType(ProjectTypes type)
        {
            return ProjectTypes.Has(type);
        }

        public bool HasComponent(ComponentTypes type)
        {
            return Components.Has(type);
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

        public ProjectTypes ProjectTypes { get; set; }
        public ComponentTypes Components { get; set; }
        public string ProjectName { get; set; }
        public bool IsNewProject { get; set; }
    }
}