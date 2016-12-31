using FluentModelBuilder.Alterations;
using FluentModelBuilder.Builder;
using FluentModelBuilder.Relational.Conventions;

namespace ProsperoTemplate.Core.Infrastructure.EntityFramework
{
    public class ProsperoTemplateModelBuilderAlteration : IAutoModelBuilderAlteration
    {
        public void Alter(AutoModelBuilder builder)
        {
            builder.UseConvention<PluralizingTableNameGeneratingConvention>();
        }
    }
}
