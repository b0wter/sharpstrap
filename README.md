Overview
========
This project is meant to help you bootstrap your Fedora (for now, other distro could be implemented easily) installation. Instead of writing a bash script with tons of commands you simply write a yaml file.

![build status](https://dev.azure.com/b0wter/SharpStrap/_apis/build/status/Generate%20Releases)(https://dev.azure.com/b0wter/SharpStrap)

Packages
--------
A package is a collection of modules that for a meaningful action together. For instance: installing VS code is done like this:

    - Name: vs_code
      Description: Imports the microsoft repo key and installs vs code.
      Requires: 
      - base_package_install_and_update
      Modules:
      - !!KeyImport
        RequiresElevation: true
        Url: https://packages.microsoft.com/keys/microsoft.asc
      - !!FileCopy
        RequiresElevation: true
        Filenames:
        - $bootstrapdir/vscode.repo
        Target: /etc/yum.repos.d/
      - !!PackageUpdate
        RequiresElevation: true
      - !!PackageInstall
        RequiresElevation: true
        PackageNames:
        - code
      - !!GenericShell
        Description: Installs vs code addons from the specified file.
        Command: $bootstrapdir/install_code_addons
        Arguments: $bootstrapdir/code_addons.txt


Module
------
A module is a single action, like updating the installed packages or install new packages. A list of predefined modules will be added in the near future but there is a fallback generic shell module that allows for the execution of any shell command.

Tutorial
========

Variables
---------
You can define variables in different ways:

 * use the ```ReadVariable```-Module to read a variable from the command line (user input)
 * use the ```ShellEvaluate```-Module to store the results of a command in a variable
 * define a global variable by adding an entry to the ```GlobalVariables``` property of the root config object
 * define a package-wide variable by adding an entry to the ```Variables``` property of a package.

 To use a variable you simple prefix it the a ```$```. For example:

    - !!ReadVariable
        VariableName: hostname

reads user input to the ```hostname``` variable. Use it like this:

    - !!GenericShell
        Command: ssh-keygen 
        Arguments: -t ed25519 -f id_ed25519_$hostname
        WorkingDirectory: $homedir/.ssh/

A couple of variables are predefined:

 * homedir - stores the home directory of the current user
 * username - stores the name of the current user

Packages
--------
Packages are collection of modules to perform a single task like installing importing a new repository and installing software from it. Packages have the following properties:

 * ```Name``` - a name should be set for every package; this allows it to be referenced by other packages and will be used for user output
 * ```Description``` - an optional description of what this package does
 * ```IsCritical``` - marks this package as critical meaning that the bootstrap process will stop if this package fails
 * ```IgnoreAlreadySolved``` - a previously run pacakge will not be run again (by default); setting this to true will make sure this package runs always
 * ```Modules``` (Array) - contains a list of modules that need to be run for this package
 * ```Requires``` (Array) - contains a list of package names that need to run before this package can run
 * ```Variables``` (Ass. Array) - contains a list of variables for this package

Modules
-------
As of now all modules are run as shell commands. This is done to allow each command to have its priviledge level set. Individual commands can be elevated using the ```RequiresElevation``` property.

### Properties for all modules ###
Every module has at least the following properties:

 * ```Id``` - name for this module, this will 
 * ```AllowError``` - if this is set to true the package will continue to run even if this module fails
 * ```SkipVariableReplacement``` - if this is set to true variables will not be used for modules

 ShellModules (which almost all modules are based on) have more properties:

 * ```RequiresElevation``` - runs this module using ```sudo```
 * ```WorkingDirectory``` - sets the working directory for this module
