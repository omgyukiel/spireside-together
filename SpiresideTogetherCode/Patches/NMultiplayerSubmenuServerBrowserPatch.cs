using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SpiresideTogether.SpiresideTogetherCode;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NMultiplayerSubmenu), nameof(NMultiplayerSubmenu._Ready))]
public static class NMultiplayerSubmenuServerBrowserPatch
{
    private const string ButtonName = "SpiresideTogetherServerBrowserButton";
    // Godot duplicate flag 4 copies scripts but not signal connections.
    private const int DuplicateScriptsOnly = 4;

    private static void Postfix(NMultiplayerSubmenu __instance)
    {
        Node submenuNode = __instance;

        Node? buttonContainer = submenuNode.GetNodeOrNull<Node>("ButtonContainer");
        if (submenuNode.GetNodeOrNull<Node>(ButtonName) != null ||
            buttonContainer?.GetNodeOrNull<Node>(ButtonName) != null)
        {
            return;
        }

        Control serverBrowserButton = CreateServerBrowserButton(submenuNode, __instance);

        if (buttonContainer != null)
        {
            buttonContainer.AddChild(serverBrowserButton);
        }
        else
        {
            serverBrowserButton.AnchorLeft = 0.5f;
            serverBrowserButton.AnchorRight = 0.5f;
            serverBrowserButton.AnchorTop = 1.0f;
            serverBrowserButton.AnchorBottom = 1.0f;
            serverBrowserButton.OffsetLeft = -110.0f;
            serverBrowserButton.OffsetRight = 110.0f;
            serverBrowserButton.OffsetTop = -124.0f;
            serverBrowserButton.OffsetBottom = -76.0f;
            submenuNode.AddChild(serverBrowserButton);
        }

        MainFile.Logger.Info("Added Server Browser button to multiplayer submenu.");
    }

    private static Control CreateServerBrowserButton(Node submenuNode, NMultiplayerSubmenu submenu)
    {
        NSubmenuButton? joinButton = submenuNode.GetNodeOrNull<NSubmenuButton>("ButtonContainer/JoinButton");
        if (joinButton != null && joinButton.Duplicate(DuplicateScriptsOnly) is NSubmenuButton submenuButton)
        {
            submenuButton.Name = ButtonName;
            submenuButton.GetNodeOrNull<MegaLabel>("%Title")?.SetTextAutoSize("Server Browser");
            submenuButton.GetNodeOrNull<MegaRichTextLabel>("%Description")?.SetTextAutoSize("Browse public Steam lobbies.");

            ((GodotObject)submenuButton).Connect(
                NClickableControl.SignalName.Released,
                Callable.From<NButton>((Action<NButton>)(_ => OpenServerBrowser(submenu))),
                0u);

            return submenuButton;
        }

        Button fallbackButton = new()
        {
            Name = ButtonName,
            Text = "Server Browser",
            CustomMinimumSize = new Vector2(220, 48),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        ((GodotObject)fallbackButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => OpenServerBrowser(submenu))),
            0u);

        return fallbackButton;
    }

    private static void OpenServerBrowser(NMultiplayerSubmenu submenu)
    {
        NMainMenu? mainMenu = FindAncestor<NMainMenu>(submenu);
        if (mainMenu == null)
        {
            MainFile.Logger.Warn("Cannot open server browser because NMainMenu ancestor was not found.");
            return;
        }

        Node mainMenuNode = mainMenu;
        Node? existing = mainMenuNode.GetNodeOrNull<Node>(SteamLobbyBrowserScreen.RootName);
        if (existing != null)
        {
            existing.QueueFree();
        }

        Control screen = SteamLobbyBrowserScreen.Create(mainMenu);
        mainMenuNode.AddChild(screen);
        MainFile.Logger.Info("Opened Spireside Together server browser screen.");
    }

    private static T? FindAncestor<T>(Node node) where T : Node
    {
        Node? current = node;
        while (current != null)
        {
            if (current is T match)
            {
                return match;
            }

            current = current.GetParent();
        }

        return null;
    }
}
