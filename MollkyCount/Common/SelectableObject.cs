using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MollkyCount.Common
{
    public class SelectableObject<T> : INotifyPropertyChanged
    {
        private T _item;
        public T Item 
        {
            get { return _item; }
            set
            {
                _item = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Item"));
            }   
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
            }   
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
