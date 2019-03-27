This package is supposed to replace FluentMigrator, DbUp or RoundhousE or whatever you're using. All of these frameworks made it hard for me in some way so I decided to roll my own. 

#### Problems with FluentMigrator
* I don't see the point in not writing sql in sql.
* I don't understand how to update data when changing my schema
* I don't see the point in down-scripts.
* I don't see how you easily can track changes made to a specific stored procedure, view, trigger, function, whatever in your git repo.

#### Problems with RoundhousE
* I don't understand how to "install" it. 
* But that doesn't matter because it doesn't say it works with any sql server above 2008 anyways.
* Also, @carl-berg wants it to work with embedded resources.

#### Problems with DbUp
* Promising, but I want to journal my EVERYTIME-scripts as well. The fact that they have changed is important, and I need to see which changes were part of a release.
* I found something called Improving.DbUp which kinda seemed promising as well, but it wasn't net core compatible and also a piece of junk, not letting me decide for myself where my connectionstrings are.
* I tried to modify DbUp by creating my own implementations of scriptproviders and journalingstuff, and realized I'm changing so much stuff it didn't look like DbUp anymore and it was not even exactly as I'd like it anyway.

Soooo, I felt that I'm done creating garbage migration scripts that are not working the way I want. Welcome to Galactic Waste Management.

# Galactic Waste Management

## Quickstart
1. Create a new dot net core console app. 
2. Run *Install-Package GalacticWasteManagement*
3. Paste this in your Program.cs
```csharp
static async Task Main(string[] args)
{
    var connectionString = "your-connection-string";
    var wasteManager = GalacticWasteManager.Create<Program>(connectionString);
    await wasteManager.Update("GreenField");
    Console.ReadLine();
}
```
4. Make sure you're building C# 7.1 or higher for the async Main to work. *Right click on your project -> Properties -> Build -> Advanced... -> Language Version* 

5. Create a folder called Scripts in your project
6. Create three folders inside the newly created Scripts folder, named *RunIfChanged*, *Seed* and *vNext*
7. Create a sql file that creates some tables or something and put it into the vNext folder.
8. Set build action of that file to *Embedded resource*.
9. Done! Run!

## Creating a new migrations release
*I'll fill this out in the future, promise*

## Configuration

### Environment

#### Logging
GalacticWasteManager continuosly tell you what it is doing through an *ILogger*. Change through *wasteManager.Logger*. Default is *ConsoleLogger*. Implement *ILogger* interface to create your own. There are no other implementations than *ConsoleLogger* shipped with GalacticWasteManagement.

#### Output
GalacticWasteManager can output all relevant sql that was run after a migration is complete. Change through *wasteManager.Output*. Default is *NullOutput*. Implement *IOutput* interface to create your own. GalacticWasteManager ships with a *FileOuput* that you can use. 

### Project Settings
Project settings are supposed to stay basically the same through your whole project. You decide what they should be once, and then you leave them at that. They do not change when switching environments. These settings can only be set through an overload of the  *GalacticWasteManager.Create* method.

#### Versioning 
Default versioning is *Major.Minor*. Migration scripts related to a release are expected to live in a folder with the a version conforming to this standard as its name (i.e. 4.2, or 12.3), under the *Scripts.Migration* folder. Scripts are not versioned _below_ *minor*. Scripts with the same version are executed in alphabetical order. In the schema journal, you can look at the *Id* column if you'd want to know which order scripts were run.

You can change the versioning strategy by creating your own implementation of *IMigrationVersioning*.

#### ScriptProviders
By default, scripts are read from *.sql*-files located in two different locations. The first location is part of the GalacticWastePackage itself, and you will never see them. These are scripts for creating the database, dropping schema and creating the SchemaVersionJournal table. The other scripts are by default read from the assembly that contains the class you use as type parameter in  *GalacticWasteManager.Create*. You can create and provide your own *IScriptProvider*s if you'd like. If you still want to incorporate the defaults, this is what they look like:

