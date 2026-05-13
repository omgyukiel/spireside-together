using System;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SpiresideTogether.SpiresideTogetherCode;

public sealed class SteamLobbyBrowserScreen
{
    public const string RootName = "SpiresideTogetherServerBrowserScreen";

    public static Control Create(NMainMenu mainMenu)
    {
        Control root = new()
        {
            Name = RootName,
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        Button closeButton = new()
        {
            Text = "Back",
            CustomMinimumSize = new Vector2(100, 42),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };
        closeButton.AnchorLeft = 0.5f;
        closeButton.AnchorRight = 0.5f;
        closeButton.AnchorTop = 0.5f;
        closeButton.AnchorBottom = 0.5f;
        closeButton.OffsetLeft = 280.0f;
        closeButton.OffsetRight = 380.0f;
        closeButton.OffsetTop = -314.0f;
        closeButton.OffsetBottom = -272.0f;
        ((GodotObject)closeButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => root.QueueFree())),
            0u);

        PanelContainer panel = new()
        {
            MouseFilter = Control.MouseFilterEnum.Stop,
            CustomMinimumSize = new Vector2(760, 520)
        };
        panel.AnchorLeft = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -380.0f;
        panel.OffsetRight = 380.0f;
        panel.OffsetTop = -260.0f;
        panel.OffsetBottom = 260.0f;

        MarginContainer margin = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass
        };
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);

        Control browser = SteamLobbyBrowserPanel.CreateEmbedded(
            "SpiresideTogetherServerBrowserPanel",
            "Server Browser",
            lobbyId => JoinLobby(mainMenu, lobbyId));

        margin.AddChild(browser);
        panel.AddChild(margin);
        root.AddChild(panel);
        root.AddChild(closeButton);

        return root;
    }

    private static void JoinLobby(NMainMenu mainMenu, ulong lobbyId)
    {
        MainFile.Logger.Info($"Attempting server browser join for lobby {lobbyId}.");
        TaskHelper.RunSafely(mainMenu.JoinGame(SteamClientConnectionInitializer.FromLobby(lobbyId)));
    }
}
