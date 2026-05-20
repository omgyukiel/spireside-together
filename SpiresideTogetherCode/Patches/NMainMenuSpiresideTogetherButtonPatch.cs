using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

/// <summary>
/// Adds the Spireside Together entry point to the main menu and loads the scene-backed lobby hub.
/// Hub actions are wired here so the Godot scene owns layout while this patch owns integration with
/// the native main menu and host flow.
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class NMainMenuSpiresideTogetherButtonPatch
{
    private const string ButtonName = "SpiresideTogetherMainMenuButton";
    private const string HubRootName = "SpiresideTogetherLobbyHub";
    private const string HubScenePath = "res://SpiresideTogether/ui/server_browser_screen.tscn";
    // Godot duplicate flag 4 copies scripts without copying signal connections.
    private const int DuplicateScriptsOnly = 4;

    private static void Postfix(NMainMenu __instance)
    {
        Node mainMenuNode = __instance;
        Node? buttonContainer = mainMenuNode.GetNodeOrNull<Node>("MainMenuTextButtons");
        if (buttonContainer == null)
        {
            MainFile.Logger.Warn("Could not add Spireside Together button because MainMenuTextButtons was not found.");
            return;
        }

        if (buttonContainer.GetNodeOrNull<Node>(ButtonName) != null)
        {
            return;
        }

        Control button = CreateButton(buttonContainer, __instance);
        buttonContainer.AddChild(button);
        MoveButtonAfterMultiplayer(buttonContainer, button);
        MainFile.Logger.Info("Added Spireside Together main menu button.");
    }

    private static void MoveButtonAfterMultiplayer(Node buttonContainer, Control button)
    {
        Node? multiplayerButton = buttonContainer.GetNodeOrNull<Node>("MultiplayerButton");
        if (multiplayerButton == null)
        {
            MainFile.Logger.Warn("Could not place Spireside Together button after MultiplayerButton because MultiplayerButton was not found.");
            return;
        }

        buttonContainer.MoveChild(button, multiplayerButton.GetIndex() + 1);
    }

    private static Control CreateButton(Node buttonContainer, NMainMenu mainMenu)
    {
        NMainMenuTextButton? template = buttonContainer.GetNodeOrNull<NMainMenuTextButton>("MultiplayerButton");
        if (template != null && template.Duplicate(DuplicateScriptsOnly) is NMainMenuTextButton textButton)
        {
            textButton.Name = ButtonName;
            SetTextButtonLabel(textButton);

            ((GodotObject)textButton).Connect(
                NClickableControl.SignalName.Released,
                Callable.From<NButton>((Action<NButton>)(_ => OpenHub(mainMenu))),
                0u);

            return textButton;
        }

        Button fallbackButton = new()
        {
            Name = ButtonName,
            Text = "Spireside Together",
            CustomMinimumSize = new Vector2(220, 40),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        ((GodotObject)fallbackButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => OpenHub(mainMenu))),
            0u);

        return fallbackButton;
    }

    private static void SetTextButtonLabel(NMainMenuTextButton textButton)
    {
        if (textButton.GetChildCount() == 0)
        {
            MainFile.Logger.Warn("Could not set Spireside Together button label because duplicated text button has no children.");
            return;
        }

        MegaLabel? label = textButton.GetChild<MegaLabel>(0, false);
        if (label == null)
        {
            MainFile.Logger.Warn("Could not set Spireside Together button label because first child is not a MegaLabel.");
            return;
        }

        label.Text = "Spireside Together";
    }

    private static void OpenHub(NMainMenu mainMenu)
    {
        Node mainMenuNode = mainMenu;
        Node? existing = mainMenuNode.GetNodeOrNull<Node>(HubRootName);
        if (existing != null)
        {
            existing.QueueFree();
        }

        PackedScene? scene = ResourceLoader.Load<PackedScene>(HubScenePath);
        if (scene == null)
        {
            MainFile.Logger.Error($"Could not load Spireside Together lobby hub scene at {HubScenePath}.");
            return;
        }

        Control hub = scene.Instantiate<Control>();
        hub.Name = HubRootName;
        mainMenuNode.AddChild(hub);
        WireBackButton(hub);
        WireCreateButton(hub, mainMenu);
        WireDirectJoinControls(hub, mainMenu);
        MainFile.Logger.Info("Opened Spireside Together lobby hub scene.");
    }

    private static void WireBackButton(Control hub)
    {
        Button? backButton = hub.GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/Header/BackButton");
        if (backButton == null)
        {
            MainFile.Logger.Warn("Could not wire lobby hub BackButton because it was not found.");
            return;
        }

        ((GodotObject)backButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => hub.QueueFree())),
            0u);
    }

    private static void WireCreateButton(Control hub, NMainMenu mainMenu)
    {
        LineEdit? descriptionInput = hub.GetNodeOrNull<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/CreateSection/CreateRow/DescriptionInput");
        Button? createButton = hub.GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/CreateSection/CreateRow/CreateButton");
        if (descriptionInput == null || createButton == null)
        {
            MainFile.Logger.Warn("Could not wire lobby hub CreateButton because DescriptionInput or CreateButton was not found.");
            return;
        }

        ((GodotObject)createButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => CreateStandardLobby(hub, mainMenu, descriptionInput))),
            0u);
    }

    private static void CreateStandardLobby(Control hub, NMainMenu mainMenu, LineEdit descriptionInput)
    {
        string description = SteamLobbyMetadata.NormalizeDescription(descriptionInput.Text);
        PendingLobbyCreationMetadata.SetDescription(description);
        MainFile.Logger.Info($"Creating Spireside Together standard lobby with description '{description}'.");

        hub.QueueFree();
        NMultiplayerHostSubmenu hostSubmenu = mainMenu.SubmenuStack.PushSubmenuType<NMultiplayerHostSubmenu>();
        hostSubmenu.StartHost(GameMode.Standard);
    }

    private static void WireDirectJoinControls(Control hub, NMainMenu mainMenu)
    {
        LineEdit? lobbyIdInput = hub.GetNodeOrNull<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/DirectJoinSection/DirectJoinRow/LobbyIdInput");
        Button? pasteButton = hub.GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/DirectJoinSection/DirectJoinRow/PasteButton");
        Button? joinIdButton = hub.GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/DirectJoinSection/DirectJoinRow/JoinIdButton");
        if (lobbyIdInput == null || pasteButton == null || joinIdButton == null)
        {
            MainFile.Logger.Warn("Could not wire lobby hub direct join controls because LobbyIdInput, PasteButton, or JoinIdButton was not found.");
            return;
        }

        ((GodotObject)pasteButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => PasteClipboardInto(lobbyIdInput))),
            0u);

        ((GodotObject)joinIdButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => JoinLobbyById(hub, mainMenu, lobbyIdInput.Text))),
            0u);

        ((GodotObject)lobbyIdInput).Connect(
            LineEdit.SignalName.TextSubmitted,
            Callable.From<string>((Action<string>)(rawLobbyId => JoinLobbyById(hub, mainMenu, rawLobbyId))),
            0u);
    }

    private static void PasteClipboardInto(LineEdit input)
    {
        input.Text = DisplayServer.ClipboardGet();
        input.GrabFocus();
        input.CaretColumn = input.Text.Length;
    }

    private static void JoinLobbyById(Control hub, NMainMenu mainMenu, string? rawLobbyId)
    {
        if (!LobbyIdParser.TryParseLobbyId(rawLobbyId, out ulong lobbyId))
        {
            MainFile.Logger.Warn($"Could not direct join lobby because '{rawLobbyId}' is not a valid Steam lobby id.");
            return;
        }

        MainFile.Logger.Info($"Joining Steam lobby by id {lobbyId}.");
        hub.QueueFree();
        TaskHelper.RunSafely(mainMenu.JoinGame(SteamClientConnectionInitializer.FromLobby(lobbyId)));
    }
}
