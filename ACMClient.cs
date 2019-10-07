using Aliyun.ACM.Sdk.Model;
using Aliyun.Acs.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aliyun.ACM.Sdk
{
    /// <summary>
    /// 
    /// </summary>
    public class ACMClient : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly string endpoint;
        private readonly HttpClient _net;
        private readonly HmacSHA1Signer _hmacSHA1 = new HmacSHA1Signer();
        private readonly string _accessKey;
        private readonly string _secretKey;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="net"></param>
        /// <param name="accessKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="_endpoint"></param>
        public ACMClient(HttpClient net, string accessKey, string secretKey, string _endpoint = "acm.aliyun.com")
        {
            endpoint = _endpoint;
            _net = net ?? throw new ArgumentNullException(nameof(net));
            _accessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey));
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="signString"></param>
        private void AddHeader(HttpRequestMessage request, string signString)
        {
            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            request.Headers.Add("Spas-AccessKey", _accessKey);
            request.Headers.Add("timeStamp", $"{timeStamp}");
            request.Headers.Add("Spas-Signature", _hmacSHA1.SignString($"{signString}+{timeStamp}", _secretKey));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetIPAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"http://{endpoint}:8080/diamond-server/diamond");
            var resp = await _net.SendAsync(req);
            var ip = await resp.Content.ReadAsStringAsync();
            return ip.Trim();
        }
        /// <summary>
        /// 获取 ACM 配置
        /// </summary>
        /// <param name="tenant">租户信息，对应 ACM 的命名空间 ID</param>
        /// <param name="dataId">配置的 ID</param>
        /// <param name="group">配置的分组</param>
        /// <returns></returns>
        public async Task<string> GetConfigAsync(string tenant, string dataId, string group)
        {
            var ip = await GetIPAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://{ip}:8080/diamond-server/config.co?dataId={dataId}&group={group}&tenant={tenant}");
            AddHeader(request, $"{tenant}+{group}");
            request.Content = new StringContent("", encoding: Encoding.UTF8);
            var response = await _net.SendAsync(request);
            var respText = await response.Content.ReadAsStringAsync();
            return respText;
        }
        /// <summary>
        /// 获取指定命名空间内的 ACM 配置信息
        /// </summary>
        /// <param name="tenant">租户信息，对应 ACM 的命名空间 ID</param>
        /// <param name="pageNo">分页页号</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        public async Task<ACMConfig> GetAllConfigByTenantAsync(string tenant, int pageNo, int pageSize = 20)
        {
            var ip = await GetIPAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://{ip}:8080/diamond-server/basestone.do?method=getAllConfigByTenant&tenant={tenant}&pageNo={pageNo}&pageSize={pageSize}");
            AddHeader(request, $"{tenant}");
            request.Content = new StringContent("", encoding: Encoding.UTF8);
            var response = await _net.SendAsync(request);
            var respText = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ACMConfig>(respText);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenant">租户信息，对应 ACM 的命名空间 ID</param>
        /// <param name="dataId">配置的 ID</param>
        /// <param name="group">配置的分组</param>
        /// <param name="content">配置的内容</param>
        /// <returns></returns>
        public async Task<string> SyncUpdateAllAsync(string tenant, string dataId, string group, string content)
        {
            var ip = await GetIPAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{ip}:8080/diamond-server/basestone.do?method=syncUpdateAll");
            AddHeader(request, $"{tenant}+{group}");
            request.Content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("tenant",tenant),
                new KeyValuePair<string,string>("dataId",dataId),
                new KeyValuePair<string,string>("group",group),
                new KeyValuePair<string,string>("content",content),
            });
            var response = await _net.SendAsync(request);
            var respText = await response.Content.ReadAsStringAsync();
            return respText;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="dataId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<string> DeleteAllDatumsAsync(string tenant, string dataId, string group)
        {
            var ip = await GetIPAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{ip}:8080/diamond-server/datum.do?method=deleteAllDatums");
            AddHeader(request, $"{tenant}+{group}");
            request.Content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("tenant",tenant),
                new KeyValuePair<string,string>("dataId",dataId),
                new KeyValuePair<string,string>("group",group),
            });
            var response = await _net.SendAsync(request);
            var respText = await response.Content.ReadAsStringAsync();
            return respText;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_net != null)
                _net.Dispose();
        }
    }
}
