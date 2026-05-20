using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.addons.mega_text;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

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
}
