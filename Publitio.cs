using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.IO;

namespace Publitio
{
    /**
     * <summary>This class is the main interface to the Publitio API.
     * For the most up-to-date and complete documentation, see https://publit.io/docs.
     * Note that this library has a dependency on <c>Newtonsoft.Json</c>, and some of the public
     * methods return instances of <c>Newtonsoft.Json.Linq.JObject</c>.</summary>
     */
    public class PublitioApi
    {
        private readonly string key;
        private readonly string secret;
        private readonly HttpClient client;
        private readonly Random random = new Random();
        private readonly SHA1 sha1 = new SHA1CryptoServiceProvider();

        /**
         * <summary>Create a PublitioApi object with the given API key and secret</summary>
         *
         * <param name="key">Your API key, which you may find on your Publitio dashboard at
         * https://publit.io/dashboard.</param>
         * <param name="secret">Your API secret, which you may find on your Publitio dashboard at
         * https://publit.io/dashboard.</param>
         */
        public PublitioApi(string key, string secret)
        {
            this.key = key;
            this.secret = secret;
            client = new HttpClient
            {
                BaseAddress = new Uri("https://api.publit.io/v1/"),
            };
        }

        /**
         * Make a GET request to the specified URI, with the specified query parameters.
         * <param name="uri">The target URI. It should be relative, such as
         * "/files/info/<fileId>" or "files/info/<fileId>".</param>
         * <param name="query">The query parameters to be encoded into the URL. This will commonly be
         * an instance of <c>Dictionary<string, object></c>.</param>
         *
         * <code>
         * await publitioApi.GetAsync("/files/list");
         * await publitioApi.GetAsync("files/list", new Dictionary<string, object>{ ["limit"] = 2 });
         * </code>
         */
        public Task<JObject> GetAsync(string uri, IEnumerable<KeyValuePair<string, object>> query) =>
            ApiCallAsync(HttpMethod.Get, uri, query);

        /**
         * Make a PUT request to the specified URI, with the specified query parameters.
         * <param name="uri">The target URI. It should be relative, such as
         * "/files/update/<fileId>" or "files/update/<fileId>".</param>
         * <param name="query">The query parameters to be encoded into the URL. This will commonly be
         * an instance of <c>Dictionary<string, object></c>.</param>
         *
         * <code>
         * await publitioApi.PutAsync("files/update/MvHX8Zx5", new Dictionary<string, object>{ ["title"] = "x" });
         * </code>
         */
        public Task<JObject> PutAsync(string uri, IEnumerable<KeyValuePair<string, object>> query = null) =>
            ApiCallAsync(HttpMethod.Put, uri, query);

        /**
         * <summary>Make a DELETE request to the specified URI.</summary>
         * <param name="uri">The target URI. It should be relative,
         * such as "/files/delete/<fileId>" or "files/delete/<fileId>".</param>
         *
         * <code>
         * await publitioApi.DeleteAsync("files/delete/MvHX8Zx5");
         * </code>
         */
        public Task<JObject> DeleteAsync(string uri) => ApiCallAsync(HttpMethod.Delete, uri);

        /**
         * <summary>Upload a file to the given URL.
         * This method is used to upload watermarks and media files, depending on which URI you use.</summary>
         * 
         * <param name="uri">The target URI. It should be relative, such as
         * "/files/create" or "files/create". The only two valid URIs for this
         * method are "files/create" and "watermarks/create".</param>
         * <param name="query">The query parameters to encode into the URL.</param>
         * <param name="data">The file data, given as a <c>Stream</c>.</param>
         *
         * <code>
         * using (var f = File.OpenRead("path/to/file"))
         * {
         *     await publitioApi.UploadFileAsync("files/create", new Dictionary{ ["title"] = "x" }, f);
         * }
         * </code>
         */
        public Task<JObject> UploadFileAsync(
            string uri,
            IEnumerable<KeyValuePair<string, object>> query = null,
            Stream data = null)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(data), "file", "file");
            return ApiCallAsync(HttpMethod.Post, uri, query, content);
        }

        private async Task<JObject> ApiCallAsync(
            HttpMethod method,
            string uri,
            IEnumerable<KeyValuePair<string, object>> query = null,
            HttpContent content = null)
        {
            query = query ?? Enumerable.Empty<KeyValuePair<string, object>>();
            uri = RemoveLeadingSlashes(uri);
            uri += EncodeQuery(Enumerable.Concat(AuthQuery(), query));
            var res = await client.SendAsync(new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(uri, UriKind.Relative),
                Content = content,
            });
            var json = await res.Content.ReadAsStringAsync();

            try
            {
                return (JObject)JsonConvert.DeserializeObject(json);
            }
            catch (JsonException)
            {
                throw InvalidJsonException.ForJson(json, uri);
            }
        }

        /**
         * <summary>Turn key-value pairs into their URL-encoded equivalents.
         * Example output: "?&key1=value1&key2=value2"</summary>
         */
        private static string EncodeQuery(IEnumerable<KeyValuePair<string, object>> query)
        {
            var builder = new StringBuilder("?");
            foreach (var pair in query)
            {
                var key = pair.Key;
                var value = pair.Value.ToString();
                builder.Append($"&{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }
            return builder.ToString();
        }

        /**
         * <summary>Return the authentication query parameters.</summary>
         */
        private IEnumerable<KeyValuePair<string, object>> AuthQuery()
        {
            var timestamp = UnixTimestamp();
            var nonce = random.Next(10000000, 100000000);
            var signature = sha1.ComputeHash(Encoding.ASCII.GetBytes($"{timestamp}{nonce}{secret}"));
            yield return new KeyValuePair<string, object>("api_key", key);
            yield return new KeyValuePair<string, object>("api_timestamp", timestamp);
            yield return new KeyValuePair<string, object>("api_nonce", nonce);
            yield return new KeyValuePair<string, object>(
                "api_signature",
                BitConverter.ToString(signature).Replace("-", "").ToLower()
            );
        }

        private static int UnixTimestamp() =>
            (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        
        private static string RemoveLeadingSlashes(string uri)
        {
            while (uri.StartsWith("/"))
            {
                uri = uri.Substring(1);
            }
            return uri;
        }
    }
}
