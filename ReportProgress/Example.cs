using BlueByte.SOLIDWORKS.Extensions;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ReportProgress
{
    public class Example
    {
       
        public static void Run()
        {

            // Launch SOLIDWORKS Instance
            var solidworksManager = new SOLIDWORKSInstanceManager();
            var swApp = solidworksManager.GetNewInstance("/m", SOLIDWORKSInstanceManager.Year_e.Year2023, 160);
            swApp.Visible = true;

            // Locate and open the test file
            var currentLocation = new FileInfo(typeof(Example).Assembly.Location).Directory;
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

            bool repeat = true;

            while (repeat)
            {
                // Perform the long operation with progress bar
                PerformOperationWithProgress(swApp, swModel);

                // Cleanup
                swApp.CloseDoc(swModel.GetTitle());

                Console.WriteLine("\nOperation completed.");

                // Ask the user if they want to repeat
                Console.Write("Do you want to repeat the process? (Y/N): ");
                var input = Console.ReadLine();

                repeat = input?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true;

                if (repeat)
                {
                    // Reopen or reinitialize your document/model if necessary
                    swModel = swApp.ActiveDoc as ModelDoc2; // or reopen as needed

                }
            }

            // Exit SOLIDWORKS
            swApp.ExitApp();

            Console.WriteLine("\nExiting the program. Press any key to close...");
            Console.ReadKey();
        }

        private static void PerformOperationWithProgress(SldWorks swApp, ModelDoc2 swModel)
        {
            
            swApp.GetUserProgressBar(out UserProgressBar swPrgBar);

            int maxProgress = CalculateMaxProgress();
            
            swPrgBar.Start(0, maxProgress, "Performing face operations...");

            int progressCounter = 0;

           
            for (int i = 0; i < maxProgress; i++)
            {
                progressCounter++;

                swPrgBar.UpdateTitle($"Counting {progressCounter}...");


                if (swPrgBar.UpdateProgress(progressCounter) == (int)swUpdateProgressError_e.swUpdateProgressError_UserCancel)
                {
                    int userResponse = swApp.SendMsgToUser2(
                        "Would you like to cancel this time consuming operation?",
                        (int)swMessageBoxIcon_e.swMbQuestion,
                        (int)swMessageBoxBtn_e.swMbYesNo);

                    if (userResponse == (int)swMessageBoxResult_e.swMbHitYes)
                     {
                        swPrgBar.End();
                        Console.WriteLine("Operation canceled by user.");
                        return;
                    }
                }

            }


            swPrgBar.End();
            Console.WriteLine("Operation completed successfully.");
        }

        private static int CalculateMaxProgress()
        {
            return 100000;
        }
    }
}
