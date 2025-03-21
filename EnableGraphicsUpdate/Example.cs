using BlueByte.SOLIDWORKS.Extensions;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnableGraphicsUpdate
{
    public class Example
    {
        public static void Run()
        {
            var solidworksManager = new SOLIDWORKSInstanceManager();
            var swApp = solidworksManager.GetNewInstance("", SOLIDWORKSInstanceManager.Year_e.Year2023, 160);
            swApp.Visible = true;

            var currentLocation = (new FileInfo(typeof(Example).Assembly.Location)).Directory;
            var testFile = currentLocation.GetFiles()
                .FirstOrDefault(x => x.Name.Equals("tank_20lb_propane_&.SLDPRT", StringComparison.OrdinalIgnoreCase));

            if (testFile == null)
                throw new FileNotFoundException("Could not find 'tank_20lb_propane_&.SLDPRT' in the current directory.");

            string filePath = testFile.FullName;

            // Open the file
            int errors = 0, warnings = 0;
            var swModel = swApp.OpenDoc6(filePath,
                                         (int)swDocumentTypes_e.swDocPART,
                                         (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                                         "", ref errors, ref warnings);

            if (swModel == null)
                throw new InvalidOperationException("Failed to open the document.");

            // Scenario 1: EnableGraphicsUpdate = true
            Console.WriteLine("Starting Scenario 1 (EnableGraphicsUpdate = true)");
            MeasureSelectionTime(swModel, true);


            swModel.ClearSelection();

            Console.WriteLine("\n-----------------------------------------------\n");

            // Scenario 2: EnableGraphicsUpdate = false
            Console.WriteLine("Starting Scenario 2 (EnableGraphicsUpdate = false)");
            MeasureSelectionTime(swModel, false);

            swApp.CloseDoc(swModel.GetTitle());
            swApp.ExitApp();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void MeasureSelectionTime(ModelDoc2 swModel, bool enableGraphicsUpdate)
        {
            var swFeature = (Feature)swModel.FirstFeature();

            var ModelView = swModel.ActiveView as ModelView;
            ModelView.EnableGraphicsUpdate = enableGraphicsUpdate;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (swFeature != null)
            {
                swFeature.Select2(true, -1);
                swFeature = (Feature)swFeature.GetNextFeature();
            }

            stopwatch.Stop();

            ModelView.EnableGraphicsUpdate = true; // Reset graphics update

            Console.WriteLine($"Total Selection Time (EnableGraphicsUpdate={enableGraphicsUpdate}): {stopwatch.ElapsedMilliseconds} ms");
        }

    }
}
