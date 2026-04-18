using System;

namespace OOPHomework
{
    class Building
    {
        public double Height { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public int Floors { get; set; }
        public bool HasBasement { get; set; }

        public Building(double h, double w, double l, int f, bool b)
        {
            Height = h;
            Width = w;
            Length = l;
            Floors = f;
            HasBasement = b;
        }

        public double GetTotalArea()
        {
            int totalFloors = HasBasement ? Floors + 1 : Floors;
            return Width * Length * totalFloors;
        }

        public void AddFloor()
        {
            Floors++;
        }

        public void RemoveFloor()
        {
            if (Floors > 1)
            {
                Floors--;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Building myHouse = new Building(10, 15, 20, 2, true);

            Console.WriteLine($"Initial Floors: {myHouse.Floors}");
            Console.WriteLine($"Total Area: {myHouse.GetTotalArea()} sq m");

            myHouse.AddFloor();
            Console.WriteLine($"After adding a floor: {myHouse.Floors}");
            Console.WriteLine($"New Total Area: {myHouse.GetTotalArea()} sq m");

            myHouse.RemoveFloor();
            Console.WriteLine($"After removing a floor: {myHouse.Floors}");

            Console.ReadKey();
        }
    }
}
