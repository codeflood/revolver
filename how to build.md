# How to build Revolver #

Revolver builds against and deploys to a local Sitecore instance on your machine. The Sitecore instance can be located anywhere on the local machine. As Revolver includes build configurations for several different Sitecore versions it is not possible to setup Sitecore in the same folder as the Revolver project.

## Building the Project ##

Perform the following before opening the solution in Visual Studio.

1. Ensure you have the desired Sitecore version already installed and working on the local machine.
1. Copy the `src\deploy.targets.example` file to `src\deploy.targets`.
	1. The `src\deploy.targets` file is a local only file. It's already been added to the `.gitignore` file.
1. Edit the `src\deploy.targets` file to update the paths to the Sitecore instances.
1. For each of the Sitecore versions you want to build against, update the `SitecorePath` property to reference the web root of the Sitecore instance.
	1. Each `SitecorePath` property includes a `Condition` defining which Sitecore version the property is used for.
1. Open the solution in Visual Studio and select the appropriate build configuration.
	1. The numbers of a build configuration refers to the Sitecore version the project will be built for.
1. Rebuild the solution
1. The project will automatically deploy the Revolver files to the Sitecore instance you build against.

## Restore Sitecore Items ##

In addition to building the project and deploying the files, you must also restore the Revolver items.

1. Copy the `src\Data\serialization` folder to the `data` folder of the Sitecore instance.
1. Navigate to the `/sitecore/admin/serialization.aspx` utility page
1. In the 'Select databases' section select the `core` database.
1. Click the `Update {core} databases` button
