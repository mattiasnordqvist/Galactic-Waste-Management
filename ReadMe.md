This package is supposed to replace FluentMigrator, DbUp or RoundhousE or whatever you're using. All of these frameworks made it hard for me in some way so I decided to roll my own. 

#### Problems with FluentMigrator
* I don't see the point in not writing sql in sql.
* I don't understand how to update data when changing my schema
* I don't see the point in down-scripts.
* Even though I wrote my own plain-sql-script-loader for FluentMigrator, I don't see how you easily can track changes made to a specific stored procedure, view, trigger, function, whatever in your git repo.

#### Problems with RoundhousE
* I don't understand how to "install" it. 
* But that doesn't matter because it doesn't say it works with any sql server above 2008 anyways.
* Also, @carl-berg wants it to work with embedded resources.

#### Problems with DbUp
* Promising, but I want to journal my EVERYTIME-scripts as well. The fact that they have changed is important, and I need to see which changes were part of a release.
* I found something called Improving.DbUp which kinda seemed promising as well, but it wasn't net core compatible and also a piece of junk not letting me decide where my connectionstrings come from.
* I tried to modify DbUp by creating my own implementations of scriptproviders and journalingstuff, and realized I'm changing so much stuff it didn't look like DbUp anymore and it was not even exactly as I'd like it anyway.

Soooo, I felt that I'm done creating garbage migration scripts that are not working the way I want... Welcome to Galactic Waste Management.

