using Microsoft.UI.Xaml;
using SuperAudio.Helpers.SettingsHelper.Providers;
using System;
using System.Collections.Generic;

namespace SuperAudio.Helpers.SettingsHelper;


public partial class SettingsHelper : ObservableSettings
{
    private static readonly SettingsHelper instance = new(SettingsProviderFactory.CreateProvider());
    public static SettingsHelper Current => instance;

    private SettingsHelper(ISettingsProvider provider)
        : base(provider)
    {
    }
    public const int MaxRecentlyVisitedSamples = 7;

    public ElementTheme SelectedAppTheme
    {
        get => GetOrCreateDefault<ElementTheme>(ElementTheme.Default);
        set => Set(value);
    }

    public bool IsLeftMode
    {
        get => GetOrCreateDefault<bool>(true);
        set => Set(value);
    }
    public string Language
    {
        get => GetOrCreateDefault<string>("auto");
        set => Set(value);
    }

    public bool IsShowCopyLinkTeachingTip
    {
        get => GetOrCreateDefault<bool>(true);
        set => Set(value);
    }

    public List<string> RecentlyVisited
    {
        get => GetOrCreateDefault<List<string>>([]);
        private set => Set(value);
    }

    public List<string> Favorites
    {
        get => GetOrCreateDefault<List<string>>([]);
        private set => Set(value);
    }

    public bool IsFirstRun
    {
        get => GetOrCreateDefault<bool>(true);
        set => Set(value);
    }
    /// <summary>
    /// 录制完成后定位文件
    /// </summary>
    public bool IsOpenFileAfterRecording
    {
        get => GetOrCreateDefault<bool>(true);
        set => Set(value);
    }
    public void UpdateFavorites(Action<List<string>> updater)
    {
        var list = Favorites;
        updater(list);
        Favorites = list;
        _ = JumpListHelper.UpdateJumpListAsync();
    }
    public void UpdateRecentlyVisited(Action<List<string>> updater)
    {
        var list = RecentlyVisited;
        updater(list);
        RecentlyVisited = list;
        _ = JumpListHelper.UpdateJumpListAsync();
    }
}
