using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace DasBlog.ReCaptcha
{
    public class ReCaptureValidator
    {
        const string RECAPTCHA_URL = "https://www.google.com/recaptcha/api/siteverify";
        const string VERIFY_RECAPTHA = @"{""secret"": ""{0}"", ""response"": ""{1}""}";
        private string _reCaptureSecret;

        public ReCaptureValidator(string secret)
        {
            _reCaptureSecret = secret;
        }

        public bool IsValidUserToken(string token, string ipaddress)
        {
            bool status = false;
            try
            {
                HttpClient client = new HttpClient();

                client.PostAsync(new Uri(RECAPTCHA_URL),
                                    new StringContent(string.Format(VERIFY_RECAPTHA, _reCaptureSecret, token), Encoding.UTF8, "application/json"))
                                    .ContinueWith((requestTask) =>
                                    {
                                        requestTask.Result.Content.ReadAsStringAsync().ContinueWith(
                                            (readTask) =>
                                            {
                                                string json = readTask.Result.ToString();
                                                var result = JsonConvert.DeserializeObject<ReCaptchaResults>(json);
                                                status = result.Success;
                                            });
                                    });
            }
            catch
            {

            }

            return status;
        }
    }
}
