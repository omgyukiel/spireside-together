using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace SpiresideTogether.SpiresideTogetherCode;

public static class SpiresideLobbyUiScenes
{
    private const string HostLobbyIdScenePath = "res://SpiresideTogether/ui/HostLobbyId.tscn";
    private const string HostLobbyIdRootName = "SpiresideTogetherHostLobbyId";

    public static void ShowHostLobbyId(string lobbyId)
    {
        Control? scene = InstantiateScene(HostLobbyIdScenePath, HostLobbyIdRootName);
        if (scene == null)
        {
            return;
        }

        Label? label = scene.GetNodeOrNull<Label>("Panel/HBoxContainer/LobbyIdLabel");
        Button? copyButton = scene.GetNodeOrNull<Button>("Panel/HBoxContainer/CopyLobbyIdButton");
        if (label == null || copyButton == null)
        {
            MainFile.Logger.Warn("Could not wire host lobby id scene because LobbyIdLabel or CopyLobbyIdButton was not found.");
            scene.QueueFree();
            return;
        }

        label.Text = $"LobbyId: {lobbyId}";
        ((GodotObject)copyButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => DisplayServer.ClipboardSet(lobbyId))),
            0u);

        AddToCurrentMainMenu(scene, HostLobbyIdRootName);
    }

    private static Control? InstantiateScene(string scenePath, string sceneName)
    {
        PackedScene? packedScene = ResourceLoader.Load<PackedScene>(scenePath);
        if (packedScene == null)
        {
            MainFile.Logger.Warn($"Could not load {sceneName} scene at {scenePath}.");
            return null;
        }

        Control scene = packedScene.Instantiate<Control>();
        scene.Name = sceneName;
        return scene;
    }

    private static void AddToCurrentMainMenu(Control scene, string rootName)
    {
        Node? parent = NGame.Instance?.MainMenu;
        if (parent == null)
        {
            MainFile.Logger.Warn($"Could not add {rootName} because the main menu is not available.");
            scene.QueueFree();
            return;
        }

        Node? existing = parent.GetNodeOrNull<Node>(rootName);
        existing?.QueueFree();
        parent.AddChild(scene);
    }
}
