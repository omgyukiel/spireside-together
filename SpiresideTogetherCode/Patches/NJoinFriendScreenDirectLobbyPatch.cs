using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NJoinFriendScreen), nameof(NJoinFriendScreen._Ready))]
public static class NJoinFriendScreenDirectLobbyPatch
{
    private const string DirectJoinRootName = "SpiresideTogetherDirectJoin";

    // Add a simple direct-join row when the existing Join Friends screen is built.
    private static void Postfix(NJoinFriendScreen __instance)
    {
        Node screenNode = __instance;

        if (screenNode.GetNodeOrNull<Node>(DirectJoinRootName) != null)
        {
            return;
        }

        Control buttonContainer = screenNode.GetNode<Control>("%ButtonContainer");
        Node? parent = buttonContainer.GetParent();
        if (parent == null)
        {
            MainFile.Logger.Warn("Could not add direct lobby join controls: ButtonContainer has no parent.");
            return;
        }

        VBoxContainer root = new()
        {
            Name = DirectJoinRootName,
            CustomMinimumSize = new Vector2(520, 70)
        };

        Label label = new()
        {
            Text = "Join by Steam lobby ID"
        };

        HBoxContainer row = new()
        {
            CustomMinimumSize = new Vector2(520, 42)
        };

        LineEdit input = new()
        {
            PlaceholderText = "Paste lobby ID",
            CustomMinimumSize = new Vector2(360, 38)
        };

        Button joinButton = new()
        {
            Text = "Join",
            CustomMinimumSize = new Vector2(120, 38)
        };

        joinButton.Pressed += () => JoinLobby(__instance, input.Text);
        input.TextSubmitted += _ => JoinLobby(__instance, input.Text);

        row.AddChild(input);
        row.AddChild(joinButton);
        root.AddChild(label);
        root.AddChild(row);

        parent.AddChild(root);
        parent.MoveChild(root, buttonContainer.GetIndex());

        MainFile.Logger.Info("Added direct Steam lobby ID join controls.");
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
