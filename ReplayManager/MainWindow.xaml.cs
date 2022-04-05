﻿using Microsoft.Win32;
using ReplayManager.Classes;
using ReplayManager.Classes.Records;
using ReplayManager.Classes.XMLFormalization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace ReplayManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<age3rec> Records { get; set; } = new ObservableCollection<age3rec>();

        private RelayCommand closeCommand;

        public RelayCommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new RelayCommand(w => CloseCommandMethod(w), null);
                }
                return this.closeCommand;
            }
        }

        private RelayCommand renameCommand;

        public RelayCommand RenameCommand
        {
            get
            {
                if (this.renameCommand == null)
                {
                    this.renameCommand = new RelayCommand(w => RenameCommandMethod(w), null);
                }
                return this.renameCommand;
            }
        }

        public void OnReceiveLine(string file)
        {
            Dispatcher.Invoke(async () =>
            {

                await readFiles(new string[] { file });

                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                Topmost = true;
                Activate();
                await Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }));
            });
        }

        public MainWindow()
        {
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 20 });
            InitializeComponent();
            DataContext = this;
            var myCur = Application.GetResourceStream(new Uri("pack://application:,,,/resources/Cursor.cur")).Stream;
            Cursor = new Cursor(myCur);
            /*
                        FXML.GenerateUnitsFile(@"C:\Users\vladt\Desktop\Формализация данных\stringtabley.xml",
                                         @"C:\Users\vladt\Desktop\Формализация данных\protoy.xml");
                        FXML.GenerateTechsFile(@"C:\Users\vladt\Desktop\Формализация данных\stringtabley.xml",
                            @"C:\Users\vladt\Desktop\Формализация данных\techtreey.xml");
                        FXML.GenerateDecksFile(@"C:\Users\vladt\Desktop\Формализация данных\stringtabley.xml",
                            @"C:\Users\vladt\Desktop\Формализация данных\techtreey.xml", new List<string>() {
                                     @"C:\Users\vladt\Desktop\Формализация данных\homecityxpsioux.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityamericans.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecitybritish.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecitychinese.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecitydeinca.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecitydutch.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityethiopians.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityfrench.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecitygerman.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityhausa.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityindians.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityjapanese.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecitymexicans.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityottomans.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityportuguese.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityrussians.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityspanish.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityswedish.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityxpaztec.xml",
                         @"C:\Users\vladt\Desktop\Формализация данных\homecityxpiroquois.xml", });


                        var json = File.ReadAllText("data/Decks.txt");
                        var Decks = JsonConvert.DeserializeObject<List<DeckData>>(json);
                        List<string> icons = new List<string>();
                        Decks.ForEach(d => { icons.AddRange(d.Cards.Select(x => x.Icon)); });

                        File.WriteAllLines("icons.txt", icons.Distinct().ToList());*/
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            };
            Process.Start(psi);
        }



        private void SaveControlImage(FrameworkElement control,
string filename)
        {
            // Get the size of the Visual and its descendants.
            Rect rect = VisualTreeHelper.GetDescendantBounds(control);

            // Make a DrawingVisual to make a screen
            // representation of the control.
            DrawingVisual dv = new DrawingVisual();

            // Fill a rectangle the same size as the control
            // with a brush containing images of the control.
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(control);
                ctx.DrawRectangle(brush, null, new Rect(rect.Size));
            }

            // Make a bitmap and draw on it.
            int width = (int)control.ActualWidth;
            int height = (int)control.ActualHeight;
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            // Make a PNG encoder.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            // Save the file.
            using (FileStream fs = new FileStream(Path.Combine(AppContext.BaseDirectory, filename),
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(fs);
            }


            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer",
                Arguments = "/e, /select, \"" + Path.Combine(AppContext.BaseDirectory, filename) + "\""
            });
            Process.Start(new ProcessStartInfo()
            {
                FileName = Path.Combine(Environment.CurrentDirectory, filename),
                UseShellExecute = true,
                Verb = "open"
            });
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }

        private async Task readFiles(string[] filenames)
        {
            gProcessing.Visibility = Visibility.Visible;
            bRenameAll.IsEnabled = false;
            bOpen.IsEnabled = false;
            foreach (string file in filenames)
            {
                if (Path.GetExtension(file).ToLower() != ".age3yrec")
                {
                    continue;
                }

                if (Records.Any(x => x.RecordPath.ToLower() == file.ToLower()))
                {
                    continue;
                }
                var record = new age3rec();
                if (await record.Read(file))
                {
                    Records.Add(record);
                }
            }
            bOpen.IsEnabled = true;
            if (Records.Count > 0)
            {
                bRenameAll.IsEnabled = true;
                gProcessing.Visibility = Visibility.Collapsed;
            }
            else
            {
                bRenameAll.IsEnabled = false;
                gProcessing.Visibility = Visibility.Collapsed;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Age of Empires 3 record|*.age3yrec";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                await readFiles(openFileDialog.FileNames);
            }

        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var grid in FindVisualChildren<Border>(this))
            {
                if (grid.Name == "gDeck")
                {
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Screenshots"));
                    SaveControlImage(grid, Path.Combine(AppContext.BaseDirectory, "Screenshots", DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss") + ".png"));
                }
            }

        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseCommandMethod(object parameter)
        {
            Records.Remove(Records.First(x=> x.RecordPath == parameter.ToString()));
            if (Records.Count == 0)
            {
                bRenameAll.IsEnabled = false;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void RenameCommandMethod(object parameter)
        {
            bRenameAll.IsEnabled = false;
            bOpen.IsEnabled = false;
            try
            {
                var record = Records.First(x => x.RecordPath == parameter.ToString());
                string newPath = Path.Combine(Path.GetDirectoryName(record.RecordPath), record.RenamedRecord);
                File.Move(record.RecordPath, newPath, true);
                record.RecordPath = newPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            bRenameAll.IsEnabled = true;
            bOpen.IsEnabled = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            bOpen.IsEnabled = false;
            bRenameAll.IsEnabled = false;
            foreach (var record in Records)
            {
                try
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(record.RecordPath), record.RenamedRecord);
                    File.Move(record.RecordPath, newPath, true);
                    record.RecordPath = newPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            bRenameAll.IsEnabled = true;
            bOpen.IsEnabled = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("OpENED");
            await readFiles(Environment.GetCommandLineArgs());
            var pipe = new SingleInstanceApp();
            pipe.ReceiveLine += OnReceiveLine;
            await pipe.StartServer("ReplayManager");
        }
    }

    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom(value.ToString());
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString() != "1")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class AmountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString() != "-1")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
