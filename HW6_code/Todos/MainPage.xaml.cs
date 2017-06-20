using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Runtime.InteropServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using System.Text;
using SQLitePCL;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace Todos
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
            this.ViewModel = new ViewModels.TodoItemViewModel();
            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
        }

        ViewModels.TodoItemViewModel ViewModel { get; set; }

        Models.TodoItem ShareItem;
        private Uri SelectedPicUri;
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditTodoItem.Visibility == Visibility.Collapsed)
            {
                ViewModel.selected = null;
                Frame.Navigate(typeof(NewPage), ViewModel);
            }
            else
            {
                button1.Content = "Create";
                ViewModel.selected = null;
                title.Text = "";
                detail.Text = "";
                date.Date = DateTime.Now;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
            if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
            {
                this.ViewModel = (ViewModels.TodoItemViewModel)(e.Parameter);
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }

        async void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var dp = args.Request.Data;
            var deferral = args.Request.GetDeferral();
            var photoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/background.jpg"));
            dp.Properties.Title = ShareItem.title;
            dp.Properties.Description = ShareItem.detail;
            dp.SetText("done" + ShareItem.title);
            dp.SetStorageItems(new List<StorageFile> { photoFile });
            deferral.Complete();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.selected = (Models.TodoItem)(e.ClickedItem);
            if (this.ActualWidth > 800)
            {
                title.Text = ViewModel.selected.title;
                detail.Text = ViewModel.selected.detail;
                date.Date = ViewModel.selected.date.Date;
                img.Source = new BitmapImage(ViewModel.selected.ImageUri);
                if (ViewModel.selected != null)
                {
                    button1.Content = "Update";
                    title.Text = ViewModel.selected.title;
                    detail.Text = ViewModel.selected.detail;
                    date.Date = ViewModel.selected.date;
                }
                else
                {
                    button1.Content = "Create";
                }
            }
            else
            {
                Frame.Navigate(typeof(NewPage), ViewModel);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg = "";
            if (title.Text == "")
            {
                errorMsg += "Title is empty.\n";
            }
            if (detail.Text == "")
            {
                errorMsg += "Details are empty.\n";
            }
            if (date.Date.CompareTo(DateTime.Today) < 0)
            {
                errorMsg += "Invalid Date.\n";
            }

            if (errorMsg != "")
            {
                var msgDialog = new MessageDialog(errorMsg).ShowAsync();
            }
            if (ViewModel.selected != null)
            {
                ViewModel.UpdateTodoItem("", title.Text, detail.Text, date.Date.Date, SelectedPicUri);
                var message = new MessageDialog("Update Success！").ShowAsync();
            }
            else
            {
                ViewModel.AddTodoItem(title.Text, detail.Text, date.Date.Date, SelectedPicUri);
                var message = new MessageDialog("Create Success！").ShowAsync();
            }
            title.Text = "";
            detail.Text = "";
            date.Date = DateTimeOffset.Now;
            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
            button1.Content = "Create";
            Frame.Navigate(typeof(MainPage), ViewModel);

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.selected == null)
            {
                title.Text = "";
                detail.Text = "";
                date.Date = DateTime.Now;
                img.Source = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
            }
            else
            {
                title.Text = ViewModel.selected.title;
                detail.Text = ViewModel.selected.detail;
                date.Date = ViewModel.selected.date;
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.selected = (Models.TodoItem)((MenuFlyoutItem)sender).DataContext;

            if (EditTodoItem.Visibility == Visibility.Collapsed)
            {
                Frame.Navigate(typeof(NewPage), ViewModel);
            }
            else
            {
                button1.Content = "Update";
                title.Text = ViewModel.selected.title;
                detail.Text = ViewModel.selected.detail;
                date.Date = ViewModel.selected.date;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.selected = (Models.TodoItem)((MenuFlyoutItem)sender).DataContext;
            var db = App.conn;
            if (ViewModel.selected != null) {
                using (var statement = db.Prepare("DELETE FROM todolist WHERE Id = ?")) {
                    statement.Bind(1, ViewModel.selected.id);
                    statement.Step();
                }
                ViewModel.Allitems.Remove(ViewModel.selected);
            }
            ViewModel.selected = null;
        }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(File.ReadAllText("tile.xml", Encoding.UTF8));
            foreach(var item in ViewModel.Allitems){
                var elements = XmlDoc.GetElementsByTagName("text");
                for (int i = 0; i < elements.Length;)
                {
                    var Title = elements[i++] as XmlElement;
                    Title.InnerText = item.title;
                    var Detail = elements[i++] as XmlElement;
                    Detail.InnerText = item.detail;
                }
                var tileNotification = new TileNotification(XmlDoc);
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Update(tileNotification);
            }
            
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            ShareItem = (Models.TodoItem)((MenuFlyoutItem)sender).DataContext;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
    
            request.Data.Properties.Title = ShareItem.title;
            request.Data.Properties.Description = ShareItem.detail;
            request.Data.SetText(ShareItem.detail);
            request.Data.SetText(ShareItem.detail);

            var Deferral = args.Request.GetDeferral();
            var SharePhoto = await Package.Current.InstalledLocation.GetFileAsync("Assets\\background.jpg");
            request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(SharePhoto);
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(SharePhoto));
            Deferral.Complete();

        }

        private async void button23_Click(object sender, RoutedEventArgs e)
        {
            var db = App.conn;
            using (var statement = db.Prepare("SELECT Title, Description, Time FROM todolist WHERE Title = ? OR Description = ? OR Time = ?"))
            {
                StringBuilder result = new StringBuilder();
                statement.Bind(1, textBox23.Text);
                statement.Bind(2, textBox23.Text);
                statement.Bind(3, textBox23.Text);
                textBox23.Text = "";
                SQLiteResult r = statement.Step();
                while (SQLiteResult.ROW == r)
                {

                    result.Append("Title : " + (string)statement[0] + " Description : " + (string)statement[1] + " Time : " + (string)statement[2] + "\n");
                    r = statement.Step();
                }
                if (SQLiteResult.DONE == r)
                {
                    var dialog = new MessageDialog(result.ToString())
                    {
                        Title = "搜索结果"
                    };
                    await dialog.ShowAsync();
                }
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.DecodePixelWidth = 600; 
                    await bitmapImage.SetSourceAsync(fileStream);
                    img.Source = bitmapImage;
                    SelectedPicUri = new Uri(file.Path);
                }
            }
        }
    }
}
