using System.Configuration;
using System.Data;
using System.Windows;
using WPF_Audio.Data;
using Microsoft.EntityFrameworkCore;

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

            using (var db = new AudioDbContext())
            {
                db.Database.Migrate();
            }
        }
    }
}