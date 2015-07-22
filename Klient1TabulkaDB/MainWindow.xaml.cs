using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Shapes;

namespace Klient1TabulkaDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool Rendered = false;
        public MainWindow()
        {
            InitializeComponent();
            employeeBox.DisplayDateEnd = DateTime.Now;
            Refresh_Click(this, new RoutedEventArgs());
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!Rendered)
            {
                Rendered = true;
                GridViewColumnHeader_Click(surnameHeader.Header, new RoutedEventArgs());
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            DB.CloseConnection();
            base.OnClosing(e);
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (nameBox.Text.Length > 0 & surnameBox.Text.Length > 0 & birthdateBox.Text.Length > 0 & employeeBox.Text.Length > 0 & wageBox.Text.Length > 0) 
            {
                EmployeeItem item = new EmployeeItem();
                item.Name = nameBox.Text;
                item.Surname = surnameBox.Text;
                item.Date_of_birth = DateTime.Parse(birthdateBox.Text);
                item.Employed_since = DateTime.Parse(employeeBox.Text);
                item.Wage = int.Parse(wageBox.Text);
                try
                {
                    DB.InsertRecord(item);
                    clearButton_Click(sender, e);
                    listView.Items.Add(item);
                    Storyboard sb = this.FindResource("titleAnimation") as Storyboard;
                    sb.Begin();
                }
                catch (Exception ex)
                {
                    errorText.Text = ex.Message;
                    Storyboard sb = this.FindResource("errorAnimation") as Storyboard;
                    sb.Begin();
                }
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            nameBox.Text = "";
            surnameBox.Text = "";
            birthdateBox.Text = "";
            employeeBox.Text = "";
            wageBox.Text = "";
        }

        private void ClearDB_Click(object sender, RoutedEventArgs e)
        {
            if (DB.Connection == null) 
            {
                Refresh_Click(sender, e);
            }
            MessageBoxResult result = MessageBox.Show("Do you really want to reset the database? This will erase all of the records.", "Delete record", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
            if (result == MessageBoxResult.OK)
            {
                DB.ResetDatabase();
            }
            Refresh();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            DB.CloseConnection();
            DB.InitializeConnection();
            Footer.Content = "Online";
            submitButton.IsEnabled = true;
            Footer.Foreground = Brushes.DarkGreen;
            Refresh();
        }

        private void Terminate_Click(object sender, RoutedEventArgs e)
        {
            DB.CloseConnection();
            listView.Items.Clear();
            Footer.Content = "Offline";
            submitButton.IsEnabled = false;
            Footer.Foreground = Brushes.DimGray;
        }

        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                EmployeeItem[] items = new EmployeeItem[listView.SelectedItems.Count];
                listView.SelectedItems.CopyTo(items, 0);
                if (items.Length > 1)
                {
                    MessageBoxResult result = MessageBox.Show("Do you really want to remove selected records (" + items.Length + ")?", "Delete records", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                    if (result == MessageBoxResult.OK)
                    {
                        foreach (EmployeeItem item in items)
                        {
                            DB.DeleteRecord(item);
                            listView.Items.Remove(item);
                        }
                    }
                }
                else if (items.Length == 1)
                {
                    EmployeeItem selectedItem = (listView.SelectedItem as EmployeeItem);
                    MessageBoxResult result = MessageBox.Show("Do you really want to remove \"" + selectedItem.Surname + "\"?", "Delete record", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                    if (result == MessageBoxResult.OK)
                    {
                        DB.DeleteRecord(selectedItem);
                        listView.Items.Remove(selectedItem);
                    }
                }
            }
        }

        void Refresh()
        {
            listView.Items.Clear();
            EmployeeItem[] items = DB.LoadData();
            foreach (EmployeeItem item in items)
            {
                listView.Items.Add(item);
            }
            listView.Items.Refresh();
        }

        private void form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter & !e.IsRepeat & submitButton.IsEnabled)
            {
                submitButton_Click(sender, e);
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainGrid.ColumnDefinitions[1].MaxWidth = this.RenderSize.Width - SystemParameters.ResizeFrameVerticalBorderWidth * 4;
            mainGrid.ColumnDefinitions[1].Width = new GridLength(mainGrid.ColumnDefinitions[1].Width.Value);
        }

        private void EditBox_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            EmployeeItem item = listView.SelectedItem as EmployeeItem;
            if (item.Name == e.OldValue.ToString())
                item.Name = e.NewValue as string;
            else if (item.Surname == e.OldValue.ToString())
                item.Surname = e.NewValue as string;
            else if (item.Date_of_birth.ToShortDateString() == e.OldValue.ToString())
                item.Date_of_birth = DateTime.Parse(e.NewValue as string);
            else if (item.Employed_since.ToShortDateString() == e.OldValue.ToString())
                item.Employed_since = DateTime.Parse(e.NewValue as string);
            else
            {
                int i = 0;
                if (int.TryParse(e.OldValue.ToString(), out i))
                {
                    if (item.Wage == i)
                        item.Wage = int.Parse(e.NewValue.ToString());
                }
            }
            DB.UpdateRecord(item);
            Refresh();
        }

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                listView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            listView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            listView.Items.Refresh();
        }
    }

    public class EmployeeItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Date_of_birth { get; set; }
        public int Wage { get; set; }
        public DateTime Employed_since { get; set; }

        public EmployeeItem()
        {
        }

        public EmployeeItem(int ID, string name, string surname, DateTime date, int wage, DateTime employedsince)
        {
            this.ID = ID;
            this.Name = name;
            this.Surname = surname;
            this.Date_of_birth = date;
            this.Wage = wage;
            this.Employed_since = employedsince;
        }
    }
}
