
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

var Filer = new FilerService(new FilerSetting()
{
    //RootPath = @"D:/Development",
    RootPath = "C:/"
});

var RootFolder = Filer
    .InfoFolder()
    .WithSort(SortByType.Length);

var F = RootFolder.Folders.FirstOrDefault();

while (true)
{
    Console.Write($"{RootFolder?.FolderName}:");
    var Input = Console.ReadLine();
    switch (Input?.ToLower())
    {
        case "next":
            //var Next = RootFolder?.NextFolder();
            var Next = Filer.RCS_ToNextFolder(RootFolder);

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
        default:
            break;

    }
}