using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string caller = null)
        {
            if (PropertyChanged != null) {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(caller));
            }
        }            

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string caller = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(caller);
            return true;
        }
    }
}
