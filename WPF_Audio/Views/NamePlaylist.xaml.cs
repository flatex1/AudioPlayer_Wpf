using System.Windows;

namespace WPF_Audio
{
    public partial class NamePlaylist : Window
    {
        public string ResponseText { get; set; }
        public NamePlaylist(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            Question.Text = question;
            ResponseTextBox.Text = defaultAnswer;
            ResponseTextBox.SelectAll();
            ResponseTextBox.Focus();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = ResponseTextBox.Text;
            DialogResult = true;
        }
    }
}
