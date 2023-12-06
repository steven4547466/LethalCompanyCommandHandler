CommandHandler is a mod strictly for developers to add commands in a way that is compatible with other mods.

If you are a developer, you need only download the dll from [releases](https://github.com/steven4547466/LethalCompanyCommandHandler/releases), reference it in your project then do:

```cs
CommandHandler.CommandHandler.RegisterCommand("command", (string[] args) =>
{
    // Handler
});

// OR with aliases:

CommandHandler.CommandHandler.RegisterCommand("command", new List<string>() { "alias" }, (string[] args) =>
{
    // Handler
});
```

And add it as a dependency to your mod in the manifest.