using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamGridUpdateSummary
{
    [Serializable]
    public class ComponentItem : ObservableObject, ICloneable
    {
        [NonSerialized]
        private Formula _formula;
        public Formula Formula
        {
            get { return _formula; }
            set { SetField(ref _formula, value); }
        }

        [NonSerialized]
        private bool _isCalculating;
        public bool IsCalculating
        {
            get { return _isCalculating; }
            set { SetField(ref _isCalculating, value); }
        }

        private int _position;
        public int Position
        {
            get { return _position; }
            set { SetField(ref _position, value); }
        }

        private string _name;

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        private double _mass;
        public double Mass
        {
            get { return _mass; }
            set { SetField(ref _mass, value); }
        }

        private double _volume;
        public double Volume
        {
            get { return _volume; }
            set { SetField(ref _volume, value); }
        }

        private double _density;
        //[PropertyChanged.AlsoNotifyFor("Mass", "Volume", "PercentByMass", "PercentByVolume")]
        public double Density
        {
            get { return _density; }
            set { SetField(ref _density, value); }
        }

        private double _percentByVolume;
        public double PercentByVolume
        {
            get { return _percentByVolume; }
            set { SetField(ref _percentByVolume, value); }
        }

        private double _percentByMass;
        public double PercentByMass
        {
            get { return _percentByMass; }
            set { SetField(ref _percentByMass, value); }
        }

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set { SetField(ref _comment, value); }
        }

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
                        source = nameof(Volume);
                    else //if (Mass != 0.0 && Volume == 0.0)
                        source = nameof(Mass);
                }

                switch (source)
                {
                    case nameof(Mass):
                        if (Density != 0.0)
                            Volume = Mass / Density;
                        else if (Density == 0 && Mass > 0 && Volume > 0)
                            Density = Mass / Volume;
                        else
                            Volume = 0.0;
                        break;
                    case nameof(Volume):
                        if (Density == 0 && Mass > 0 && Volume > 0)
                            Density = Mass / Volume;
                        else
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
