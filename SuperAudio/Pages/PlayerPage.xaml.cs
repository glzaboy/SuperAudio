using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SuperAudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PlayerPage : Page
{
    private PlayerPageViewModel ViewModel { get; }
    public PlayerPage()
    {
        InitializeComponent();
        ViewModel = App.Host.Services.GetRequiredService<PlayerPageViewModel>();
    }
    // 在目标页面中
    /* protected override void OnNavigatedTo(NavigationEventArgs e)
     {
         base.OnNavigatedTo(e);
         if (e.Parameter is FileItem fileItem && e.NavigationMode == NavigationMode.Forward)
         {
             // 根据 filePath 加载 FileItem 或直接使用路径
             LoadData(fileItem);

         }
     }*/
    public void LoadData(List<FileItem> item)
    {
        ViewModel.PlayListItems = [.. item];
        StartConnectedAnimation(item[0]);
        ViewModel.PlayWithInternalPlayerCommand.Execute(item);
    }
    private async void StartConnectedAnimation(FileItem item)
    {
        // 等待 UI 布局完成，确保目标元素已渲染
        await Task.Delay(50); // 短暂延迟，让布局完成

        // 1. 获取目标元素（封面 Border 或其中的图标）
        var targetElement = CoverBorder; // 或者 CoverIcon

        // 2. 获取动画服务
        var animationService = ConnectedAnimationService.GetForCurrentView();

        // 3. 尝试获取之前准备的动画（Key 必须与源页面一致）
        var animation = animationService.GetAnimation(item.FullPath);
        if (animation != null)
        {
            // 4. 启动动画，飞入目标元素
            animation.TryStart(targetElement);
        }
        else
        {
            // 如果动画不存在，可以做个淡入效果作为后备
            targetElement.Opacity = 0;
            await Task.Delay(100);
            targetElement.Opacity = 1;
        }
    }
    public void PrepareReturnAnimation()
    {
        if (ViewModel.PlayListItems == null || CoverBorder == null)
            return;

        // 确保元素还在可视化树中且已加载
        if (!CoverBorder.IsLoaded || CoverBorder.Parent == null)
            return;

        // 额外检查：确保 XamlRoot 不为 null（WinUI 3 中）
        if (CoverBorder.XamlRoot == null)
            return;

        try
        {
            var animationService = ConnectedAnimationService.GetForCurrentView();
            FileItem currentFile = ViewModel.PlayListItems[0];
            var animationKey = currentFile.FullPath + "_return";
            var animation = animationService.PrepareToAnimate(animationKey, CoverBorder);
            if (animation != null)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PrepareReturnAnimation 失败: {ex.Message}");
        }
    }
    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        PlayListPlayer?.MediaPlayer.Pause();
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.Back)
        {
            FileItem currentFile = ViewModel.PlayListItems[0];
            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(currentFile.FullPath, CoverBorder);

            // Use the recommended configuration for back animation.
            animation.Configuration = new DirectConnectedAnimationConfiguration();

            //(this.ViewModel as IDisposable)?.Dispose();
        }
    }

}
