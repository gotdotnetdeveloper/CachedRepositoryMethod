using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CachedRepository.Annotations;

namespace CachedRepository
{
  public  class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Guid Id { get; set; }

        public string Name { get ; set ;  }
        public string Name2 { get; set; }
        public string Name3 { get; set; }

        public DateTime Data1 { get; set; }
        public DateTime Data2 { get; set; }
        public DateTime Data3 { get; set; }
    }
}
