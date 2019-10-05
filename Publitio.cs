using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace Publitio
{
    public class PublitioApi
    {
        private readonly string key;
        private readonly string secret;
        private readonly HttpClient client;
        private readonly Random random = new Random();
        private readonly SHA1 sha1 = new SHA1CryptoServiceProvider();

        public PublitioApi(string key, string secret)
        {
            this.key = key;
            this.secret = secret;
            client = new HttpClient
            {
                BaseAddress = new Uri("https://api.publit.io/v1/"),
            };
        }

        public async Task<IDictionary<string, object>> GetAsync(
            string uri,
            IEnumerable<KeyValuePair<string, object>> query = null)
        {
            query = query ?? Enumerable.Empty<KeyValuePair<string, object>>();
            uri += EncodeQuery(Enumerable.Concat(AuthQuery(), query));
            var res = await client.GetStringAsync(uri);
            return Parse(res);
        }

        public async Task<IDictionary<string, object>> Put(string uri)
        {
            return null;
        }

        public async Task<IDictionary<string, object>> Delete(string uri)
        {
            return null;
        }

        public async Task<IDictionary<string, object>> Post()
        {
            return null;
        }

        private async Task<IDictionary<string, object>> ApiCall(
            HttpMethod method,
            string uri,
            IEnumerable<KeyValuePair<string, object>> query = null,
            HttpContent content = null)
        {
            return null;
        }

        private static string EncodeQuery(IEnumerable<KeyValuePair<string, object>> query)
        {
            var builder = new StringBuilder("?");
            foreach (var pair in query)
            {
                var key = pair.Key;
                var value = pair.Value.ToString();
                builder.Append($"&{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }
            Console.WriteLine(builder.ToString());
            return builder.ToString();
        }

        /**
         * Return the authentication query parameters.
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

        private static IDictionary<string, object> Parse(string json) =>
            (IDictionary<string, object>)JsonConvert.DeserializeObject(json);
    }
}
