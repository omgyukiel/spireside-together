using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SpiresideTogether.SpiresideTogetherCode;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NMultiplayerSubmenu), nameof(NMultiplayerSubmenu._Ready))]
public static class NMultiplayerSubmenuServerBrowserPatch
{
    private const string ButtonName = "SpiresideTogetherServerBrowserButton";

    private static void Postfix(NMultiplayerSubmenu __instance)
    {
        Node submenuNode = __instance;

        if (submenuNode.GetNodeOrNull<Node>(ButtonName) != null)
        {
            return;
        }

        Control serverBrowserButton = CreateServerBrowserButton(submenuNode, __instance);
        serverBrowserButton.AnchorLeft = 1.0f;
        serverBrowserButton.AnchorRight = 1.0f;
        serverBrowserButton.AnchorTop = 1.0f;
        serverBrowserButton.AnchorBottom = 1.0f;
        serverBrowserButton.OffsetLeft = -172.0f;
        serverBrowserButton.OffsetRight = -36.0f;
        serverBrowserButton.OffsetTop = -92.0f;
        serverBrowserButton.OffsetBottom = -48.0f;
        submenuNode.AddChild(serverBrowserButton);

        MainFile.Logger.Info("Added Server Browser button to multiplayer submenu.");
    }

    private static Control CreateServerBrowserButton(Node submenuNode, NMultiplayerSubmenu submenu)
    {
        Button button = new()
        {
            Name = ButtonName,
            Text = "Browse",
            CustomMinimumSize = new Vector2(136, 44),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        ((GodotObject)button).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => OpenServerBrowser(submenu))),
            0u);

        return button;
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
