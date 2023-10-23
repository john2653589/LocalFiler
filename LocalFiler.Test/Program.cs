
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

var Filer = new FilerService(new FilerSetting()
{
    RootPath = @"D:/Development",
});

var RootFolder = Filer.InfoFolder();
var e = RootFolder.IsRoot;
var c = RootFolder.Folders.ToArray().FirstOrDefault();
var D = c.TotalLength;
while (true)
{
    Console.Write($"{RootFolder?.FolderName}:");
    var Input = Console.ReadLine();
    switch (Input?.ToLower())
    {
        case "next":
            var Next = RootFolder?.NextFolder(PositionByType.Name);
            if (Next is null)
                Console.WriteLine("Next is null");
            else
                RootFolder = Next;
            break;
        case "pre":
            var Previous = RootFolder?.PreviousFolder(PositionByType.Name);
            if (Previous is null)
                Console.WriteLine("Previous is null");
            else
                RootFolder = Previous;
            break;
        case "print":
            Console.WriteLine(RootFolder?.FolderName);
            break;
        case "back":
            var Back = RootFolder?.ParentFolder;
            RootFolder = Back;
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

