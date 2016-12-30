using System;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Prospero.Extensions.EntityFramework.Conventions.Migrations;

namespace ProsperoTemplate.Web.Core.Factories
{
    public class ProsperoTemplateDbContextFactory : GenericConventionDbContextFactory<Startup>
    {
    }
}