using FluentModelBuilder.Alterations;
using FluentModelBuilder.Builder;
using FluentModelBuilder.Relational.Conventions;

namespace ProsperoTemplate.Core
{
    public class ProsperoTemplateModelBuilderAlteration : IAutoModelBuilderAlteration
    {
        public void Alter(AutoModelBuilder builder)
        {
            builder.UseConvention<PluralizingTableNameGeneratingConvention>();
        }
    }
}
