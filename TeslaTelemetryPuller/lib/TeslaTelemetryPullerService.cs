namespace TeslaTelemetryPuller
{
    using Contracts;
    using System;
    using System.Net.Http;
    using TeslaLib;

    public class TeslaTelemetryPullerService
    {
        private TeslaTelemetryPullerServiceConfig _config;
        private bool _isInitialized;
        private bool _shouldStop;
        private bool _shouldRunOnce;

        // API info
        private string _apiUrl;
        private string _apiId;
        private string _apiSecret;

        private TeslaClient _teslaClient;

        public TeslaTelemetryPullerService(TeslaTelemetryPullerServiceConfig config)
        {
            _config = config;
            _isInitialized = false;
            _shouldStop = false;
            _shouldRunOnce = false;
        }

        public void Initialize()
        {
            if (bool.Parse(_config["runonce"]))
            {
                _shouldRunOnce = true;
            }

            ExtractApiInfo();

            _teslaClient = new TeslaClient("msgfromside3@gmail.com", _apiId, _apiSecret);

            _isInitialized = true;
        }

        public void Run()
        {
            IsInitialized();

            do
            {
                _teslaClient.         } while (!_shouldStop && !_shouldRunOnce);
        }

        public void Stop()
        {
            IsInitialized();

            _shouldStop = true;
        }

        private void ExtractApiInfo()
        {
            var url = _config["TeslaApiClientInfoUrl"];
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(url).Result;

            if(!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(string.Format("Unable to connect {0}. HttpStatus={1}", url, response.StatusCode));
            }

            // Parse the response body payload.
            var jsonPayload = response.Content.ReadAsStringAsync().Result;
            var teslaClientInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TeslaClientInfo>(jsonPayload);
            _apiId = teslaClientInfo.V1.Id;
            _apiSecret = teslaClientInfo.V1.Secret;
            _apiUrl = teslaClientInfo.V1.BaseUrl + teslaClientInfo.V1.Api;
        }

        private void PullVehecleState()
        { }

        private void PullChargeState()
        { }

        private void StoreVehicleState()
        {

        }

        private void StoreChargeState()
        { }

        private int DetermineNextCollectionTime()
        { }

        private void IsInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Not initialized.");
            }
        }
    }
}
