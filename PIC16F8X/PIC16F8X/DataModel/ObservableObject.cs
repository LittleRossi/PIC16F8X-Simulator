using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PIC16F8X.DataModel
{

    // ##########################################################################################
    // # We need an Observable object type as SourceLine, because we need to throw an event,    #
    // # when a new breakoint is set in the UI. Otherwise we can´t recognize a change in the    #
    // # programm while running.                                                                #
    // ##########################################################################################

    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // method to set the new Value and send a notification, that a value has changed
        protected virtual void SetAndNotify<T>(ref T field, T value, Expression<Func<T>> property)
        {
            if (!object.ReferenceEquals(field, value))
            {
                field = value;
                this.OnPropertyChanged(property);
            }
        }

        // method to create a new event with the changedProperty as name
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> changedProperty)
        {
            if (PropertyChanged != null)
            {
                string name = ((MemberExpression)changedProperty.Body).Member.Name;
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
