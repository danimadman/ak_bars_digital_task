using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AkBarsDigital.Options;
using AkBarsDigital.Services;

namespace AkBarsDigital
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new[] { "D:\\Test", "disk:/Test" }; // Для тестов
            
            if (args.Length == 2)
            {
                UploadFilesAsync(args[0], args[1]);
            }
            else
            {
                Console.WriteLine("Необходимо 2 параметра командой строки:");
                Console.WriteLine("\t1 параметр - директория, откуда надо выгружать файлы");
                Console.WriteLine("\t2 параметр - директория в Яндекс диске, куда надо закгрузить файлы");
            }

            Console.ReadKey();
        }

        private static void UploadFilesAsync(string input, string output)
        {
            var filePaths = Directory.GetFiles(input);

            if (filePaths.Any())
            {
                var service = new YandexService(YandexOptions.Token, YandexOptions.BaseUrl);

                try
                {
                    Console.WriteLine("Список файлов:");

                    filePaths.ToList()
                        .ForEach(filePath => 
                        {
                            Console.WriteLine(Path.GetFileName(filePath));
                        });
            
                    Console.WriteLine("");

                    Parallel.ForEach(filePaths, filePath =>
                    {
                        string fileName = Path.GetFileName(filePath);
                        string yandexFilePath = $"{output}/{fileName}";
                    
                        Console.WriteLine($"Получение ссылки на загрузку {fileName}.");
                    
                        var uploadLink = service.GetLinkToDownloadFile(yandexFilePath, overwrite: true);
                    
                        byte[] file = File.ReadAllBytes(filePath);
                    
                        Console.WriteLine($"Загрузка {fileName} на Диск.");
                    
                        if (service.UploadFile(uploadLink.Result.href/*, uploadLink.Result.operation_id*/, file).Result)
                            Console.WriteLine($"{fileName} загружен.");
                        else 
                            Console.WriteLine($"Не удалось загрузить {fileName}.");
                    });

                    // void UploadFileAsync(string filePath)
                    // {
                    //     string fileName = Path.GetFileName(filePath);
                    //     string yandexFilePath = $"{output}/{fileName}";
                    //
                    //     Console.WriteLine($"Получение ссылки на загрузку {fileName}.");
                    //
                    //     var uploadLink = service.GetLinkToDownloadFile(yandexFilePath, overwrite: true);
                    //
                    //     byte[] file = File.ReadAllBytes(filePath);
                    //
                    //     Console.WriteLine($"Загрузка {fileName} в Диск.");
                    //
                    //     if (service.UploadFile(uploadLink.Result.href, uploadLink.Result.operation_id, file).Result)
                    //     {
                    //         Console.WriteLine($"{fileName} загружен.");
                    //     }
                    //     else Console.WriteLine($"Не удалось загрузить {fileName}.");
                    // }
                    //
                    // Task[] tasks = filePaths.Select(x => Task.Run(() => UploadFileAsync(x))).ToArray();
                    //
                    // foreach (var task in tasks)
                    //     task.Start();
                    //
                    // Task.WaitAll(tasks);

                    Console.WriteLine("Выгрузка завершена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    service.Dispose();
                }
            }
            else Console.WriteLine("Нет файлов для выгрузки.");
        }
    }
}