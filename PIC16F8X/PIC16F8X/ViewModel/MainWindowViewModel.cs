using PIC16F8X.DataModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace PIC16F8X.ViewModel
{
    class MainWindowViewModel: ObservableObject
    {


        // SFR Values Property
        private ObservableCollection<string> sfrValues;
        public ObservableCollection<string> SFRValues
        {
            get { return sfrValues; }
            set { SetAndNotify(ref sfrValues, value, () => SFRValues); }
        }





        public MainWindowViewModel()
        {
            sfrValues = new ObservableCollection<string>(Enumerable.Repeat("00", 10).ToList());
        }
    }
}
