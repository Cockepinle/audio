using System.Collections.Generic;
using System.Windows;

namespace AudioPlayerr
{
    /// <summary>
    /// Логика взаимодействия для MyNewWindow.xaml
    /// </summary>
    public partial class MyNewWindow : Window
    {
        private MainWindow mainWindow;

        public MyNewWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }
        public void AddListenedTracks(List<string> tracks)
        {
            foreach (var track in tracks)
            {
                listBox.Items.Add(track);
            }
        }

        private void listBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedTrack = (string)listBox.SelectedItem;
            mainWindow.PlaySelectedTrack(selectedTrack);
            DialogResult = true;
        }

       
    }
}
