var target = Argument("target", "Default");

Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore();
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild("./src/**/project.json");
        DotNetCoreBuild("./test/**/project.json");
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
    {
        var directories = System.IO.Directory.GetDirectories("./test/");
        foreach(var directory in directories)
            DotNetCoreTest(directory);
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
    {
		var publishRoot = "./artifacts/ProsperoTemplate.Web/";

        var settings = new DotNetCorePublishSettings
        {
            Framework = "netcoreapp1.0",
            Configuration = "Release",
            OutputDirectory = publishRoot
        };   
        
        DotNetCorePublish("./src/ProsperoTemplate.Web", settings);
        Zip(publishRoot, "./artifacts/ProsperoTemplate.Web.zip");
    });

Task("Upload-AppVeyor-Artifacts")
    .IsDependentOn("Package")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
	{
		var artifact = MakeAbsolute(File(@"./artifacts/ProsperoTemplate.Application.zip"));
		AppVeyor.AddInformationalMessage("Uploading artifacts");
		AppVeyor.UploadArtifact(artifact, settings => settings
			.SetArtifactType(AppVeyorUploadArtifactType.WebDeployPackage)
		);
	});

Task("Publish")
    .IsDependentOn("Upload-AppVeyor-Artifacts")
    .Does(() => {
        Information("Published");
    });

	
Task("Default")
    .IsDependentOn("Package")
    .Does(() =>
    {
		Information("Packaged"); 
    });

RunTarget(target);