using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace XamGridUpdateSummary
{
    public class MainViewModel : ObservableObject
    {
        #region Constructor
        public MainViewModel()
        {
            ActiveIndex = -1;
            Formula = new Formula();

            // Add example components with arbitrary values
            var c = new ComponentItem();
            c.Name = "Water";
            c.Mass = 10.0;
            c.Density = 1.0;
            AddComponent(c);

            c = new ComponentItem();
            c.Name = "Soap";
            c.Mass = 4.21;
            c.Density = 1.73;
            AddComponent(c);

            c = new ComponentItem();
            c.Name = "Bleach";
            c.Mass = 0.24;
            c.Density = 0.92;
            AddComponent(c);

            //Formula.Calculate();
        }
        #endregion

        #region Formula

        private Formula _formula;

        /// <summary>
        /// Current Formula
        /// </summary>
        public Formula Formula
        {
            get { return _formula; }
            set { SetField(ref _formula, value); }
        }

        public ComponentItem AddComponent()
        {
            return AddComponent(new ComponentItem());
        }

        public ComponentItem AddComponent(ComponentItem item)
        {
            Formula.AddComponent(item);
            SubscribeComponent(item);
            RefreshSummaries();
            return item;
        }

        public bool RemoveComponent(ComponentItem item)
        {
            UnsubscribeComponent(item);
            bool result = Formula.RemoveComponent(item);
            RefreshSummaries();
            return result;
        }

        /// <summary>
        /// HACK: Linking refresh summary from view to view model (MVVM support)
        /// </summary>
        private Action _refreshSummaries = delegate() { };
        public Action RefreshSummaries
        {
            get { return _refreshSummaries; }
            set { _refreshSummaries = value; }
        }
        #endregion

        #region INCP Handler
        private void SubscribeComponent(ComponentItem item)
        {
            PropertyChangedEventManager.AddHandler(item as INotifyPropertyChanged, HandlePropertyChanged, string.Empty);
        }

        private void UnsubscribeComponent(ComponentItem item)
        {
            PropertyChangedEventManager.RemoveHandler(item as INotifyPropertyChanged, HandlePropertyChanged, string.Empty);
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var c = sender as ComponentItem;
            if (c == null)
            {
                Trace.TraceInformation("HandlePropertyChanged: SenderType={0}, PropertyName='{1}'",
                    sender.GetType().Name, e.PropertyName);
            }
            else if (string.IsNullOrEmpty(e.PropertyName))
            {
                Trace.TraceInformation("HandlePropertyChanged: {0}, PropertyName='{1}'",
                    c.Name, e.PropertyName);
            }
            else
            {
                PropertyInfo pi = c.GetType().GetProperty(e.PropertyName);
                var value = pi.GetValue(sender);
                Trace.TraceInformation("HandlePropertyChanged: {0}.{1} = {2}", c.Name, e.PropertyName, value);

                // Update the component and formula calculations
                if (!c.IsCalculating && (e.PropertyName == "Mass" || e.PropertyName == "Volume"))
                {
                    c.Calculate(e.PropertyName);
                    Formula.Calculate(c);
                    RefreshSummaries();
                }
            }
        }
        #endregion

        #region Selected Row and Index

        private int _activeIndex;

        /// <summary>
        /// Index of the selected row (ActiveItem)
        /// </summary>
        public int ActiveIndex
        {
            get { return _activeIndex; }
            set { SetField(ref _activeIndex, value); }
        }

        /// <summary>
        /// Selected Row / Component
        /// </summary>
        private ComponentItem _activeItem;

        /// <summary>
        /// Selected Row / Component - also updates the ActiveIndex
        /// </summary>
        public ComponentItem ActiveItem
        {
            get { return _activeItem; }
            set
            {
                SetField(ref _activeItem, value);
                ActiveIndex = Formula.Components.IndexOf(value);
            }
        }
        #endregion

        #region Copy
        /// <summary>
        /// Copied row (ComponentItem) data object
        /// </summary>
        private DataObject _copyObject;

        private RelayCommand _copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand(Copy, CanCopy);
                return _copyCommand;
            }
        }

        public bool CanCopy(object parameter)
        {
            var item = (ComponentItem)parameter;
            return item != null;
        }

        public void Copy(object parameter)
        {
            if (!CanCopy(parameter)) return;
            var item = (ComponentItem)parameter;
            _copyObject = new DataObject(DataFormats.Serializable, item, false);
            Clipboard.SetDataObject(_copyObject, false);
        }
        #endregion

        #region Paste
        private RelayCommand _pasteCommand;
        public ICommand PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                    _pasteCommand = new RelayCommand(Paste, CanPaste);
                return _pasteCommand;
            }
        }

        public bool CanPaste(object parameter)
        {
            return _copyObject != null && Clipboard.IsCurrent(_copyObject);
        }

        public void Paste(object parameter)
        {
            if (!CanPaste(parameter)) return;
            var data = Clipboard.GetDataObject();
            var source = data.GetData(DataFormats.Serializable, false);
            var item = (ComponentItem)((ComponentItem)source).Clone();
            AddComponent(item);
        }
        #endregion

        #region Add
        private RelayCommand _addCommand;
        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                    _addCommand = new RelayCommand(Add, CanAdd);
                return _addCommand;
            }
        }

        public bool CanAdd(object parameter)
        {
            return true;
        }

        public void Add(object parameter)
        {
            if (!CanAdd(parameter)) return;
            AddComponent();
        }
        #endregion

        #region Delete
        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(Delete, CanDelete);
                return _deleteCommand;
            }
        }

        public bool CanDelete(object parameter)
        {
            return ActiveItem != null;
        }

        public void Delete(object parameter)
        {
            if (!CanDelete(parameter)) return;
            var item = (ComponentItem)parameter;
            RemoveComponent(item);
        }
        #endregion

        #region Move Up
        private RelayCommand _moveUpCommand;
        public ICommand MoveUpCommand
        {
            get
            {
                if (_moveUpCommand == null)
                    _moveUpCommand = new RelayCommand(MoveUp, CanMoveUp);
                return _moveUpCommand;
            }
        }

        public bool CanMoveUp(object parameter)
        {
            var item = parameter as ComponentItem;
            return item != null && item.Position > 1;
        }

        public void MoveUp(object parameter)
        {
            if (!CanMoveUp(parameter)) return;
            var item = (ComponentItem)parameter;
            Formula.SetPosition(item, item.Position - 1);
            ActiveItem = item;
            ActiveIndex = Formula.Components.IndexOf(item);
        }
        #endregion

        #region Move Down
        private RelayCommand _moveDownCommand;
        public ICommand MoveDownCommand
        {
            get
            {
                if (_moveDownCommand == null)
                    _moveDownCommand = new RelayCommand(MoveDown, CanMoveDown);
                return _moveDownCommand;
            }
        }

        public bool CanMoveDown(object parameter)
        {
            var item = parameter as ComponentItem;
            return item != null && item.Position >= 1 && item.Position < Formula.Components.Count;
        }

        public void MoveDown(object parameter)
        {
            if (!CanMoveDown(parameter)) return;
            var item = (ComponentItem)parameter;
            Formula.SetPosition(item, item.Position + 1);
            ActiveItem = item;
            ActiveIndex = Formula.Components.IndexOf(item);
        }
        #endregion

        #region Column Chooser
        private RelayCommand _columnChooserCommand;
        public ICommand ColumnChooserCommand
        {
            get
            {
                if (_columnChooserCommand == null)
                    _columnChooserCommand = new RelayCommand(ColumnChooser);
                return _columnChooserCommand;
            }
        }

        public void ColumnChooser(object _)
        {

        }
        #endregion
    }
}
