using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;
using Zeekerss.Core.Singletons;

namespace CommandHandler
{
    [BepInPlugin("steven4547466.CommandHandler", "Command Handler", "1.0.0")]
    public class CommandHandler : BaseUnityPlugin
    {
        private static Harmony Harmony { get; set; }

        internal static CommandHandler Singleton { get; private set; }

        internal static ConfigEntry<string> CommandPrefix;

        internal static Dictionary<string, Action<string[]>> CommandHandlers = new Dictionary<string, Action<string[]>>();

        internal static Dictionary<string, List<string>> CommandAliases = new Dictionary<string, List<string>>();

        void Awake()
        {
            Singleton = this;

            CommandPrefix = Config.Bind("General", "Prefix", "/", "Command prefix");

            Harmony = new Harmony($"steven4547466.CommandHandler-{DateTime.Now.Ticks}");

            Harmony.PatchAll();
        }
        
        public static bool RegisterCommand(string command, Action<string[]> handler)
        {
            if (CommandHandlers.ContainsKey(command)) return false;

            CommandHandlers.Add(command, handler);

            return true;
        }

        public static bool RegisterCommand(string command, List<string> aliases, Action<string[]> handler)
        {
            if (GetCommandHandler(command) != null) return false;

            foreach (string alias in aliases)
            {
                if (GetCommandHandler(alias) != null) return false;
            }

            CommandHandlers.Add(command, handler);

            CommandAliases.Add(command, aliases);

            return true;
        }

        public static bool UnregisterCommand(string command)
        {
            CommandAliases.Remove(command);
            return CommandHandlers.Remove(command);
        }

        public static Action<string[]> GetCommandHandler(string command)
        {
            if (CommandHandlers.TryGetValue(command, out var handler)) return handler;

            foreach (var alias in CommandAliases)
            {
                if (alias.Value.Contains(command)) return CommandHandlers[alias.Key];
            }

            return null;
        }

        public static bool TryGetCommandHandler(string command, out Action<string[]> handler)
        {
            handler = GetCommandHandler(command);
            return handler != null;
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
        static class SubmitChatPatch
        {
            private static bool HandleMessage(HUDManager manager)
            {
                string message = manager.chatTextField.text;

                if (!message.IsNullOrWhiteSpace() && message.StartsWith(CommandPrefix.Value))
                {
                    string[] split = message.Split(' ');

                    string command = split[0].Substring(CommandPrefix.Value.Length);

                    if (TryGetCommandHandler(command, out var handler))
                    {
                        string[] arguments = split.Skip(1).ToArray();
                        try
                        {
                            handler.Invoke(arguments);
                        } 
                        catch (Exception ex)
                        {
                            Singleton.Logger.LogError($"Error handling command: {command}");
                            Singleton.Logger.LogError(ex);
                        }
                    }

                    manager.localPlayer.isTypingChat = false;
                    manager.chatTextField.text = "";
                    EventSystem.current.SetSelectedGameObject(null);
                    manager.typingIndicator.enabled = false;

                    return true;
                }

                return false;
            }

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> newInstructions = new List<CodeInstruction>(instructions);

                Label returnLabel = generator.DefineLabel();

                newInstructions[newInstructions.Count - 1].labels.Add(returnLabel);

                int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldfld && 
                    (FieldInfo)i.operand == AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.isPlayerDead))) - 2;

                newInstructions.InsertRange(index, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SubmitChatPatch), nameof(SubmitChatPatch.HandleMessage))),
                    new CodeInstruction(OpCodes.Brtrue, returnLabel)
                });

                for (int z = 0; z < newInstructions.Count; z++) yield return newInstructions[z];
            }
        }
    }
}
