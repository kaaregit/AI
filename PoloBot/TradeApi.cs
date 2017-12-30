using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PoloBot
{
    class TradeApi
    {
        private String _key;
        private byte[] _secretByte;
        private static readonly Encoding encoding = Encoding.UTF8;

        private String PRIVATE_API_URL = "https://poloniex.com/tradingApi";

        public TradeApi(String key, String secret)
        {
            if(key == null || secret == null)
            {
                throw new Exception("Needs key/secret");
            }

            _key = key;

            _secretByte = encoding.GetBytes(secret);         
        }

        //Private
        public String Nonce()
        {
            var nonce = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            return Math.Round(nonce, 0).ToString();
        }

        //Private
        public byte[] EncryptHmac(String message)
        {
            using (HMACSHA512 hmac = new HMACSHA512(_secretByte))
            {
                hmac.ComputeHash(encoding.GetBytes(message));

                return hmac.Hash;
            }
        }

        //Private
        public String ByteToString(byte[] buff)
        {
            String sbinary = "";
            for(int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2");
            }

            return sbinary;
        }
        

        public HttpResponseMessage Buy(Double amount, String currencyPair, Double rate)
        {
            HttpClient _client = new HttpClient();

            var pairs = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<string, string>("command", "buy"),
                new KeyValuePair<string, string>("nonce", Nonce()),               
                new KeyValuePair<string, string>("Key", _key),
                new KeyValuePair<string, string>("currencyPair", currencyPair),
                new KeyValuePair<string, string>("rate", rate.ToString())
            };

            String paramString = "";
            String signature;


            foreach(KeyValuePair<string, string> pair in pairs)
            {
                paramString += pair.Key + "=" + pair.Value + "&";
            }

            paramString = paramString.Substring(0, paramString.Length - 1);

            signature = ByteToString(EncryptHmac(paramString));

            pairs.Add(new KeyValuePair<string, string>("Sign", signature));              

            var content = new FormUrlEncodedContent(pairs);

            var response = _client.PostAsync(PRIVATE_API_URL, content).Result;

            return response;
        }

        async public void ReturnBalances()
        {
            HttpClient _client = new HttpClient();

            var pairs = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<string, string>("command", "returnBalances"),
                new KeyValuePair<string, string>("nonce", Nonce())
            };

            String paramString = "";
            String signature;

            foreach (KeyValuePair<string, string> pair in pairs)
            {
                paramString += pair.Key + "=" + pair.Value + "&";
            }

            paramString = paramString.Substring(0, paramString.Length - 1);

            signature = ByteToString(EncryptHmac(paramString));

            _client.DefaultRequestHeaders.Add("Key", _key);
            _client.DefaultRequestHeaders.Add("Sign", signature);

            var content = new FormUrlEncodedContent(pairs);

            var response = _client.PostAsync(PRIVATE_API_URL, content).Result;

            var contents = await response.Content.ReadAsStringAsync();

            Console.WriteLine(contents);
        }

        async public void Buy(String amount, String rate, String currencyPair)
        {
            HttpClient _client = new HttpClient();

            var pairs = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<string, string>("command", "buy"),
                new KeyValuePair<string, string>("nonce", Nonce()),
                new KeyValuePair<string, string>("currencyPair", currencyPair),
                new KeyValuePair<string, string>("rate", rate),
                new KeyValuePair<string, string>("amount", amount)
            };

            String paramString = "";
            String signature;

            foreach (KeyValuePair<string, string> pair in pairs)
            {
                paramString += pair.Key + "=" + pair.Value + "&";
            }

            paramString = paramString.Substring(0, paramString.Length - 1);

            signature = ByteToString(EncryptHmac(paramString));

            _client.DefaultRequestHeaders.Add("Key", _key);
            _client.DefaultRequestHeaders.Add("Sign", signature);

            var content = new FormUrlEncodedContent(pairs);

            var response = _client.PostAsync(PRIVATE_API_URL, content).Result;

            var contents = await response.Content.ReadAsStringAsync();

            Console.WriteLine(contents);
        }

        async public void Sell(String amount, String rate, String currencyPair)
        {
            HttpClient _client = new HttpClient();

            var pairs = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<string, string>("command", "sell"),
                new KeyValuePair<string, string>("nonce", Nonce()),
                new KeyValuePair<string, string>("currencyPair", currencyPair),
                new KeyValuePair<string, string>("rate", rate),
                new KeyValuePair<string, string>("amount", amount)
            };

            String paramString = "";
            String signature;

            foreach (KeyValuePair<string, string> pair in pairs)
            {
                paramString += pair.Key + "=" + pair.Value + "&";
            }

            paramString = paramString.Substring(0, paramString.Length - 1);

            signature = ByteToString(EncryptHmac(paramString));

            _client.DefaultRequestHeaders.Add("Key", _key);
            _client.DefaultRequestHeaders.Add("Sign", signature);

            var content = new FormUrlEncodedContent(pairs);

            var response = _client.PostAsync(PRIVATE_API_URL, content).Result;

            var contents = await response.Content.ReadAsStringAsync();

            Console.WriteLine(contents);
        }
    }
}
