using Infragistics.Collections;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;
using Infragistics.Windows.DataPresenter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XamGridUpdateSummary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CellControlBase _rightClickCell;

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                // HACK: Connecting the view model to refresh the grid summaries
                vm.RefreshSummaries = () =>
                {
                    if (FormulaGrid.Rows.Count > 0)
                        FormulaGrid.Rows[0].Manager.RefreshSummaries();
                };
            }
        }

        private void CommandBinding_CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.Copy(vm.ActiveItem);
            }
        }

        private void CommandBinding_PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.Paste(vm.ActiveItem);
            }
        }

        private void FormulaGrid_CellExitedEditMode(object sender, CellExitedEditingEventArgs e)
        {
            Trace.TraceInformation("CellExitedEditMode: " + e.Cell.ToString());
        }

        /// <summary>
        /// Updates the ActiveCell when the right mouse button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition((IInputElement)sender);
            var cell = (CellControl)sender;
            var parent = (CellsPanel)cell.Parent;
            var data = parent.Row.Data;
            if (FormulaGrid.ActiveCell != cell.Cell)
            {
                FormulaGrid.ActiveCell = cell.Cell;
                Trace.TraceInformation("Cell_MouseRightButtonDown: ({0}, {1}), ActiveCell: {2}", pos.X, pos.Y, FormulaGrid.ActiveCell);
            }
            _rightClickCell = cell;
        }

        /// <summary>
        /// Updates the ActiveCell when the right mouse button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderCell_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition((IInputElement)sender);
            var cell = (HeaderCellControl)sender;
            var parent = (CellsPanel)cell.Parent;
            Trace.TraceInformation("HeaderCell_MouseRightButtonDown: ({0}, {1}), ActiveCell: {2}", pos.X, pos.Y, FormulaGrid.ActiveCell);
            _rightClickCell = cell;
            RaisePropertyChanged(nameof(CanFixColumn));
            RaisePropertyChanged(nameof(IsFixedColumn));
            RaisePropertyChanged(nameof(CanGroupByColumn));
            RaisePropertyChanged(nameof(IsGroupByColumn));
        }

        private void FormulaGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var elem = FormulaGrid.InputHitTest(new Point(e.CursorLeft, e.CursorTop));
            Trace.TraceInformation("Grid_ContextMenuOpening: ({0},{1}), Sender={2}", e.CursorLeft, e.CursorTop, sender.GetType().Name);
        }

        private void FormulaGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition((IInputElement)sender);
            Trace.TraceInformation("Grid_MouseRightButtonDown: ({0},{1}), Sender={2}", pos.X, pos.Y, sender.GetType().Name);
        }

        private void FormulaGrid_ShowColumnChooser(object sender, RoutedEventArgs e)
        {
            FormulaGrid.ShowColumnChooser();
        }
        private void FormulaGrid_ShowGroupByArea(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var location = (Infragistics.Controls.Grids.GroupByAreaLocation)menuItem.Tag;

            menuItem.IsChecked = true;
            FormulaGrid.GroupBySettings.AllowGroupByArea = location;
            //FormulaGrid.GroupBySettings.IsGroupByAreaExpanded = false;

            var parentItem = (MenuItem)menuItem.Parent;
            var otherChildren = parentItem.Items.OfType<MenuItem>().Where(x => !ReferenceEquals(x, menuItem));
            foreach (var otherItem in otherChildren)
            {
                otherItem.IsChecked = false;
            }
        }

        private void FormulaGrid_ToggleFilterRow(object sender, RoutedEventArgs e)
        {
            if (FormulaGrid.FilteringSettings.AllowFiltering == Infragistics.Controls.Grids.FilterUIType.FilterRowTop)
                FormulaGrid.FilteringSettings.AllowFiltering = Infragistics.Controls.Grids.FilterUIType.None;
            else
                FormulaGrid.FilteringSettings.AllowFiltering = Infragistics.Controls.Grids.FilterUIType.FilterRowTop;
        }

        public bool CanFixColumn
        {
            get
            {
                var column = _rightClickCell?.Column;
                return column?.IsFixable ?? false;
            }
        }

        public bool IsFixedColumn
        {
            get
            {
                var column = _rightClickCell?.Column;
                return column?.IsFixed != FixedState.NotFixed;
            }
        }

        private void FormulaGrid_ToggleFixedColumn(object sender, RoutedEventArgs e)
        {
            Column column = null;
            if (_rightClickCell != null)
                column = _rightClickCell.Column;
            if (column != null && column.IsFixable)
            {
                if (column.IsFixed == FixedState.NotFixed)
                    column.IsFixed = FixedState.Left;
                else
                    column.IsFixed = FixedState.NotFixed;
                RaisePropertyChanged(nameof(CanFixColumn));
                RaisePropertyChanged(nameof(IsFixedColumn));
            }
        }

        public bool CanGroupByColumn
        {
            get
            {
                var column = _rightClickCell?.Column;
                return column?.IsGroupable ?? false;
            }
        }

        public bool IsGroupByColumn
        {
            get
            {
                var column = _rightClickCell?.Column;
                return column?.IsGroupBy ?? false;
            }
        }

        private void FormulaGrid_ToggleGroupByColumn(object sender, RoutedEventArgs e)
        {
            Column column = null;
            if (_rightClickCell != null)
                column = _rightClickCell.Column;
            if (column != null && column.IsGroupable)
            {
                column.IsGroupBy = !column.IsGroupBy;
                RaisePropertyChanged(nameof(CanGroupByColumn));
                RaisePropertyChanged(nameof(IsGroupByColumn));
            }
        }

        #region XamDataGrid methods
        /// <summary>
        /// Validate data during edit of a cell <see cref="https://www.infragistics.com/help/wpf/xamdatapresenter-validate-data-as-your-end-users-edit-a-cell"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormulaDataGrid_CellChanged(object sender, Infragistics.Windows.DataPresenter.Events.CellChangedEventArgs e)
        {
            /* ====
            object val = e.Editor.Value;
            if (!(e.Cell.Record is FilterRecord))
            {
                if (DBNull.Value == val)
                    val = null;
                e.Cell.Value = val;
            }
            ==== */
        }
        #endregion
    }
}
