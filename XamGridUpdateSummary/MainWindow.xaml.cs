using Infragistics.Collections;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public partial class MainWindow : Window
    {
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
                FormulaGrid.ActiveCell = cell.Cell;
            Trace.TraceInformation("MouseRightButtonDown: ({0}, {1}), ActiveCell: {2}", pos.X, pos.Y, FormulaGrid.ActiveCell);
        }
    }
}
