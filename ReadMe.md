This package is supposed to replace FluentMigrator, DbUp or RoundhousE or whatever you're using. All of these frameworks made it hard for me in some way so I decided to roll my own. 

#### Problems with FluentMigrator
* I don't see the point in not writing sql in sql.
* I don't understand how to update data when changing my schema
* I don't see the point in down-scripts.
* I don't see how you easily can track changes made to a specific stored procedure, view, trigger, function, whatever in your git repo.

#### Problems with RoundhousE
* I don't understand how to "install" it. 
* But that doesn't matter because it doesn't say it works with any sql server above 2008 anyways.

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

## Modes
Migrating database schema and content is done differently depending on mode.

### GreenField

One of the modes of Galactic Waste Management is GreenField. This mode is used when you are developing a database from scratch.
In GreenField, scripts are executed like this.

### Create
All scripts in the Create-folder will be executed if a database does not already exist. There is typically only one script here and it creates the database.

### FirstRun
All scripts in the FirstRun-folder are run if the database was just created or if it was just cleaned (more on that later).
By default, a script to create the table holding schema version information can be found here.

### vNext
Next, all scripts in vNext will be run. At this point, vNext contains all scripts for the first version. In Green Field, this folder typically contain CREATE TABLE-scripts for schema creation, and INSERT-scripts for content creation.
Everytime the DatabaseUpdater runs, it will look for new, changed or removed scripts in this folder. If there are any script removed or changed, the database will be cleaned (more on that later) before all vNext scripts are run.
If there is only new scripts found, the will be run without cleaning the database first. If all scripts are unchanged, nothing will be done here.

### RunIfChanged
This folder contain scripts that will be run if they are new or changed. This folder typically contain stored procedures, views, triggers functions and the like. These scripts should typically be idempotent.
During Green Field development you can add and remove scripts from here as you like, since it easy to force a new clean migration.

### Seed
Scripts in this folder are only run in Green Field. They typically contain data that aids the developers by creating some fake data.
Changes or removal of any Seed scripts will trigger a clean migration, to ensure the data matches the schema. 
If the folder only contain Added files though, they will be run without cleaning and migrating the schema again.

### Drop
In any instance where the schema has to be cleaned (changed or removed vNext or Seed scripts), the scripts in this folder will be run.


All scripts in GreenField development will have version 'vNext' or 'local' (only seed scripts) in the SchemaVersionJournal table.
Create/Drop/FirstRun-scripts are not journaled.

### Live Field

LiveField mode is supposed to be used when running against a production database. Before this happens, scripts should have been moved from vNext to Migrations/{version}.
LiveField mode works like Green Field with a few exceptions.

Seed-scripts, Drop-scripts and vNext-scripts are not run.
Create and FirstRun will be run if database does not exist.
Migration-scripts are run instead of vNext-scripts. All migrationscripts not run, that have a higher version than current highest version will be run. 
Any other scripts in the Migration folder, changed, added or removed, will generate warnings.
RunIfChanged-scripts will be run as in GreenField.
