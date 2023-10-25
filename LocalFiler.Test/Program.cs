
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

var Filer = new FilerService(new FilerSetting()
{
    //RootPath = @"D:/Development",
    RootPath = "D:/"
});


var RootFolder = Filer.InfoFolder();
while (true)
{
    Console.Write($"{RootFolder?.FolderName}:");
    var Input = Console.ReadLine();
    switch (Input?.ToLower())
    {
        case "next":
            var Next = RootFolder?.NextFolder();
            //var Next = Filer.RCS_ToNextFolder(RootFolder);

            if (Next is null)
                Console.WriteLine("Next is null");
            else
                RootFolder = Next;
            break;
        case "pre":
            var Previous = RootFolder?.PreviousFolder();
            if (Previous is null)
                Console.WriteLine("Previous is null");
            else
                RootFolder = Previous;
            break;
        case "print":
            Console.WriteLine(RootFolder?.FolderName);
            break;
        case "back":
            var ParentFolder = RootFolder?.ParentFolder;
            if (ParentFolder is null)
                Console.WriteLine("Already at root");
            else
                RootFolder = ParentFolder;
            break;
        case "in":
            var First = RootFolder?.Folders.FirstOrDefault();
            if (First is null)
                Console.WriteLine("Folders is empty");
            else
                RootFolder = First;
            break;
        case "length":
            var Length = RootFolder?.TotalLength;
            Console.WriteLine(Length);
            break;
        case "files":
            var Idx = 1;
            foreach (var File in RootFolder.Files)
            {
                Console.WriteLine($"{Idx} - {File.FileName}");
                Idx++;
            }
            break;
        case "rename-file":
            Console.Write("Old file name:");
            var OldFileName = Console.ReadLine();
            Console.Write("New file name:");
            var NewFileName = Console.ReadLine();
            if (OldFileName is null || NewFileName is null)
                continue;
            var FindFile = Filer.RCS_FindToFile(RootFolder, Item => Item.WithFileName(OldFileName));
            if (FindFile is null)
            {
                Console.WriteLine($"{OldFileName} is not found");
                continue;
            }
            Filer.ReNameFile(FindFile, NewFileName);
            Console.WriteLine($"ReName file success to {NewFileName}");
            break;
        case "rename-folder":
            Console.Write("Old folder name:");
            var OldFolderName = Console.ReadLine();
            Console.Write("New folder name:");
            var NewFolderName = Console.ReadLine();
            if (OldFolderName is null || NewFolderName is null)
                continue;

            var FindFolder = Filer.RCS_FindToFolder(RootFolder, Item => Item
                .WithConfig(RootFolder?.Config)
                .AddPath(OldFolderName));

            if (FindFolder is null)
            {
                Console.WriteLine($"{OldFolderName} is not found");
                continue;
            }
            Filer.ReNameFolder(FindFolder, NewFolderName);
            Console.WriteLine($"ReName folder success to {NewFolderName}");
            break;
        case "requery-all":
            RootFolder?.ReQuery();
            Console.WriteLine("ReQuery all success");
            break;
        case "requery-file":
            RootFolder?.ReQueryFile();
            Console.WriteLine("ReQuery file success");
            break;
        case "requery-folder":
            RootFolder?.ReQueryFolder();
            Console.WriteLine("ReQuery folder success");
            break;
        default:
            break;

    }
}