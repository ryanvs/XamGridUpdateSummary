using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamGridUpdateSummary
{
    public class Formula : ObservableObject
    {
        // Calculate Flag
        private bool _isCalculating;
        public bool IsCalculating
        {
            get { return _isCalculating; }
            set { SetField(ref _isCalculating, value); }
        }

        // Totals
        private double _totalMass;
        public double TotalMass
        {
            get { return _totalMass; }
            set { SetField(ref _totalMass, value); }
        }

        private double _totalVolume;
        public double TotalVolume
        {
            get { return _totalVolume; }
            set { SetField(ref _totalVolume, value); }
        }

        // Components
        private Lazy<ObservableCollection<ComponentItem>> _components =
            new Lazy<ObservableCollection<ComponentItem>>(() => new ObservableCollection<ComponentItem>());

        public ObservableCollection<ComponentItem> Components
        {
            get { return _components.Value; }
        }

        public void Calculate()
        {
            Calculate(null);
        }

        public void Calculate(ComponentItem sender)
        {
            try
            {
                if (IsCalculating) return;
                IsCalculating = true;

                // Initialize the variables
                TotalVolume = 0;
                TotalMass = 0;

                // Calculate the totals
                foreach (var item in Components)
                {
                    if (!object.ReferenceEquals(sender, item))
                        item.Calculate();
                    TotalMass += item.Mass;
                    TotalVolume += item.Volume;
                }

                // Calculate the component summaries
                foreach (var item in Components)
                {
                    item.PercentByMass = (TotalMass == 0.0) ? 0.0 : item.Mass / TotalMass * 100.0;
                    item.PercentByVolume = (TotalVolume == 0.0) ? 0.0 : item.Volume / TotalVolume * 100.0;
                }
            }
            finally
            {
                IsCalculating = false;
            }
        }

        public void AddComponent(ComponentItem item)
        {
            if (!Components.Contains(item))
            {
                item.Formula = this;
                Components.Add(item);
                item.Position = Components.IndexOf(item) + 1;
                item.Calculate();
                Calculate(item);
            }
        }

        public bool RemoveComponent(ComponentItem item)
        {
            bool result = Components.Remove(item);
            if (result)
            {
                Renumber();
                Calculate();
            }
            return result;
        }

        public void Renumber()
        {
            int position = 0;
            foreach (var item in Components)
            {
                ++position;
                if (item.Position != position)
                    item.Position = position;
            }
        }

        public void SetPosition(ComponentItem item, int position)
        {
            if (Components.Contains(item))
            {
                // Check if the position is the same -- no change
                if (item.Position == position)
                    return;

                --position;
                Components.Remove(item);
                Components.Insert(position, item);
                Renumber();
            }
        }
    }
}
