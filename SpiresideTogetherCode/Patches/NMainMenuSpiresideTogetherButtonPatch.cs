using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private const string LobbyRowScenePath = "res://SpiresideTogether/ui/LobbyRow.tscn";
    private static readonly Color VersionMismatchColor = new(0.95f, 0.25f, 0.2f);
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
        EnableSpiresideButton(button);
        MainFile.Logger.Info("Added Spireside Together main menu button.");
    }

    internal static void EnableSpiresideButton(NMainMenu mainMenu)
    {
        Node mainMenuNode = mainMenu;
        Control? button = mainMenuNode.GetNodeOrNull<Control>($"MainMenuTextButtons/{ButtonName}");
        if (button == null)
        {
            return;
        }

        EnableSpiresideButton(button);
    }

    private static void EnableSpiresideButton(Control button)
    {
        if (button is NClickableControl clickableControl)
        {
            clickableControl.Enable();
        }

        if (button is Button godotButton)
        {
            godotButton.Disabled = false;
        }

        button.Modulate = Colors.White;
        button.SelfModulate = Colors.White;
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
        ClearMockLobbyRows(hub);
        WireBackButton(hub);
        WireCreateButton(hub, mainMenu);
        WireDirectJoinControls(hub, mainMenu);
        WireRefreshButton(hub, mainMenu);
        MainFile.Logger.Info("Opened Spireside Together lobby hub scene.");
    }

    private static void ClearMockLobbyRows(Control hub)
    {
        VBoxContainer? rows = hub.GetNodeOrNull<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/ScrollMarginContainer/Rows");
        if (rows == null)
        {
            MainFile.Logger.Warn("Could not clear mock lobby rows because Rows was not found.");
            return;
        }

        ClearLobbyRows(rows);
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

    private static void WireRefreshButton(Control hub, NMainMenu mainMenu)
    {
        Button? refreshButton = hub.GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/BrowserHeader/RefreshButton");
        LineEdit? filterInput = hub.GetNodeOrNull<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/BrowserHeader/FilterRowsInput");
        Label? statusLabel = hub.GetNodeOrNull<Label>("PanelContainer/MarginContainer/VBoxContainer/StatusLabel");
        VBoxContainer? rows = hub.GetNodeOrNull<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/ScrollMarginContainer/Rows");
        PackedScene? rowScene = ResourceLoader.Load<PackedScene>(LobbyRowScenePath);
        if (refreshButton == null || statusLabel == null || rows == null || rowScene == null)
        {
            MainFile.Logger.Warn("Could not wire lobby hub RefreshButton because RefreshButton, StatusLabel, Rows, or LobbyRow scene was not found.");
            return;
        }

        statusLabel.Text = $"Ready to refresh public lobbies. Your version: {GameCompatibilityMetadata.CurrentGameVersion}.";
        List<SteamLobbyBrowserEntry> cachedEntries = new();

        ((GodotObject)refreshButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => TaskHelper.RunSafely(RefreshLobbyList(hub, mainMenu, statusLabel, rows, rowScene, filterInput, cachedEntries)))),
            0u);

        if (filterInput != null)
        {
            ((GodotObject)filterInput).Connect(
                LineEdit.SignalName.TextChanged,
                Callable.From<string>((Action<string>)(filterText => UpdateFilteredLobbyRows(hub, mainMenu, statusLabel, rows, rowScene, cachedEntries, filterText))),
                0u);
        }
    }

    private static async Task RefreshLobbyList(
        Control hub,
        NMainMenu mainMenu,
        Label statusLabel,
        VBoxContainer rows,
        PackedScene rowScene,
        LineEdit? filterInput,
        List<SteamLobbyBrowserEntry> cachedEntries)
    {
        statusLabel.Text = "Requesting public lobbies from Steam...";
        ClearLobbyRows(rows);
        MainFile.Logger.Info("Requesting Spireside Together public lobby list from Steam.");

        try
        {
            var entries = await SteamLobbyBrowser.RequestPublicLobbies();
            cachedEntries.Clear();
            cachedEntries.AddRange(entries);
            string filterText = filterInput?.Text ?? "";
            int visibleRows = UpdateFilteredLobbyRows(hub, mainMenu, statusLabel, rows, rowScene, cachedEntries, filterText);
            statusLabel.Text = FormatLobbyStatus(entries.Count, visibleRows);
            MainFile.Logger.Info($"Spireside Together lobby refresh returned {entries.Count} public lobbies.");

            foreach (SteamLobbyBrowserEntry entry in entries)
            {
                MainFile.Logger.Info(
                    $"Lobby {entry.LobbyId}: owner={entry.OwnerId}, name='{entry.Name}', description='{entry.Description}', version='{entry.GameVersion}', players={entry.MemberCount}/{entry.MemberLimit}");
            }
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Steam lobby refresh failed. Check logs.";
            MainFile.Logger.Error($"Spireside Together lobby refresh failed: {ex}");
        }
    }

    private static int UpdateFilteredLobbyRows(
        Control hub,
        NMainMenu mainMenu,
        Label statusLabel,
        VBoxContainer rows,
        PackedScene rowScene,
        IReadOnlyList<SteamLobbyBrowserEntry> entries,
        string? filterText)
    {
        ClearLobbyRows(rows);

        if (entries.Count == 0)
        {
            AddEmptyLobbyRow(rows, "No public lobbies found.");
            return 0;
        }

        int visibleRows = 0;
        foreach (SteamLobbyBrowserEntry entry in entries)
        {
            if (!MatchesLobbyFilter(entry, filterText))
            {
                continue;
            }

            AddLobbyRow(hub, mainMenu, rows, rowScene, entry);
            visibleRows++;
        }

        if (visibleRows == 0)
        {
            AddEmptyLobbyRow(rows, "No lobbies match the filter.");
        }

        statusLabel.Text = FormatLobbyStatus(entries.Count, visibleRows);
        return visibleRows;
    }

    private static bool MatchesLobbyFilter(SteamLobbyBrowserEntry entry, string? filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return true;
        }

        string filter = filterText.Trim();
        return entry.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
               || entry.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)
               || entry.GameVersion.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatLobbyStatus(int totalRows, int visibleRows)
    {
        string version = GameCompatibilityMetadata.CurrentGameVersion;
        return visibleRows == totalRows
            ? $"Found {totalRows} public lobbies. Your version: {version}."
            : $"Showing {visibleRows} of {totalRows} public lobbies. Your version: {version}.";
    }

    private static void ClearLobbyRows(VBoxContainer rows)
    {
        foreach (Node child in rows.GetChildren())
        {
            child.QueueFree();
        }
    }

    private static void AddEmptyLobbyRow(VBoxContainer rows, string text)
    {
        rows.AddChild(new Label
        {
            Text = text,
            MouseFilter = Control.MouseFilterEnum.Ignore
        });
    }

    private static void AddLobbyRow(
        Control hub,
        NMainMenu mainMenu,
        VBoxContainer rows,
        PackedScene rowScene,
        SteamLobbyBrowserEntry entry)
    {
        Control row = rowScene.Instantiate<Control>();
        SetRowLabel(row, "HostNameLabel", entry.Name);
        SetRowLabel(row, "DescriptionLabel", entry.Description);
        SetRowLabel(row, "VersionLabel", entry.GameVersion);
        SetRowLabel(row, "PlayersLabel", $"{entry.MemberCount}/{entry.MemberLimit}");
        WireRowJoinButton(hub, mainMenu, row, entry);
        rows.AddChild(row);
    }

    private static void SetRowLabel(Control row, string nodeName, string text)
    {
        Label? label = row.GetNodeOrNull<Label>(nodeName);
        if (label == null)
        {
            MainFile.Logger.Warn($"Could not set lobby row label because {nodeName} was not found.");
            return;
        }

        label.Text = text;
    }

    private static void WireRowJoinButton(Control hub, NMainMenu mainMenu, Control row, SteamLobbyBrowserEntry entry)
    {
        Button? joinButton = row.GetNodeOrNull<Button>("JoinButton");
        if (joinButton == null)
        {
            MainFile.Logger.Warn($"Could not wire lobby row JoinButton for lobby {entry.LobbyId} because JoinButton was not found.");
            return;
        }

        if (!IsCompatibleLobbyVersion(entry.GameVersion))
        {
            joinButton.Disabled = true;
            joinButton.Text = "Version";
            joinButton.TooltipText = $"Host version {entry.GameVersion} does not match your version {GameCompatibilityMetadata.CurrentGameVersion}.";
            joinButton.Modulate = VersionMismatchColor;
            SetRowLabelColor(row, "VersionLabel", VersionMismatchColor);
            return;
        }

        ((GodotObject)joinButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => JoinLobbyById(hub, mainMenu, entry.LobbyId.ToString()))),
            0u);
    }

    private static bool IsCompatibleLobbyVersion(string lobbyGameVersion)
    {
        return string.Equals(lobbyGameVersion, GameCompatibilityMetadata.CurrentGameVersion, StringComparison.OrdinalIgnoreCase);
    }

    private static void SetRowLabelColor(Control row, string nodeName, Color color)
    {
        Label? label = row.GetNodeOrNull<Label>(nodeName);
        if (label == null)
        {
            return;
        }

        label.Modulate = color;
    }
}
