using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Ass1
{
    // product object class
    class Product : INotifyPropertyChanged
    {
        // instance variables
        string name;
        int id;
        int kg;
        double price;
        double subtotal;
        int added;
        public event PropertyChangedEventHandler PropertyChanged;

        // parametized constructor
        public Product(string name, int id, int kg, double price)
        {
            this.name = name;
            this.id = id;
            this.kg = kg;
            this.price = price;
        }

        // constructor
        public Product() { }

        // getters and setters
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }
        public int Kg
        {
            get { return kg; }
            set
            {
                kg = value;
                OnPropertyChanged();
            }
        }
        public double Price
        {
            get { return price; }
            set
            {
                price = value;
                OnPropertyChanged();
            }
        }
        public int Added
        {
            get { return added; }
            set
            {
                added = value;
                OnPropertyChanged();
                Subtotal = added * price;
            }
        }
        public double Subtotal
        {
            get
            { return subtotal; }
            set
            {
                subtotal = value;
                OnPropertyChanged();
            }
        }

        // property changed trigger
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
