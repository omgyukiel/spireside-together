using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace SpiresideTogether.SpiresideTogetherCode;

public sealed class SteamLobbyBrowserPanel
{
    private const int RowsPerPage = 8;

    private readonly Action<ulong>? _joinLobby;
    private readonly Label _statusLabel;
    private readonly VBoxContainer _list;
    private readonly Button _previousPageButton;
    private readonly Button _nextPageButton;
    private IReadOnlyList<SteamLobbyBrowserEntry> _entries = Array.Empty<SteamLobbyBrowserEntry>();
    private int _pageIndex;

    private SteamLobbyBrowserPanel(
        Action<ulong>? joinLobby,
        Label statusLabel,
        VBoxContainer list,
        Button previousPageButton,
        Button nextPageButton)
    {
        _joinLobby = joinLobby;
        _statusLabel = statusLabel;
        _list = list;
        _previousPageButton = previousPageButton;
        _nextPageButton = nextPageButton;
    }

    public static Control Create(string name, string title, Action<ulong>? joinLobby)
    {
        Control root = CreateEmbedded(name, title, joinLobby);
        root.AnchorLeft = 1.0f;
        root.AnchorRight = 1.0f;
        root.AnchorTop = 0.5f;
        root.AnchorBottom = 0.5f;
        root.OffsetLeft = -660.0f;
        root.OffsetRight = -40.0f;
        root.OffsetTop = -190.0f;
        root.OffsetBottom = 120.0f;

        return root;
    }

    public static Control CreateEmbedded(string name, string title, Action<ulong>? joinLobby)
    {
        PanelContainer root = new()
        {
            Name = name,
            MouseFilter = Control.MouseFilterEnum.Stop,
            CustomMinimumSize = new Vector2(720, 460),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };

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
            CustomMinimumSize = new Vector2(680, 330),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        VBoxContainer list = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        HBoxContainer pagingRow = new()
        {
            MouseFilter = Control.MouseFilterEnum.Pass
        };

        Button previousPageButton = new()
        {
            Text = "<",
            CustomMinimumSize = new Vector2(54, 36),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All,
            Disabled = true
        };

        Button nextPageButton = new()
        {
            Text = ">",
            CustomMinimumSize = new Vector2(54, 36),
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All,
            Disabled = true
        };

        Label pagingSpacer = new()
        {
            Text = "",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        SteamLobbyBrowserPanel panel = new(joinLobby, statusLabel, list, previousPageButton, nextPageButton);

        ((GodotObject)refreshButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)(() => TaskHelper.RunSafely(panel.Refresh()))),
            0u);

        ((GodotObject)previousPageButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)panel.PreviousPage),
            0u);

        ((GodotObject)nextPageButton).Connect(
            Button.SignalName.Pressed,
            Callable.From((Action)panel.NextPage),
            0u);

        pagingRow.AddChild(pagingSpacer);
        pagingRow.AddChild(previousPageButton);
        pagingRow.AddChild(nextPageButton);

        header.AddChild(titleLabel);
        header.AddChild(refreshButton);
        scroll.AddChild(list);
        column.AddChild(header);
        column.AddChild(statusLabel);
        column.AddChild(scroll);
        column.AddChild(pagingRow);
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

        _entries = entries;
        _pageIndex = 0;

        if (entries.Count == 0)
        {
            _statusLabel.Text = $"Found 0 Steam lobbies. Local version: {GameCompatibilityMetadata.CurrentGameVersion}.";
            AddEmptyRow("No lobbies returned by Steam.");
            UpdatePagingButtons();
            return;
        }

        RenderPage();
    }

    private void PreviousPage()
    {
        if (_pageIndex <= 0)
        {
            return;
        }

        _pageIndex--;
        RenderPage();
    }

    private void NextPage()
    {
        if (_pageIndex >= LastPageIndex)
        {
            return;
        }

        _pageIndex++;
        RenderPage();
    }

    private void RenderPage()
    {
        ClearList();

        int startIndex = _pageIndex * RowsPerPage;
        int endIndex = Math.Min(startIndex + RowsPerPage, _entries.Count);
        for (int i = startIndex; i < endIndex; i++)
        {
            AddLobbyRow(_entries[i]);
        }

        _statusLabel.Text = $"Found {_entries.Count} Steam lobbies. Page {_pageIndex + 1}/{LastPageIndex + 1}. Local version: {GameCompatibilityMetadata.CurrentGameVersion}.";
        UpdatePagingButtons();
    }

    private int LastPageIndex => Math.Max(0, (_entries.Count - 1) / RowsPerPage);

    private void UpdatePagingButtons()
    {
        _previousPageButton.Disabled = _pageIndex <= 0 || _entries.Count <= RowsPerPage;
        _nextPageButton.Disabled = _pageIndex >= LastPageIndex || _entries.Count <= RowsPerPage;
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
            CustomMinimumSize = new Vector2(660, 38)
        };

        Label nameLabel = new()
        {
            Text = entry.Name,
            CustomMinimumSize = new Vector2(150, 34),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.Fill
        };

        Label descriptionLabel = new()
        {
            Text = entry.Description,
            CustomMinimumSize = new Vector2(250, 34),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        Label versionLabel = new()
        {
            Text = entry.GameVersion,
            CustomMinimumSize = new Vector2(100, 34),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        Label playerCountLabel = new()
        {
            Text = $"{entry.MemberCount}/{entry.MemberLimit}",
            CustomMinimumSize = new Vector2(60, 34),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        row.AddChild(nameLabel);
        row.AddChild(descriptionLabel);
        row.AddChild(versionLabel);
        row.AddChild(playerCountLabel);

        if (_joinLobby != null)
        {
            Button joinButton = new()
            {
                Text = "Join",
                CustomMinimumSize = new Vector2(74, 34),
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
