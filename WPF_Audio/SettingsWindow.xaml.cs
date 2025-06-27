using System.Windows;
using System.Collections.Generic;
using System.Linq;
using WPF_Audio.Data;
using WPF_Audio.Models;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WPF_Audio
{
    public partial class SettingsWindow : Window
    {
        private Dictionary<string, string> _hotkeys = new();
        private readonly string[] _actions = new[] { "PlayPause", "Next", "Prev", "Like", "VolumeUp", "VolumeDown" };

        public SettingsWindow()
        {
            InitializeComponent();
            LoadHotkeys();
        }

        private void LoadHotkeys()
        {
            using var db = new AudioDbContext();
            var settings = db.HotkeySettings.ToList();
            foreach (var action in _actions)
            {
                var setting = settings.FirstOrDefault(s => s.Action == action);
                _hotkeys[action] = setting?.KeyGesture ?? string.Empty;
            }
            HotkeyPlayPause.Text = _hotkeys["PlayPause"];
            HotkeyNext.Text = _hotkeys["Next"];
            HotkeyPrev.Text = _hotkeys["Prev"];
            HotkeyLike.Text = _hotkeys["Like"];
            HotkeyVolumeUp.Text = _hotkeys["VolumeUp"];
            HotkeyVolumeDown.Text = _hotkeys["VolumeDown"];
        }

        private void ChangeHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement btn && btn.Tag is string action)
            {
                System.Windows.Controls.TextBox targetBox = action switch
                {
                    "PlayPause" => HotkeyPlayPause,
                    "Next" => HotkeyNext,
                    "Prev" => HotkeyPrev,
                    "Like" => HotkeyLike,
                    "VolumeUp" => HotkeyVolumeUp,
                    "VolumeDown" => HotkeyVolumeDown,
                    _ => null
                };
                if (targetBox != null)
                {
                    targetBox.IsReadOnly = false;
                    targetBox.Focus();
                    targetBox.Text = "";
                    targetBox.PreviewKeyDown += HotkeyTextBox_PreviewKeyDown;
                    targetBox.Tag = action;
                }
            }
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb && tb.Tag is string action)
            {
                string gesture = "";
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    gesture += "Ctrl+";
                if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                    gesture += "Alt+";
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                    gesture += "Shift+";
                gesture += e.Key.ToString();
                _hotkeys[action] = gesture;
                tb.Text = gesture;
                tb.IsReadOnly = true;
                tb.PreviewKeyDown -= HotkeyTextBox_PreviewKeyDown;
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AudioDbContext();
            foreach (var action in _actions)
            {
                var setting = db.HotkeySettings.FirstOrDefault(s => s.Action == action);
                if (setting == null)
                {
                    db.HotkeySettings.Add(new HotkeySetting { Action = action, KeyGesture = _hotkeys[action] });
                }
                else
                {
                    setting.KeyGesture = _hotkeys[action];
                }
            }
            db.SaveChanges();
            this.Close();
        }
    }
} 