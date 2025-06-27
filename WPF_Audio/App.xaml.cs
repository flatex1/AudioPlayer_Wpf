using System.Configuration;
using System.Data;
using System.Windows;
using WPF_Audio.Data;
using Microsoft.EntityFrameworkCore;
using WPF_Audio.Converters;

namespace WPF_Audio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Resources.Add("IntToVisibilityConverter", new IntToVisibilityConverter());
            Resources.Add("BoolToVisibilityConverter", new BoolToVisibilityConverter());
            Resources.Add("StringToImageSourceConverter", new StringToImageSourceConverter());

            using var db = new AudioDbContext();
            db.Database.Migrate();
        }
    }
}