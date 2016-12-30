namespace TeslaTelemetryPuller
{
    using System;

    public class TeslaTelemetryPullerService
    {
        private TeslaTelemetryPullerServiceConfig _config;
        private bool _isInitialized;
        private bool _shouldStop;
        private bool _shouldRunOnce;

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

            _isInitialized = true;
        }

        public void Run()
        {
            IsInitialized();

            do
            {

            } while (!_shouldStop && !_shouldRunOnce);
        }

        public void Stop()
        {
            IsInitialized();

            _shouldStop = true;
        }

        private void IsInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Not initialized.");
            }
        }
    }
}
