CommandHandler is a mod strictly for developers to add commands in a way that is compatible with other mods.

If you are a user, the mod can be downloaded from [here](https://thunderstore.io/c/lethal-company/p/steven4547466/CommandHandler/). The default prefix is `/`, but you can change that in r2modman config.

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

`RegisterCommand` will return `false` if the command or any alias is already used and will **NOT** register the command for compatibility reasons.

There is also an `UnregisterCommand` which you can use to unregister a command.

And add it as a dependency to your mod in the manifest.