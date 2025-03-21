using BlueByte.SOLIDWORKS.Extensions;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnableFeatureTree
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

            int errors = 0, warnings = 0;
            var swModel = swApp.OpenDoc6(filePath,
                                         (int)swDocumentTypes_e.swDocPART,
                                         (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                                         "", ref errors, ref warnings);

            if (swModel == null)
                throw new InvalidOperationException("Failed to open the document.");

            var swFeatureMgr = swModel.FeatureManager;

            // Scenario 1: EnableFeatureTree = true
            Console.WriteLine("Starting Scenario 1 (EnableFeatureTree = true)");
            MeasureSelectionTime(swModel, swFeatureMgr, true);

            Console.WriteLine("\n-----------------------------------------------\n");

            // Scenario 2: EnableFeatureTree = false
            Console.WriteLine("Starting Scenario 2 (EnableFeatureTree = false)");
            MeasureSelectionTime(swModel, swFeatureMgr, false);

            swApp.CloseDoc(swModel.GetTitle());
            swApp.ExitApp();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void MeasureSelectionTime(ModelDoc2 swModel, FeatureManager swFeatureMgr, bool enableFeatureTree)
        {
            // Set EnableFeatureTree property
            swFeatureMgr.EnableFeatureTree = enableFeatureTree;

            // Clear any existing selection
            swModel.ClearSelection2(true);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var swFeature = (Feature)swModel.FirstFeature();

            while (swFeature != null)
            {
                swFeature.Select2(true, -1);
                swFeature = (Feature)swFeature.GetNextFeature();
            }

            stopwatch.Stop();

            // Reset the property to true to restore default behavior
            swFeatureMgr.EnableFeatureTree = true;

            Console.WriteLine($"Total Selection Time (EnableFeatureTree={enableFeatureTree}): {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