# Galactic Waste Management

  * [Quickstart](#quickstart)
  * [Configuration](#configuration)
    + [Environment](#environment)
      - [Logging](#logging)
      - [Output](#output)
      - [Parameters](#parameters)
        * [ConsoleInput](#consoleinput)
        * [ConfigInput](#configinput)
    + [Project Settings](#project-settings)
      - [Versioning](#versioning)
      - [ScriptProviders](#scriptproviders)
    + [Migration Settings](#migration-settings)
      - [Mode (required)](#mode-required)
      - [Parameters](#parameters-1)
      - [ScriptVariables](#scriptvariables)
  * [Implement you own *IScript*s, *IScriptProvider*s and *IScriptType*s](#implement-you-own-iscripts-iscriptproviders-and-iscripttypes)
  * [Implement your own Mode](#implement-your-own-mode)
  * [Implement your own VersioningStrategy](#implement-your-own-versioningstrategy)
  * [How to do minor tweaks to the defaults (like drop/create/initialize)](#how-to-do-minor-tweaks-to-the-defaults-like-dropcreateinitialize)
  * [Creating a new migrations release (going to production and brownfield)](#creating-a-new-migrations-release-going-to-production-and-brownfield)
  * [Typical asp.net core usage](#typical-aspnet-core-usage)
  * [Captain Data and how it can aid in insert and seed scripts](#captain-data-and-how-it-can-aid-in-insert-and-seed-scripts)
  * [Details on built-in modes](#details-on-built-in-modes)
    + [GreenField](#greenfield)
      - [Initialize](#initialize)
      - [vNext](#vnext)
      - [RunIfChanged](#runifchanged)
      - [Seed](#seed)
      - [Drop](#drop)
    + [LiveField](#livefield)
    + [BrownField](#brownfield)

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

## Configuration

### Environment

#### Logging
GalacticWasteManager continuosly tell you what it is doing through an *ILogger*. Change through *wasteManager.Logger*. Default is *ConsoleLogger*. Implement *ILogger* interface to create your own. There are no other implementations than *ConsoleLogger* shipped with GalacticWasteManagement.

#### Output
GalacticWasteManager can output all relevant sql that was run after a migration is complete. Change through *wasteManager.Output*. Default is *NullOutput*. Implement *IOutput* interface to create your own. GalacticWasteManager ships with a *FileOuput* that you can use. 

#### Parameters
You can configure how parameters are passed to the different modes (see below about modes). By default, parameters are supplied through
Console window or through *Update*-method. Parameters passed through *Update* will not need to be passed through Console.
Input is changed through .Parameters.SetInput(). *ConsoleInput* and *ConfigInput* is provided by Galactic Waste Manager. *MainArgsInput* is probably coming someday.

There are also some parameters used by Galactic Waste Manager regardless of mode. See below how they can be used.
* skip-create, optional boolean, default false. If true, will assume database exist and not attempt creating one. Must be true if you do not have access to master database.
* ensure-db-exists, optional boolean, default false. If skip-create is true, you can set this to true as well to halt execution early if database happens to not exist.

##### ConsoleInput
Since *ConsoleInput* uses the Console, it is not a very good input type when running migrations on server etc. In those cases, all parameters should be provided through the *Update*-method or a *ConfigInput*.

##### ConfigInput
Retrieves parameters from a ConfigSection. Typical usage might look like this:

```csharp
// appsettings.json
{
 "Migration": {
    "Mode": "GreenField",
    "ConnectionString": "Data Source=<instanceName>;Initial Catalog=<DbName>;Integrated Security=SSPI;",

    "GreenField": {
      "clean": true
    },
    "BrownField": {
      "clean": false,
      "source": "C:\\Program Files\\Microsoft SQL Server\\MSSQL13.MSSQLSERVER\\MSSQL\\Backup\\MyBackupFile.bak"
    },
    "LiveField": {
      "skip-create": true,
      "ensure-db-exists": true
    },
  }
}

// your code
var config = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: false)
   .Build();
var migrationConfig = config.GetSection("Migration");
var mode = migrationConfig["Mode"];

var input = new ConfigInput(migrationConfig.GetSection(mode));
var wasteManager = GalacticWasteManager.Create<Program>(migrationConfig["ConnectionString"]);
wasteManager.Parameters.SetInput(input);
await wasteManager.Update(mode);
```

### Project Settings
Project settings are supposed to stay basically the same through your whole project. You decide what they should be once, and then you leave them at that. They do not change when switching environments. Changing them can wreak havoc to your project if you don't know what you're doing. These settings can only be set through an overload of the  *GalacticWasteManager.Create* method.

#### Versioning 
Default versioning is *Major.Minor*. Migration scripts related to a release are expected to live in a folder with a name conforming to this standard (i.e. 4.2, or 12.3), under the *Scripts.Migration* folder. Scripts are not versioned _below_ *minor*. Scripts with the same version are executed in alphabetical order. In the schema journal, you can look at the *Id* or *Applied* column if you want to know which order scripts were run.

You can change the versioning strategy by creating your own implementation of *IMigrationVersioning*.

#### ScriptProviders
By default, scripts are read from *.sql*-files located in two different locations. The first location is part of the GalacticWastep package itself, and you will never see them. These are scripts for creating the database, dropping schema and creating the SchemaVersionJournal table. The other scripts are by default read from the assembly that contains the class you use as type parameter in  *GalacticWasteManager.Create*. They are expected to live in folders named *Scripts/vNext*, *Scripts/RunIfChanged*, *Scripts/Migration* and *Scripts/Seed* and they are expected to be embedded resources. You can create and provide your own *IScriptProvider*s if you'd like. If you still want to incorporate the defaults, this is what they look like:

```csharp
new BuiltInScriptsScriptProvider(),
new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(T)), "Scripts")
```

A *IScriptProvider* can return any implementation of *IScript*. There are some rules regarding how *IScript*s should be implemented. More on that further down.

### Migration Settings
These settings are typically provided through *wasteManager.Update()* each time you do a migration.

#### Mode (required)
Determines which strategy to use when migrating. Currently, GalacticWasteManager comes with *GreenField*, *LiveField* and *BrownField* modes. *GreenField* is for when you are developing on a brand new database. *LiveField* is for your production environment, no matter if the database is new or not. *BrownField* is for developing after your first release. You can create your own migration strategies as well. Implement *IMigration* or subclass *MigrationBase* and register in *GalacticWasteManager.MigrationFactories*, or you can supply it directly to the Update-method if you don't want it easily configurable. Details on the different modes and how you can implement your own further down. 

#### Parameters
Some modes require additional parameters to be set. See each mode for specifications. Parameters are usually supplied through a given input, but their values can be predefined through the *Update*-method. If a required parameter is not set this way, Galactic Waste Manager will ask the current Input to _retrieve_ the parameter.

#### ScriptVariables
Will by default contain your database name (as provided in connectionstring) on key *DbName*. Otherwise empty. Any *$variable$* in your scripts will be replaced with matching values in the scriptVariables dictionary. Avoid using this unless you're okay with coupling your sql-scripts with GalacticWasteManagement. 

## Implement you own *IScript*s, *IScriptProvider*s and *IScriptType*s
// TODO Document

## Implement your own Mode
// TODO Document

## Implement your own VersioningStrategy
// TODO Document

## How to do minor tweaks to the defaults (like drop/create/initialize)
// TODO Document

## Creating a new migrations release (going to production and brownfield)
// TODO

## Typical asp.net core usage
This solution suggested below gives you a seperate console app for manual migrations, and an automatic database migrator on startup of your webhost.

Create a new console app project. Install GalacticWasteManager from nuget. Drop these two files in there. Replace "GreenField" with your desired mode, or pass it in as argument from the main method, or maybe read it from the config file.

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        var connectionString = config.GetConnectionString(Environment.MachineName);
        await new DatabaseMigrator(connectionString).Migrate("GreenField");
        Console.ReadLine();
    }
}

public class DatabaseMigrator
{
    private readonly string _connectionString;
    private readonly IOutput _output;
    
    public DatabaseMigrator(string connectionString, IOutput output = null)
    {
        _connectionString = connectionString;
        _output = output ?? new NullOutput();
    }

    public async Task Migrate(string mode)
    {
        var wasteManager = GalacticWasteManager.Create(
            new DefaultProjectSettings<Program>(),
            _connectionString);
        await wasteManager.Update(mode);
    }
}
```
Then, in your aspnet core project. Make your Main method look something like this. Replace "GreenField" with your desired mode here as well.

```csharp
 public class Program
{
    public static async Task Main(string[] args)
    {
        var webHost = CreateWebHostBuilder(args).Build();
        using (var scope = webHost.Services.CreateScope())
        {
            var migrator = scope.ServiceProvider.GetRequiredService<DatabaseMigrator>();
            await migrator.Migrate("GreenField");
        }

        await webHost.RunAsync();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}

```

There you go!

## Captain Data and how it can aid in insert and seed scripts
// TODO Document

## Details on built-in modes

### GreenField

One of the modes of Galactic Waste Management is GreenField. This mode is used when you are developing a database from scratch.
In GreenField, scripts are executed like this.

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
Create and Initialize might be run, depending on if the database exist and if the SchemaVersionJournal table exist.
Migration-scripts are run instead of vNext-scripts. All migrationscripts not run, that have a higher version than current highest version will be run. (and they should be run in version-order, then alphabetically).
If there would be any other scripts in the Migration folder, changed, added or removed, an error will be raised, since older versions should be cemented.
RunIfChanged-scripts will be run as in GreenField. However, a new situation in LiveField is when you actually remove a RunIfChanged-script. If you do so, you should remove it AND add an ordinary *Migration*-script as well. This migration-script should contain the *drop*-part of the removed *RunIfChanged*-script. Removed *RunIfChanged*-scripts will be journaled on new rows with type 'Deprecated', using the same name and hash as the original script, but with a new version number (the version currently migrating to). If you change your mind and want to re-add the *RunIfChanged*-script again in a future version, you can do so without problems. :+1:

### BrownField

When you continue development after your first release, you want to run in BrownField mode. Brownfield accepts a Source-parameter, which should point to a .bak-file from which you can restore your database. This backup should've been taken from the live database at some point.
BrownField works pretty much like GreenField, but if Source is set, it will *Restore* database instead of *Clean* it. After restore or clean, all Migration-, vNext-, RunfIfchanged- and Seed-scripts will be evaluated for execution.
New migration scripts should again go in the *vNext* folder. Scripts in the *Migration*-folder are considered *cemented* and you are not allowed to change them. Changing them will result in an error. THESE 
