using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Todos.Models
{
    class TodoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string idd;
        public string id
        {
            get { return idd; }
            set
            {
                idd = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("id"));
                }
            }
        }


        public string title { get; set; }
        public string detail { get; set; }
        public DateTime date { get; set; }
        private Uri _ImageUri;
        public Uri ImageUri
        {
            get { return _ImageUri; }
            set
            {
                if (!object.Equals(_ImageUri, value))
                {
                    SetImage(value);
                    _ImageUri = value;
                }
            }
        }
        private BitmapImage _ImageSource;
        public BitmapImage ImageSource
        {
            get { return _ImageSource; }
            set
            {
                _ImageSource = value;
            }
        }

        private async void SetImage(Uri targetImageUri)
        {
            if (targetImageUri == null)
            {
                ImageSource = null;
            }
            else
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(targetImageUri);
                var fileStream = await file.OpenAsync(FileAccessMode.Read);
                var img = new BitmapImage();
                img.SetSource(fileStream);
                ImageSource = img;
            }
        }
        private bool _completed;
        public bool completed
        {
            get { return _completed; }
            set
            {
                _completed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("completed"));
            }
        }
        public TodoItem(string title, string description, DateTime date, Uri imgsource)
        {
            this.idd = Guid.NewGuid().ToString();
            this.title = title;
            this.detail = description;
            this._completed = false;
            this.date = date.Date.Date;
            this._ImageUri = imgsource;
        }
        public TodoItem(string id, string title, string description, DateTime date, Uri imgsource)
        {
            this.idd = id;
            this.title = title;
            this.detail = description;
            this._completed = false;
            this.date = date.Date.Date;
            this._ImageUri = imgsource;
        }
    }
}