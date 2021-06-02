using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace TextFinde

{
    class Path
    {

        public string InputPath; //sciezka wprowadzona przez urzytkownika
        private string TextToFind; // Text do znalezienia
        private string LogPath;

        public int PathType; //rodzaj sciezki 1- plik, 2- folder, 3-błąd
        public Stack PathLists; //stos przechowujacy liste sciezek do plików które należy sprawdzić
        public Stack DirLists;
        //metoda do wprowadzenia i sprawdzenia rodzaju ściezki
        public int inPath()
        {
            Console.WriteLine("Insert text to find:\n");
            TextToFind = Console.ReadLine();

            Console.WriteLine("Insert path:\n");
            InputPath = Console.ReadLine();
            try
            {
                FileAttributes attr = File.GetAttributes(InputPath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return 1;
                else
                    return 2;
            }
            catch
            {

                return 3;
            }

        }

        public void CreateLog(string LogPathIn)
        {
            LogPath = LogPathIn + "\\LogFile.log";
            File.CreateText(LogPath);
        }
        public void exploreDir(string DirPath)
        {
            foreach (string path in Directory.GetFiles(DirPath))
            {
                if (System.IO.Path.GetExtension(path) == ".txt")
                {
                    //Console.WriteLine(path);
                    PathLists.Push(path);
                }

            }
        }
        //Metoda uzupełnia stos ścieżkami do plików
        public void explorePatch()
        {
            if (PathType == 2)
            {
                PathLists.Push(InputPath);
            }
            else if (PathType == 1)
            {
                while (DirLists.Count > 0)
                {
                    InputPath = DirLists.Pop().ToString();
                    exploreDir(InputPath);

                    foreach (string path in Directory.GetDirectories(InputPath))
                    {
                        //Console.WriteLine("test");
                        DirLists.Push(path);
                    }


                }

            }
            //Console.WriteLine(PathLists.Count.ToString());
        }

        public async Task explore_fileAsync(string path, CancellationToken ct)
        {
            int index = 0;
            Console.WriteLine(path);
            Thread.Sleep(1000);
            foreach (string line in File.ReadAllLines(path))
            {

                if (ct.IsCancellationRequested == true)
                {                  
                    Console.WriteLine("task canceled");
                    ct.ThrowIfCancellationRequested();
                    break;
                }
                else if (line.Contains(TextToFind))
                {
                    using (StreamWriter outputFile = new StreamWriter(LogPath, append: true))
                    {
                        await outputFile.WriteAsync(path + " " + "in line " + index.ToString()+"\n");
                    }
                }
                index++;
            }
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
                Path p = new Path();
                p.PathType = p.inPath();
                p.PathLists = new Stack();

                var tasks = new List<Task>();

            if (p.PathType == 3)
                {
                    Console.WriteLine("Invalid path");
                }
                else if (p.PathType == 2)
                {
                    Console.WriteLine("Start searching in file");
                    p.CreateLog(p.InputPath);
                    p.explorePatch();

                }
                else if (p.PathType == 1)
                {
                    Console.WriteLine("Start searching in folder");
                    p.CreateLog(p.InputPath);
                    p.DirLists = new Stack();
                    p.DirLists.Push(p.InputPath);
                    p.explorePatch();
                }

            int PathCount = p.PathLists.Count;
            string curPath;
            var tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;
            tokenSource.CancelAfter(1000);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < PathCount; i++)
            {
                curPath = p.PathLists.Pop().ToString();

                Task taskA  = Task.Factory.StartNew(() => 
                {

                    p.explore_fileAsync(curPath, ct);
                }, ct);
                
                tasks.Add(taskA);


                Thread.Sleep(100);
                
            }

            /*while (tasks.Count>0)
            {
                  for (int i = 0; i < tasks.Count; i++)
                  {

                    try
                    {
                        if (tasks[i].IsCompleted == true)
                        {
                            Console.WriteLine("Zadanie o ID :" + tasks[i].Id.ToString() + " status: " + tasks[i].Status);
                            tasks.Remove(tasks[i]);
                        }
                        else
                        {
                            Console.WriteLine("Zadanie o ID :" + tasks[i].Id.ToString() + " status: " + tasks[i].Status);
                        }
                        Thread.Sleep(50);
                    }
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
                    }
                    finally
                    {
                        tokenSource.Dispose();
                    }
                }


            }*/
            tokenSource.Cancel();


            try
            {
                /*while (tasks.Count > 0)
                {
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if (tasks[i].IsCompleted == true)
                        {
                            Console.WriteLine("Zadanie o ID :" + tasks[i].Id.ToString() + " status: " + tasks[i].Status);
                            tasks.Remove(tasks[i]);
                        }
                        else
                        {
                            Console.WriteLine("Zadanie o ID :" + tasks[i].Id.ToString() + " status: " + tasks[i].Status);
                        }
                    }
                    Thread.Sleep(50);
                }*/

                await Task.WhenAll(tasks.ToArray());

            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"{nameof(OperationCanceledException)} zwrócono wyjątek: {e.Message}");
            }
            finally
            {
                tokenSource.Dispose();
            }
        }
    }

}
