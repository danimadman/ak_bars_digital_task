using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AkBarsDigital.Models;
using AkBarsDigital.Models.Disk.Resources;
using AkBarsDigital.Models.Disk.Operations;
using Newtonsoft.Json;

namespace AkBarsDigital.Services
{
    public class YandexService : IDisposable
    {
        private HttpClient _httpClient;
        
        public YandexService(string AuthToken, string BaseUrl)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", AuthToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private async Task<object> Get(string path, Type returnType)
        {
            var response = await _httpClient.GetAsync(path).ConfigureAwait(false);
            string responseBody = await response.Content.ReadAsStringAsync();
            
            return response.IsSuccessStatusCode 
                ? JsonConvert.DeserializeObject(responseBody, returnType)
                : JsonConvert.DeserializeObject<ErrorMsg>(responseBody);
        }
        
        private async Task<bool> Put(string path, HttpContent content)
        {
            var response = await _httpClient
                .PutAsync(path, content)
                .ConfigureAwait(false);
            
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Получить ссылку для загрузки файла
        /// </summary>
        /// <param name="path">Путь к загружаемому файлу на Диске</param>
        /// <param name="fields">Список возвращаемых атрибутов</param>
        /// <param name="overwrite">Перезаписать существующий файл</param>
        /// <returns>Возвращает сгенерированный URL для загрузки файла</returns>
        /// <exception cref="Exception"></exception>
        public async Task<ResourceUploadLink> GetLinkToDownloadFile(string path, string fields = null, bool? overwrite = null)
        {
            object tmp = await Get($"v1/disk/resources/upload" +
                                   $"?path={path.Replace("/", "%2F")}" +
                                   $"{(overwrite == null ? "" : "&overwrite=" + overwrite)}" + 
                                   $"{(string.IsNullOrEmpty(fields) ? "" : "&fields=" + fields)}", 
                typeof(ResourceUploadLink));
            
            if (tmp is ResourceUploadLink uploadLink)
            {
                return uploadLink;
            }
            
            throw new Exception((tmp as ErrorMsg)?.Message ?? "Что-то пошло не так :("); 
        }

        public async Task<OperationStatus> GetOperationStatus(string operationId, string fields = null)
        {
            object tmp = await Get($"v1/disk/operations/" +
                                   $"?operation_id={operationId}" +
                                   $"{(string.IsNullOrEmpty(fields) ? "" : "&fields=" + fields)}", 
                typeof(OperationStatus));
            
            if (tmp is OperationStatus status)
            {
                return status;
            }
            
            throw new Exception((tmp as ErrorMsg)?.Message ?? "Что-то пошло не так :("); 
        }
        
        /// <summary>
        /// Загрузка файла в Диск по URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<bool> UploadFile(string url/*, string operationId*/, byte[] file)
        {
            return await Put(url, new ByteArrayContent(file));

            // OperationStatus status;
            //
            // while (true)
            // {
            //     status = await GetOperationStatus(operationId);
            //     
            //     if (status.status == "success")
            //         return true;
            //     
            //     if (status.status == "failed")
            //         return false;
            // }
            //
            // return false;
        }
    }
}