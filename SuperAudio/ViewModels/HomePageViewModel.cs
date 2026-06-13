using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using System;

namespace SuperAudio.ViewModels
{
    public partial class HomePageViewModel:ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; } = $"this is {DateTime.Now.ToString()}";


    }
}
