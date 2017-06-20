using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.MyObservableCollection;

namespace Todos.ViewModels
{
    class TodoItemViewModel
    {
        private MyObservableCollection<Models.TodoItem> allitems = new MyObservableCollection<Models.TodoItem>();
        public MyObservableCollection<Models.TodoItem> Allitems { get { return this.allitems; } }

        private Models.TodoItem selectedItem = default(Models.TodoItem);
        public Models.TodoItem selected
        {
            get
            {
                return this.selectedItem;
            }
            set
            {
                selectedItem = value;
            }
        }
        public TodoItemViewModel()
        {
            var db = App.conn;
            using (var statement = db.Prepare("SELECT Id, Title, Description, Time, Imguri FROM todolist"))
            {
                StringBuilder result = new StringBuilder();
                SQLiteResult r = statement.Step();
                while (SQLiteResult.ROW == r)
                {
                    for (int num = 0; num < statement.DataCount; num += 5)
                    {
                        allitems.Add(new Models.TodoItem((string)statement[num], (string)statement[num + 1], (string)statement[num + 2], Convert.ToDateTime((string)statement[num + 3]), new Uri((string)statement[num + 4])));
                    }
                    r = statement.Step();
                }
                if (SQLiteResult.DONE == r)
                {

                }
            }
        }

        public void AddTodoItem(string title, string description, DateTime date, Uri img)
        {
            img = img == null ? new Uri("ms-appx:///Assets/background.jpg") : img;
            var str = img.ToString();
            if (str.IndexOf("Assets") == -1)
            {
                img = new Uri("ms-appx:///Assets/background.jpg");
            }
            else
            {
                img = new Uri("ms-appx:///" + str.Substring(str.IndexOf("Assets/")));
            }
            var todoitem = new Models.TodoItem(title, description, date, img);
            insert(todoitem.id, title, description, date.ToString(), img.ToString());
            this.allitems.Add(todoitem);
        }
        public void insert(string id, string title, string description, string date, string img)
        {
            var db = App.conn;
            using (var statement = db.Prepare("INSERT INTO todolist (Id, Title, Description, Time, Imguri) VALUES (?, ?, ?, ?, ?);"))
            {
                statement.Bind(1, id);
                statement.Bind(2, title);
                statement.Bind(3, description);
                statement.Bind(4, date);
                statement.Bind(5, img);
                statement.Step();
            }
        }
        public void RemoveTodoItem()
        {
            allitems.Remove(this.selectedItem);
            var db = App.conn;
            using (var statement = db.Prepare("DELETE FROM todolist WHERE Id = ?;"))
            {
                statement.Bind(1, selectedItem.id);
                statement.Step();
            }
            this.selectedItem = null;
        }

        public void UpdateTodoItem(string id, string title, string description, DateTime date, Uri img)
        {
            this.selectedItem.title = title;
            this.selectedItem.detail = description;
            this.selectedItem.date = date;
            img = img == null ? selectedItem.ImageUri : img;
            var str = img.ToString();
            if (str.IndexOf("Assets") == -1)
            {
                img = new Uri("ms-appx:///Assets/1.jpg");
            }
            else
            {
                img = new Uri("ms-appx:///" + str.Substring(str.IndexOf("Assets/")));
            }
            this.selectedItem.ImageUri = img;
            var db = App.conn;
            using (var statement = db.Prepare("UPDATE todolist SET Title = ?, Description = ?, Time = ?, Imguri = ? WHERE Id = ?;"))
            {
                statement.Bind(1, selectedItem.title);
                statement.Bind(2, selectedItem.detail);
                statement.Bind(3, selectedItem.date.ToString());
                var img1 = selectedItem.ImageUri == null ? new Uri("ms-appx:///Assets/1.jpg") : selectedItem.ImageUri;
                statement.Bind(4, img1.ToString());
                statement.Bind(5, selectedItem.id);
                statement.Step();
            }

            this.selectedItem = null;
        }
    }
}
