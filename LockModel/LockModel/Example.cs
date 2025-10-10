using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueByte.SOLIDWORKS.Extensions;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace LockModel
{
    public class Example
    {
        public static void Run()
        {
            var solidworksManager = new SOLIDWORKSInstanceManager();
            var swApp = solidworksManager.GetNewInstance("", SOLIDWORKSInstanceManager.Year_e.Latest, 160);
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

            // Scenario 1: LockModel = true
            Console.WriteLine("Starting Scenario 1 (LockModel = true)");
            MeasureSelectionTime(swModel, true);


            swModel.ClearSelection();

            Console.WriteLine("\n-----------------------------------------------\n");

            // Scenario 2: LockModel = false
            Console.WriteLine("Starting Scenario 2 (LockModel = false)");
            MeasureSelectionTime(swModel, false);

            swApp.CloseDoc(swModel.GetTitle());
            swApp.ExitApp();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void MeasureSelectionTime(ModelDoc2 swModel, bool lockModel)
        {
            var swFeature = (Feature)swModel.FirstFeature();

            var ModelView = swModel.ActiveView as ModelView;

            if (lockModel)
                swModel.Lock();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var rootItem = swModel.FeatureManager.GetFeatureTreeRootItem2((int)swFeatMgrPane_e.swFeatMgrPaneBottom);
            TraverseTreeItems(rootItem);

            stopwatch.Stop();

            if (lockModel)
                swModel.UnLock();

            Console.WriteLine($"Total Selection Time (LockModel={lockModel}): {stopwatch.ElapsedMilliseconds} ms");
        }








        public static void PrintComponentsFlat(ModelDoc2 swModel)
        {
            Console.WriteLine("Method 1: Flat List using GetComponents");
            var swAssembly = swModel as AssemblyDoc;

            if (swAssembly == null)
                return;
            var comps = swAssembly.GetComponents(false) as object[];
            foreach (Component2 comp in comps)
                Console.WriteLine(comp.Name2);
        }
        // Method 2: Recursive Hierarchical Traversal using GetFeatureTreeRootItem
        public static void TraverseTreeItems(TreeControlItem item, string indent = "")
        {
            while (item != null)
            {
                Console.WriteLine($"{indent}{item.Text}");

                var child = item.GetFirstChild();
                if (child != null)
                    TraverseTreeItems(child, indent + "  ");

                item = item.GetNext();
            }
        }





        static void TraverseFeatureForComponents(Feature swFeature, Action<Component2> performAction)
        {
            var swSubFeature = default(Feature);

            var swComponent = swFeature.GetSpecificFeature2() as Component2;
            if (swComponent != null)
            {
                performAction(swComponent);

                swSubFeature = swComponent.FirstFeature();
                while (swSubFeature != null)
                {
                    TraverseFeatureForComponents(swSubFeature, performAction);
                    swSubFeature = swSubFeature.GetNextFeature() as Feature;
                }
            }


        }

    }
}
