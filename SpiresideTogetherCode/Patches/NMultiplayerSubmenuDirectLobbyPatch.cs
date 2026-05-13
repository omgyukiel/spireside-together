using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.addons.mega_text;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NMultiplayerSubmenu), nameof(NMultiplayerSubmenu._Ready))]
public static class NMultiplayerSubmenuDirectLobbyPatch
{
    private const string DirectJoinButtonName = "SpiresideTogetherDirectLobbyJoinButton";

    // Godot's default Duplicate() flags include signal connections. We want the
    // cloned button's visuals and script, but not the original Join Friends click handler.
    private const int DuplicateWithoutSignals = 14;

    private static void Postfix(NMultiplayerSubmenu __instance)
    {
        Node submenuNode = __instance;
        Control buttonContainer = submenuNode.GetNode<Control>("ButtonContainer");

        if (buttonContainer.GetNodeOrNull<Node>(DirectJoinButtonName) != null)
        {
            return;
        }

        NSubmenuButton templateButton = buttonContainer.GetNode<NSubmenuButton>("JoinButton");
        NSubmenuButton directJoinButton = CreateDirectJoinButton(templateButton);

        buttonContainer.AddChild(directJoinButton);
        Traverse.Create(directJoinButton).Field("_locKeyPrefix").SetValue(null);
        SetButtonText(directJoinButton);

        ((GodotObject)directJoinButton).Connect(
            NClickableControl.SignalName.Released,
            Callable.From<NButton>((Action<NButton>)(_ => JoinLobbyFromClipboard(__instance))),
            0u);

        MainFile.Logger.Info("Added direct Steam lobby ID join button to multiplayer submenu.");
    }

    private static NSubmenuButton CreateDirectJoinButton(NSubmenuButton templateButton)
    {
        Node duplicatedNode = ((Node)templateButton).Duplicate(DuplicateWithoutSignals);

        if (duplicatedNode is not NSubmenuButton directJoinButton)
        {
            throw new InvalidOperationException("Could not duplicate multiplayer submenu button.");
        }

        directJoinButton.Name = DirectJoinButtonName;
        return directJoinButton;
    }

    private static void SetButtonText(NSubmenuButton directJoinButton)
    {
        Node buttonNode = directJoinButton;

        MegaLabel? title = buttonNode.GetNodeOrNull<MegaLabel>("%Title");
        MegaRichTextLabel? description = buttonNode.GetNodeOrNull<MegaRichTextLabel>("%Description");

        if (title == null || description == null)
        {
            MainFile.Logger.Warn("Could not find title/description labels for direct lobby join button.");
            return;
        }

        title.SetTextAutoSize("Join Lobby ID");
        description.Text = "Join the Steam lobby ID currently copied to your clipboard.";
    }

    private static void JoinLobbyFromClipboard(NMultiplayerSubmenu submenu)
    {
        string rawLobbyId = DisplayServer.ClipboardGet();

        if (!LobbyIdParser.TryParseLobbyId(rawLobbyId, out ulong lobbyId))
        {
            MainFile.Logger.Warn($"Clipboard does not contain a valid Steam lobby ID: '{rawLobbyId}'.");
            return;
        }

        NMainMenu? mainMenu = FindMainMenu(submenu);
        if (mainMenu == null)
        {
            MainFile.Logger.Error("Could not find NMainMenu; direct lobby join cannot start.");
            return;
        }

        MainFile.Logger.Info($"Attempting direct Steam lobby join for clipboard lobby {lobbyId}.");
        TaskHelper.RunSafely(mainMenu.JoinGame(SteamClientConnectionInitializer.FromLobby(lobbyId)));
    }

    private static NMainMenu? FindMainMenu(NMultiplayerSubmenu submenu)
    {
        Node? current = submenu;
        while (current != null)
        {
            if (current is NMainMenu mainMenu)
            {
                return mainMenu;
            }

            current = current.GetParent();
        }

        NSubmenuStack? stack = Traverse.Create(submenu).Field("_stack").GetValue<NSubmenuStack>();
        return stack == null ? null : Traverse.Create(stack).Field("_mainMenu").GetValue<NMainMenu>();
    }
}
