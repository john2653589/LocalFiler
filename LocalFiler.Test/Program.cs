
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

var Filer = new FilerService(new FilerSetting()
{
    RootPath = @"D:/Development",
});

var Folder = Filer
    .InfoFolder(Item => Item.AddPath("Github"))
    .WithMode(FolderModeType.Static);

while (true)
{
    Console.Write($"{Folder?.FolderName}:");
    var Input = Console.ReadLine();
    switch (Input?.ToLower())
    {
        case "next":
            var Next = Folder?.NextFolder(PositionByType.Name);
            if (Next is null)
                Console.WriteLine("Next is null");
            else
                Folder = Next;
            break;
        case "pre":
            var Previous = Folder?.PreviousFolder(PositionByType.Name);
            if (Previous is null)
                Console.WriteLine("Previous is null");
            else
                Folder = Previous;
            break;
        case "print":
            Console.WriteLine(Folder?.FolderName);
            break;
        case "back":
            var Back = Folder?.ParentFolder;
            Folder = Back;
            break;
        case "in":
            var First = Folder?.Folders.FirstOrDefault();
            if (First is null)
                Console.WriteLine("Folders is empty");
            else
                Folder = First;
            break;
        case "length":
            var Length = Folder?.TotalLength;
            Console.WriteLine(Length);
            break;
        default:
            break;
    }
}

