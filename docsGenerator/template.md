# Bootstrap file

## Overview of Packages and Modules
Bootstrap files are written as yaml files. For details have a look at the [Wikipedia article][1].
To validate a config file you can run the ```validate``` subcommand followed by a filename, e.g.:

	dotnet run validate myconfig.yaml

A config file consists of packages which contain modules. A package is used to describe a unit of work. E.g., you might want to install Visual Studio Code. This consists of multiple steps:

1. Import the repository key from Microsoft.
2. Add the Microsoft repository.
3. Update the package manager index.
4. Install the VS Code package.
5. Install some extensions for VS Code.

Each of these steps is represented by a module. The modules are run in the order in which they are listed in their parent package. Default settings for packages/modules make sure that a package fails if any module fails (execution is aborted as soon as a module fails) and that the bootstrap process continues if a package fails.

After running (successful or not) a bootstrap process two files are written to the current working directory:

1. bootstrap.success
2. bootstrap.error

The first contains a list of all packages that finished successfully and the second contains the names of the packages that failed. If you run the boostrap process again (with these files present) any package that is listed in ```bootstrap.success``` will *not* be run again. This helps you debug your config and trying again.


## Variables
For more complex operations you can make use of variables. These can be defined globally on the configuration root level or on a package level. To use the variable ```MyVariable```  you simply use ```$MyVariable``` as a placeholder for a property. E.g.:

	- !!FolderCreation
		Foldernames:
		- $MyFirstFolder
		- $MySecondFolder
		- /opt/android_studio	

This will replace the string ```$MyFirstFolder``` with whatever is stored in the given variable. Note that any new definition of a variable will overwrite its previous definition (except when overring global variables in a package).

### Default variables
There are a number of default variables:

* _homedir_ contains to the home directory of the user
* _~_ will be expanded to _$homedir_
* _username_ contains the name of the current user

## Packages

A package has the following propertes:

#### Name (string)
A name should be given to every package and needs to be unique. The name is used for the progress output and to validate the requirements of a package (see below).

#### Description (string)
A description is an optional field that is used to describe what the package does.

#### IsCritical (bool)
If a package is marked as critical the bootstrap process will abort if the package fails. This is useful if for packages that install base packages or perform system updates.

#### IgnoreAlreadySolved (bool)
If this is set to true the package will run again even if it completed successfully in a previous run.

#### Modules (array of modules)
Contains all the modules necessary to complete this package.

#### Requires (array of string)
List of packages (identified by name) that need to run before this package will be run. This is useful if you want to make sure that base packages and system updates have been applied before doing any other work.

#### Variables (associative array)
List of variables which are defined for this package.

## Modules
{{
{
	ModulePrefix: "### ",
	ModulePropertyPrefix: "*",
	ModulePropertySuffix: "*"
}
}}

[1]: https://en.wikipedia.org/wiki/YAML
