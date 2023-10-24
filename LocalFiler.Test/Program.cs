
using Rugal.LocalFiler.Model;
using Rugal.LocalFiler.Service;

var Filer = new FilerService(new FilerSetting()
{
    //RootPath = @"D:/Development",
    RootPath = "D:\\Development\\R"
});

var RootFolder = Filer
    .InfoFolder()
    .WithSort(SortByType.Length);

var GetFile = RootFolder.Files.First();
while (GetFile is not null)
{
    Console.WriteLine(GetFile.FileName);
    GetFile = Filer.RCS_ToNextFile(GetFile);
}

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