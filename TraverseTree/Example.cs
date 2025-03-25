using BlueByte.SOLIDWORKS.Extensions;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraverseTree
{
    public class Example
    {
        public static void Run()
        {
            var solidworksManager = new SOLIDWORKSInstanceManager();
            var swApp = solidworksManager.GetNewInstance("", SOLIDWORKSInstanceManager.Year_e.Year2023, 160);
            swApp.Visible = true;

            var currentLocation = (new FileInfo(typeof(Example).Assembly.Location)).Directory;
          
            var assemblyFile = currentLocation.GetDirectories().First(x=> x.Name.Equals("Assembly")).GetFiles()
              .FirstOrDefault(x => x.Name.Equals("_fidget spinner.SLDASM", StringComparison.OrdinalIgnoreCase));

           
            if (assemblyFile == null)
                throw new FileNotFoundException($"Could not find '{assemblyFile.Name}' in the assembly directory.");

            string filePath = assemblyFile.FullName;

            int errors = 0, warnings = 0;
            var swModel = swApp.OpenDoc6(filePath,
                                         (int)swDocumentTypes_e.swDocASSEMBLY,
                                         (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                                         "", ref errors, ref warnings);


            
            if (swModel == null)
                throw new InvalidOperationException("Failed to open the document.");

         
          


            Console.WriteLine("\n--- Method 1: Flat List using GetComponents ---");
            PrintComponentsFlat(swModel);

            Console.WriteLine("\n--- Method 2: Traverse Assembly Components In Displayed Order---");
            PrintComponentsInDisplayedOrder(swModel);

            Console.WriteLine("\n--- Method 3: Recursive Hierarchical Traversal ---");
            var rootItem = swModel.FeatureManager.GetFeatureTreeRootItem2((int)swFeatMgrPane_e.swFeatMgrPaneBottom);
            TraverseTreeItems(rootItem);


            swApp.CloseDoc(swModel.GetTitle());
            swApp.ExitApp();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void PrintComponentsInDisplayedOrder(ModelDoc2 swRootAssemblyModelDoc)
        {
           Action<Component2> action = LogComponentName;

            var swFeature = swRootAssemblyModelDoc.FirstFeature() as Feature;
            while (swFeature != null)
            {
                TraverseFeatureForComponents(swFeature, action);
                swFeature = swFeature.GetNextFeature() as Feature;

            }


           
        }
     

        // Method 1: Flat List using GetComponents
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
        static void LogComponentName(Component2 swComponent)
        {
            // this code is not performant  
            int parentCount = 0;
            Component2 swParentComponent;
            swParentComponent = swComponent.GetParent();
            while (swParentComponent != null)
            {
                parentCount++;
                swParentComponent = swParentComponent.GetParent();
            }
            string indentation = string.Join(string.Empty, Enumerable.Repeat(" ", parentCount));
            Console.WriteLine($"{indentation}{swComponent.Name}");
        }
       
           
    }

    
}
