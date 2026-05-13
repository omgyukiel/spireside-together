using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace SpiresideTogether.SpiresideTogetherCode;

public sealed class SteamLobbyBrowserPanel
{
    private readonly Action<ulong>? _joinLobby;
    private readonly Label _statusLabel;
    private readonly VBoxContainer _list;

    private SteamLobbyBrowserPanel(Action<ulong>? joinLobby, Label statusLabel, VBoxContainer list)
    {
        _joinLobby = joinLobby;
        _statusLabel = statusLabel;
        _list = list;
    }

    public static Control Create(string name, string title, Action<ulong>? joinLobby)
    {
        PanelContainer root = new()
        {
            Name = name,
            MouseFilter = Control.MouseFilterEnum.Stop,
            CustomMinimumSize = new Vector2(620, 310)
        };

        root.AnchorLeft = 1.0f;
        root.AnchorRight = 1.0f;
        root.AnchorTop = 0.5f;
        root.AnchorBottom = 0.5f;
        root.OffsetLeft = -660.0f;
        root.OffsetRight = -40.0f;
        root.OffsetTop = -190.0f;
        root.OffsetBottom = 120.0f;

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

        HBoxContainer header = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass
        };

        Label titleLabel = new()
        {
            Text = title,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        Button refreshButton = new()
        {
            Text = "Refresh",
            CustomMinimumSize = new Vector2(110, 38),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All
        };

        Label statusLabel = new()
        {
            Text = $"Local game version: {GameCompatibilityMetadata.CurrentGameVersion}. Refresh to call Steam RequestLobbyList().",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        ScrollContainer scroll = new()
        {
            CustomMinimumSize = new Vector2(590, 210),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        VBoxContainer list = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        SteamLobbyBrowserPanel panel = new(joinLobby, statusLabel, list);

        ((GodotObject)refreshButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => TaskHelper.RunSafely(panel.Refresh()))),
            0u);

        header.AddChild(titleLabel);
        header.AddChild(refreshButton);
        scroll.AddChild(list);
        column.AddChild(header);
        column.AddChild(statusLabel);
        column.AddChild(scroll);
        margin.AddChild(column);
        root.AddChild(margin);

        return root;
    }

    private async Task Refresh()
    {
        _statusLabel.Text = "Requesting Steam lobby list...";
        ClearList();

        IReadOnlyList<SteamLobbyBrowserEntry> entries;
        try
        {
            entries = await SteamLobbyBrowser.RequestPublicLobbies();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Steam lobby browser refresh failed: {ex}");
            _statusLabel.Text = "Steam lobby request failed. Check logs.";
            return;
        }

        _statusLabel.Text = $"Found {entries.Count} Steam lobbies. Local version: {GameCompatibilityMetadata.CurrentGameVersion}.";

        if (entries.Count == 0)
        {
            AddEmptyRow("No lobbies returned by Steam.");
            return;
        }

        foreach (SteamLobbyBrowserEntry entry in entries)
        {
            AddLobbyRow(entry);
        }
    }

    private void ClearList()
    {
        foreach (Node child in _list.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void AddEmptyRow(string text)
    {
        _list.AddChild(new Label
        {
            Text = text,
            MouseFilter = Control.MouseFilterEnum.Ignore
        });
    }

    private void AddLobbyRow(SteamLobbyBrowserEntry entry)
    {
        HBoxContainer row = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass,
            CustomMinimumSize = new Vector2(570, 42)
        };

        string displayName = string.IsNullOrWhiteSpace(entry.Name) ? "(unnamed)" : entry.Name;
        string localGameVersion = GameCompatibilityMetadata.CurrentGameVersion;
        string gameVersion = string.IsNullOrWhiteSpace(entry.GameVersion) ? "unknown" : entry.GameVersion;
        string versionLabel = gameVersion == localGameVersion
            ? gameVersion
            : $"{gameVersion} != local {localGameVersion}";

        Label label = new()
        {
            Text = $"{displayName} | host v {versionLabel} | {entry.MemberCount}/{entry.MemberLimit} | {entry.LobbyId}",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        row.AddChild(label);

        if (_joinLobby != null)
        {
            Button joinButton = new()
            {
                Text = "Join",
                CustomMinimumSize = new Vector2(86, 36),
                MouseFilter = Control.MouseFilterEnum.Stop,
                FocusMode = Control.FocusModeEnum.All
            };

            ((GodotObject)joinButton).Connect(
                Button.SignalName.Pressed,
                Callable.From((Action)(() => _joinLobby(entry.LobbyId))),
                0u);

            row.AddChild(joinButton);
        }

        _list.AddChild(row);
    }
}
