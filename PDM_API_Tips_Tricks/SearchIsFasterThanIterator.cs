using EPDM.Interop.epdm;
using System;
using System.Diagnostics;

namespace PDM_API_Tips_Tricks
{
    internal partial class Program
    {
        public static class SearchIsFasterThanIterator
    {
            public static void Execute()
            {

                Console.WriteLine("Connecting to PDM Vault...");



                _vault.LoginAuto("Assemblageddon", 0);

                Console.WriteLine("Connected to Vault: " + _vault.Name);


                CompareTraversalMethods("C:\\Assemblageddon");


                Console.WriteLine("Done. Press any key to exit.");

            }
            static IEdmVault5 _vault = new EdmVault5();

            public static void CompareTraversalMethods(string folderPath)
            {
                // Resolve the folder
                IEdmFolder5 targetFolder = _vault.GetFolderFromPath(folderPath);
                if (targetFolder == null)
                {
                    Console.WriteLine("Folder not found: " + folderPath);
                    return;
                }

                Console.WriteLine("Comparing folder traversal methods in: " + folderPath);

                // Method 1: Iterator (GetFirstFilePosition / GetNextFile)
                TraverseWithIterator(targetFolder);

                // Method 2: Search (IEdmSearch5)
                TraverseWithSearch(folderPath);
            }

            private static int TraverseWithIterator(IEdmFolder5 folder, bool isTopLevel = true)
            {
                Stopwatch sw = null;
                if (isTopLevel)
                    sw = Stopwatch.StartNew();


                // first found file is not counted
                int count = 0;

                // Count files in current folder
                IEdmPos5 filePos = folder.GetFirstFilePosition();
                while (!filePos.IsNull)
                {
                    IEdmFile5 file = folder.GetNextFile(filePos);
                    if (file != null)
                        count++;
                }

                // Recursively process subfolders
                IEdmPos5 subfolderPos = folder.GetFirstSubFolderPosition();
                while (!subfolderPos.IsNull)
                {
                    IEdmFolder5 subfolder = folder.GetNextSubFolder(subfolderPos);
                    count += TraverseWithIterator(subfolder, false);
                }

                if (isTopLevel && sw != null)
                {
                    sw.Stop();
                    Console.WriteLine($"[Iterator] Found {count} files in {sw.ElapsedMilliseconds} ms");
                }

                return count;
            }


            private static void TraverseWithSearch(string folderPath)
            {
                int count = 1;


                IEdmSearch5 search = (IEdmSearch5)(_vault as IEdmVault7).CreateUtility(EdmUtility.EdmUtil_Search);
                search.FindFiles = true;
                search.FileName = "%";
                search.Recursive = true;
                search.FindFolders = false;
                search.StartFolderID = _vault.GetFolderFromPath(folderPath).ID;
                Stopwatch swSearch = Stopwatch.StartNew();

                IEdmSearchResult5 result = search.GetFirstResult();



                while ((result = search.GetNextResult()) != null)
                {
                    count++;
                }

                swSearch.Stop();
                Console.WriteLine($"[Search] Found {count} files in {swSearch.ElapsedMilliseconds} ms");

            }
        }
    }
}
