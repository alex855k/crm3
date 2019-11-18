using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testconsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(
            System.Globalization.CultureInfo.CurrentCulture
            + "\n"
            + Guid.Empty.ToString()
            );
            Console.Read();
        }
    }
}