```csharp
new BuiltInScriptsScriptProvider(),
new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(T)), "Scripts")
```

A *IScriptProvider* can return any implementation of *IScript*. There are some rules regarding how *IScript*s should be implemented. More on that further down.

### Migration Settings
These settings are typically provided through *wasteManager.Update()* each time you do a migration.

#### Mode (required)
Determines which strategy to use when migrating. Currently, GalacticWasteManager comes with *GreenField* and *LiveField* modes. *BrownField* is in the pipeline. You can create your own migration strategies. Implement *IMigration* or subclass *MigrationBase* and register in *GalacticWasteManager.MigrationFactories*. ~(Note to self: Why can't you just supply it directly in the Update-method?)~ (Now you can supply a factory func.) More on modes further down. 

#### Clean
Default *false*. Instructs migrator to clean schema and start anew. There are other situations when schema can be cleaned even though this settings is *false*, and there are also situations where cleaning schema is not appropriate. More on that further down.

#### ScriptVariables
Will by default contain your database name (as provided in connectionstring) on key *DbName*. Otherwise empty. Any *$variable$* in your scripts will be replaced with matching values in the scriptVariables dictionary. Avoid using this unless you're okay with coupling your sql-scripts with GalacticWasteManagement. 

## Modes
Migrating database schema and content works differently depending on mode.

### GreenField

One of the modes of Galactic Waste Management is GreenField. This mode is used when you are developing a database from scratch.
In GreenField, scripts are executed like this.

#### Create
Script of ScriptType *Create* are run if a database does not exist. There is typically only one script here and it creates the database.
//TODO: Let user configure how to check if database exist.

#### Initialize
Script of ScriptType *Initialize* are run if the database was just created or if it was just cleaned (more on that later).
*y default, a script to create the table holding schema version information can be found here. That table is called *SchemaVersionJournal*
//TODO: Let user configurehow to check if initialize script should run.

#### vNext
Next, all scripts with ScriptType *vNext* will be run. At this point, *vNext* should contain all scripts for the first version. In GreenField, these scripts are typically CREATE TABLE-scripts for schema creation, and INSERT-scripts for content creation.
Everytime the GreenField migration runs, it will look for new, changed or removed scripts in this folder. If there are any scripts removed or changed, the database will be cleaned (more on that later) before all vNext scripts are run.
If there is only new scripts found, they will be run without cleaning the database first. If all scripts are unchanged, nothing will be done here.

#### RunIfChanged
These scripts are run if they are new or changed. This folder typically contain stored procedures, views, triggers functions and the like. These scripts must be idempotent.
During GreenField development you can add and remove scripts from here as you like, since it easy to force a new clean migration.

#### Seed
*Seed* type scripts are only run in GreenField. They typically contain data that aids the developers by creating some fake data.
Changes or removal of any Seed scripts will trigger a clean migration, to ensure the data matches the schema. 
If the folder only contain Added files though, they will be run without cleaning and migrating the schema again.
These are the last scripts executed before the migration is complete.

#### Drop
In any instance where the schema has to be cleaned (changed or removed vNext or Seed scripts), all scripts of type *Drop* will be run.

All scripts in GreenField development will have version 'vNext' or 'local' (only seed scripts) in the SchemaVersionJournal table.
Create/Drop/Initialize-scripts are not journaled.

### LiveField

LiveField mode is supposed to be used when running against a production database. Before this happens, scripts should have been moved from vNext to Migrations/{version}.
LiveField mode works like Green Field with a few exceptions.

Seed-scripts, Drop-scripts and vNext-scripts are not run.
Create and FirstRun will be run if database does not exist.
Migration-scripts are run instead of vNext-scripts. All migrationscripts not run, that have a higher version than current highest version will be run. 
Any other scripts in the Migration folder, changed, added or removed, will generate warnings.
RunIfChanged-scripts will be run as in GreenField.
