using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NJoinFriendScreen), nameof(NJoinFriendScreen._Ready))]
public static class NJoinFriendScreenDirectLobbyPatch
{
    private const string DirectJoinRootName = "SpiresideTogetherDirectJoin";

    private static void Postfix(NJoinFriendScreen __instance)
    {
        Node screenNode = __instance;

        if (screenNode.GetNodeOrNull<Node>(DirectJoinRootName) != null)
        {
            return;
        }

        Control directJoinRoot = CreateDirectJoinPanel(__instance);
        screenNode.AddChild(directJoinRoot);

        MainFile.Logger.Info("Added direct Steam lobby ID join panel to Join Friends screen.");
    }

    private static Control CreateDirectJoinPanel(NJoinFriendScreen screen)
    {
        PanelContainer root = new()
        {
            Name = DirectJoinRootName,
            MouseFilter = Control.MouseFilterEnum.Stop,
            CustomMinimumSize = new Vector2(560, 118)
        };

        // Keep the direct-connect controls separate from the friend lobby list.
        // This makes them a screen-level tool instead of a fake friend lobby row.
        root.AnchorLeft = 0.5f;
        root.AnchorRight = 0.5f;
        root.AnchorTop = 1.0f;
        root.AnchorBottom = 1.0f;
        root.OffsetLeft = -280.0f;
        root.OffsetRight = 280.0f;
        root.OffsetTop = -160.0f;
        root.OffsetBottom = -42.0f;

        MarginContainer margin = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass
        };
        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);

        VBoxContainer column = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass
        };

        Label label = new()
        {
            Text = "Join by Steam lobby ID",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        HBoxContainer row = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass,
            CustomMinimumSize = new Vector2(532, 44)
        };

        NMegaLineEdit input = new()
        {
            PlaceholderText = "Paste lobby ID",
            CustomMinimumSize = new Vector2(310, 40),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        Button pasteButton = new()
        {
            Text = "Paste",
            CustomMinimumSize = new Vector2(86, 40),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        Button joinButton = new()
        {
            Text = "Join",
            CustomMinimumSize = new Vector2(86, 40),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        ((GodotObject)pasteButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => input.Text = DisplayServer.ClipboardGet().Trim())),
            0u);

        ((GodotObject)joinButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => JoinLobby(screen, input.Text))),
            0u);

        ((GodotObject)input).Connect(
            LineEdit.SignalName.TextSubmitted,
            Callable.From<string>((Action<string>)(_ => JoinLobby(screen, input.Text))),
            0u);

        row.AddChild(input);
        row.AddChild(pasteButton);
        row.AddChild(joinButton);
        column.AddChild(label);
        column.AddChild(row);
        margin.AddChild(column);
        root.AddChild(margin);

        return root;
    }

    private static void JoinLobby(NJoinFriendScreen screen, string rawLobbyId)
    {
        if (!LobbyIdParser.TryParseLobbyId(rawLobbyId, out ulong lobbyId))
        {
            MainFile.Logger.Warn($"Invalid Steam lobby ID entered: '{rawLobbyId}'.");
            return;
        }

        MainFile.Logger.Info($"Attempting direct Steam lobby join for lobby {lobbyId}.");
        TaskHelper.RunSafely(screen.JoinGameAsync(SteamClientConnectionInitializer.FromLobby(lobbyId)));
    }
}
