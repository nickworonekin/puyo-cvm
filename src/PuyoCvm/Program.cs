using DiscUtils.Iso9660;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;

namespace PuyoCvm
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new("Create a CVM for use with Puyo Puyo! 15th Anniversary and Puyo Puyo 7.");

            Argument<FileInfo> executableArgument = new Argument<FileInfo>(
                "executable",
                "The file path to the game's executable.")
                .ExistingOnly();
            rootCommand.AddArgument(executableArgument);

            Argument<DirectoryInfo> inputArgument = new Argument<DirectoryInfo>(
                "input",
                "The path to the directory containing the files and subdirectories to be added to the CVM. By default, only files and subdirectories in this directory defined in the executable's directory listing will be added.")
                .ExistingOnly();
            rootCommand.AddArgument(inputArgument);

            Option<FileInfo> outputArgument = new Option<FileInfo>(
                new string[] { "-o", "--output" },
                () => new FileInfo("ROFS.CVM"),
                "The file path to the CVM to create.")
            {
                ArgumentHelpName = "path",
            }
                .LegalFilePathsOnly();
            rootCommand.AddOption(outputArgument);

            Option<bool> allEntriesOption = new(
                "--all-entries",
                "Add all files and subdirectories from the specified input directory to the CVM, not just those defined in the executable's directory listing.");
            rootCommand.AddOption(allEntriesOption);

            rootCommand.SetHandler(
                ExecuteCommand,
                executableArgument,
                inputArgument,
                outputArgument,
                allEntriesOption);

            Parser parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseExceptionHandler(ExceptionHandler)
                .ShowHelpOnNoTokens()
                .Build();

            return await parser.InvokeAsync(args);
        }

        static void ExceptionHandler(Exception e, InvocationContext context)
        {
            // If the exception is a TargetInvocationException, use the underlying exception.
            if (e is TargetInvocationException
                && e.InnerException is not null)
            {
                e = e.InnerException;
            }

            Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
            Console.Error.WriteLine(e);
#else
            Console.Error.WriteLine(e.Message);
#endif
            Console.ResetColor();

            context.ExitCode = e.HResult;
        }

        static void ExecuteCommand(
            FileInfo executable,
            DirectoryInfo input,
            FileInfo output,
            bool allEntries)
        {
            using FileStream executableStream = executable.Open(FileMode.Open, FileAccess.ReadWrite);

            // Identify the executable and get the directory list info.
            DirectoryListInfo? directoryListInfo = Executable.GetDirectoryListInfo(executableStream);
            if (directoryListInfo is null)
            {
                throw new DirectoryListNotFoundException("Could not find the directory listing in the executable. If using with PlayStation Portable executables, ensure the executable is decrypted.");
            }

            // Read the directory list from the executable.
            executableStream.Position = directoryListInfo.Position;
            DirectoryListReader directoryListReader = new(executableStream, directoryListInfo);

            // Verify the directories and files in the directly list can be read. If not, return an error.
            List<string> directoriesNotFound = directoryListReader.Root.EnumerateAllEntries()
                .Where(x => x is DirectoryListDirectoryEntry && !Directory.Exists(Path.Combine(input.FullName, x.FullName)))
                .Select(x => x.FullName)
                .ToList();
            if (directoriesNotFound.Any())
            {
                string directoriesNotFoundFormatted = string.Join('\n', directoriesNotFound.Select(x => $"* {x}"));
                throw new DirectoryNotFoundException($"Could not find the following sub-directories in the specified input directory:\n{directoriesNotFoundFormatted}");
            }

            List<string> filesNotFound = directoryListReader.Root.EnumerateAllEntries()
                .Where(x => x is DirectoryListFileEntry && !File.Exists(Path.Combine(input.FullName, x.FullName)))
                .Select(x => x.FullName)
                .ToList();
            if (filesNotFound.Any())
            {
                string filesNotFoundFormatted = string.Join('\n', filesNotFound.Select(x => $"* {x}"));
                throw new FileNotFoundException($"Could not find the following files in the specified input directory:\n{filesNotFoundFormatted}");
            }

            // Write the CVM.
            CvmWriter writer = new(!allEntries ? directoryListReader.Root : null, input.FullName);
            using FileStream cvmStream = output.Open(FileMode.Create, FileAccess.ReadWrite);

            writer.Write(cvmStream);

            // Now that the CVM is written, go back to the executable and update the offsets/lengths
            // in the directory listing.
            cvmStream.Position = 0;
            CvmReader cvmReader = new(cvmStream);
            using CDReader cdReader = new(cvmReader.OpenIso(), false, true);

            executableStream.Position = directoryListInfo.Position;
            DirectoryListWriter directoryListWriter = new(cdReader, directoryListReader.Root);
            directoryListWriter.Write(executableStream, directoryListInfo);
        }
    }
}