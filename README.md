Overview
========
This project is meant to help you bootstrap your Fedora (for now, other distro could be implemented easily) installation. Instead of writing a bash script with tons of commands you simply write a yaml file.

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