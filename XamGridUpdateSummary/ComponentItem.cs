using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamGridUpdateSummary
{
    [PropertyChanged.ImplementPropertyChanged]
    [Serializable]
    public class ComponentItem : ICloneable
    {
        [NonSerialized]
        private Formula _formula;
        public Formula Formula
        {
            get { return _formula; }
            set { _formula = value; }
        }

        [NonSerialized]
        private bool _isCalculating;
        public bool IsCalculating
        {
            get { return _isCalculating; }
            set { _isCalculating = value; }
        }

        public int Position { get; set; }

        [Required]
        public string Name { get; set; }
        public double Mass { get; set; }
        public double Volume { get; set; }
        [PropertyChanged.AlsoNotifyFor("Mass", "Volume", "PercentByMass", "PercentByVolume")]
        public double Density { get; set; }
        public double PercentByVolume { get; set; }
        public double PercentByMass { get; set; }
        public string Comment { get; set; }

        public void Calculate()
        {
            Calculate(string.Empty);
        }

        public void Calculate(string source)
        {
            try
            {
                if (IsCalculating) return;
                IsCalculating = true;

                if (string.IsNullOrEmpty(source))
                {
                    if (Mass == 0.0 && Volume != 0.0)
                        source = "Volume";
                    else //if (Mass != 0.0 && Volume == 0.0)
                        source = "Mass";
                }

                switch (source)
                {
                    case "Mass":
                        if (Density != 0.0)
                            Volume = Mass / Density;
                        else
                            Volume = 0.0;
                        break;
                    case "Volume":
                        Mass = Density * Volume;
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                IsCalculating = false;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
