using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Notifications;
using System.Xml;
using Windows.Data.Xml.Dom;
namespace Todos
{
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        private ViewModels.TodoItemViewModel ViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = ((ViewModels.TodoItemViewModel)e.Parameter);
            if (ViewModel.selected != null)
            {
                CreateButton.Content = "Update";
                title.Text = ViewModel.selected.title;
                detail.Text = ViewModel.selected.detail;
                date.Date = ViewModel.selected.date;
                pictureBox1.Source = new BitmapImage(ViewModel.selected.ImageUri);
            }
            else
            {
                CreateButton.Content = "Create";
                title.Text = "";
                detail.Text = "";
                date.Date = DateTime.Now;  
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private void confirm(IUICommand command)
        {
            ViewModel.RemoveTodoItem();
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private void CreateButton_Clicked(object sender, RoutedEventArgs e)
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
            }
            else
            {
                ViewModel.AddTodoItem(title.Text, detail.Text, date.Date.Date, SelectedPicUri);
            }
            Frame.Navigate(typeof(MainPage), ViewModel);
        }
        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.selected == null)
            {
                title.Text = "";
                detail.Text = "";
                date.Date = DateTime.Now;
            }

            else if (ViewModel.selected != null)
            {
                ViewModel.RemoveTodoItem();
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }
        private Uri SelectedPicUri;
        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
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
                    pictureBox1.Source = bitmapImage;
                    SelectedPicUri = new Uri(file.Path);
                }
            }
        }
    }

}
