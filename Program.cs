using Mpx;
using System;
using System.Globalization;

namespace MpxMc3030
{
    class Program
    {
        static int Main(string[] arguments)
        {
            return new Program().Run(arguments);
        }

        int Run(string[] arguments)
        {
            try
            {
                PrintLogo();

                options.ParseArguments(arguments);
                if (!options.Validate())
                {
                    PrintUsage();
                    return 2;
                }

                using (var transmitter = new Transmitter())
                {
                    transmitter.Connect(options.Port);

                    if (options.Id)
                    {
                        transmitter.PrintId();
                    }
                    else if (options.Dump != null)
                    {
                        transmitter.DumpBlock(ParseBlock(options.Dump));
                    }
                    else if (options.Diff != null)
                    {
                        transmitter.DiffBlock(ParseBlock(options.Diff));
                    }
                    else if (options.Save != null)
                    {
                        var filename = options.Save;
                        if (options.Memory != null)
                        {
                            transmitter.SaveMemoryToFile(filename, ParseBlock(options.Memory));
                        }
                        else if (options.Block != null)
                        {
                            var block = ParseBlock(options.Block);
                            transmitter.SaveBlocksToFile(filename, block, block);
                        }
                        else
                        {
                            var firstBlock = options.FirstBlock != null ? ParseBlock(options.FirstBlock) : 0;
                            var lastBlock = options.LastBlock != null ? ParseBlock(options.LastBlock) : 127;
                            transmitter.SaveBlocksToFile(filename, firstBlock, lastBlock);
                        }
                    }
                    else if (options.Load != null)
                    {
                        var filename = options.Load;
                        if (options.Memory != null)
                        {
                            transmitter.LoadMemoryFromFile(filename, ParseBlock(options.Memory));
                        }
                        else
                        {
                            transmitter.LoadBlocksFromFile(filename);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid command-line arguments.");
                        return 2;
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                using (var coloredConsole = new ColoredConsole(ConsoleColor.Red))
                {
                    Console.Error.WriteLine($"ERROR: {e.Message}");
                }

                return 1;
            }
        }

        void PrintLogo()
        {
            Console.WriteLine("MPX MC3030 Tool, V1.0");
            Console.WriteLine("Copyright (C) 2018 Marius Greuel. All rights reserved.");
        }

        void PrintUsage()
        {
            Console.WriteLine("Usage: mpx-mc3030 [@response-file] [options] <files>");
            options.WriteUsage();
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    Print device ID: mpx-mc3030 -port COM3 -id");
            Console.WriteLine("    Save model memory #1 to file: mpx-mc3030 -port COM3 -memory 1 -save mc3030_model_1.mdl");
            Console.WriteLine("    Load model memory #1 from file: mpx-mc3030 -port COM3 -memory 1 -load mc3030_model_1.mdl");
            Console.WriteLine("    Dump info block 0x73: mpx-mc3030 -port COM3 -dump 0x73");
            Console.WriteLine("    Backup data memory to file: mpx-mc3030 -port COM3 -first 0x70 -last 0x7F -save mc3030_dataset.ds3");
            Console.WriteLine("    Restore data memory from file: mpx-mc3030 -port COM3 -load mc3030_text.ds3");
            Console.WriteLine("    Backup entire memory to file: mpx-mc3030 -port COM3 -save mc3030_full_backup.mc3all");
            Console.WriteLine("    Restore entire memory from file: mpx-mc3030 -port COM3 -load mc3030_full_backup.mc3all");
        }

        static uint ParseBlock(string block)
        {
            if (block.StartsWith("$"))
            {
                return uint.Parse(block.Substring(1), NumberStyles.HexNumber);
            }
            else if (block.StartsWith("0x") || block.StartsWith("0X") || block.StartsWith("&h") || block.StartsWith("&H"))
            {
                return uint.Parse(block.Substring(2), NumberStyles.HexNumber);
            }
            else
            {
                return uint.Parse(block);
            }
        }

        readonly Options options = new Options();
    }
}
