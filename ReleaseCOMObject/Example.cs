using BlueByte.SOLIDWORKS.Extensions;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseCOMObject
{
    public class Example
    {
        public static void Run()
        {
            var swApp = System.Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as SldWorks;
            swApp.Visible = true;
            Console.WriteLine($"Opened SW. ProcessID {swApp.GetProcessID()}");
            Console.WriteLine($"Closing SW...");
            swApp.ExitApp();
            // missing: System.Runtime.InteropServices.Marshal.ReleaseComObject(swApp);
            Console.WriteLine($"Close SW.");
            Console.WriteLine("My app is still running...");
            Console.ReadLine();
        }
    }
}
