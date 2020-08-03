using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace XamGridUpdateSummary
{
    [Serializable]
    public class ComponentItem : ObservableObject, ICloneable, INotifyDataErrorInfo
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

        [Required]
        [Range(double.Epsilon, double.MaxValue)]
        public double Mass
        {
            get { return _mass; }
            set { SetFieldAndValidate(ref _mass, value); }
        }

        private double _volume;

        [Required]
        [Range(double.Epsilon, double.MaxValue)]
        public double Volume
        {
            get { return _volume; }
            set { SetFieldAndValidate(ref _volume, value); }
        }

        private double _density;

        [Required]
        [Range(double.Epsilon, double.MaxValue)]
        public double Density
        {
            get { return _density; }
            set { SetFieldAndValidate(ref _density, value); }
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

        private void SetFieldAndValidate<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (SetField(ref field, value, propertyName))
                Validate(this, new PropertyChangedEventArgs(propertyName));
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

                if (string.IsNullOrEmpty(source) || source == nameof(Density))
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

        #region INotifyDataErrorInfo implementation
        private ValidationContext validationContext;
        private List<ValidationResult> validationResults;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public bool HasErrors
        {
            get { return validationResults?.Count > 0; }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return validationResults?.Where(x => x.MemberNames.Contains(propertyName))
                                    ?.Select(x => x.ErrorMessage)
                ?? Enumerable.Empty<string>();
        }

        private void Validate(object sender, PropertyChangedEventArgs e)
        {
            if (validationContext == null)
                validationContext = new ValidationContext(this, null, null);
            if (validationResults == null)
                validationResults = new List<ValidationResult>();
            else
                validationResults.Clear();
            Validator.TryValidateObject(this, validationContext, validationResults, true);
            var hashSet = new HashSet<string>(validationResults.SelectMany(x => x.MemberNames));
            foreach (var error in hashSet)
            {
                RaiseErrorsChanged(error);
            }
        }
        #endregion
    }
}
