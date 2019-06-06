using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heros
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new IFDSSolverTest();
            test.before();
            test.happyPath();


            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
