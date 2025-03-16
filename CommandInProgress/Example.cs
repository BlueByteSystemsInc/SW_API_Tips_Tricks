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

namespace CommandInProgress
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
                throw new FileNotFoundException("Could not find 'test.sldprt' in the current directory.");

            string filePath = testFile.FullName;

            // Scenario 1: CommandInProgress = True
            Console.WriteLine("Starting Scenario 1 (CommandInProgress = True)");
            swApp.CommandInProgress = true;
            MeasureTraversalTime(swApp, filePath);
            swApp.CommandInProgress = false;

            Console.WriteLine("\n-----------------------------------------------\n");

            // Scenario 2: CommandInProgress = False
            Console.WriteLine("Starting Scenario 2 (CommandInProgress = False)");
            MeasureTraversalTime(swApp, filePath);

            swApp.ExitApp();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void MeasureTraversalTime(SldWorks swApp, string filePath)
        {
            ModelDoc2 swModel = swApp.OpenDoc(filePath, (int)swDocumentTypes_e.swDocPART) as ModelDoc2;

            if (swModel == null)
                throw new Exception($"Failed to open the file: {filePath}");

            Console.WriteLine($"Opened: {swModel.GetTitle()}");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            TraverseAllFeatures(swModel);

            stopwatch.Stop();

            Console.WriteLine($"\nTraversal completed in: {stopwatch.ElapsedMilliseconds} ms\n");

            swApp.QuitDoc(swModel.GetTitle());
        }

        static void TraverseAllFeatures(ModelDoc2 swModel)
        {
            Feature swFeature = swModel.FirstFeature() as Feature;

            while (swFeature != null)
            {
                TraverseFeature(swFeature, 0);
                swFeature = swFeature.GetNextFeature() as Feature;
            }
        }

        static void TraverseFeature(Feature feature, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            Console.WriteLine($"{indent}- Feature: {feature.Name} [{feature.GetTypeName()}]");

            Feature subFeature = feature.GetFirstSubFeature() as Feature;
            while (subFeature != null)
            {
                TraverseFeature(subFeature, indentLevel + 1);
                subFeature = subFeature.GetNextSubFeature() as Feature;
            }
        }
    }

 
}
