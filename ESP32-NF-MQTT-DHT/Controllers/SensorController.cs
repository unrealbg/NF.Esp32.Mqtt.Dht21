﻿namespace ESP32_NF_MQTT_DHT.Controllers
{
    using System;
    using System.Net;

    using ESP32_NF_MQTT_DHT.Services.Contracts;

    using Microsoft.Extensions.Logging;

    using Models;

    using nanoFramework.Json;
    using nanoFramework.WebServer;

    [Authentication("Basic:user p@ssw0rd")]
    public class SensorController : BaseController
    {
        //private readonly IDhtService _dhtService;
        //private readonly IAhtSensorService _ahtSensorService;
        private readonly IShtc3SensorService _shtc3SensorService;
        private readonly ILogger _logger;

        public SensorController(
            ILoggerFactory loggerFactory,
            IShtc3SensorService shtc3SensorService) //IDhtService dhtService) , IAhtSensorService ahtSensorService)
        {
            //_dhtService = dhtService ?? throw new ArgumentNullException(nameof(dhtService));
            //_ahtSensorService = ahtSensorService ?? throw new ArgumentNullException(nameof(ahtSensorService));
            _shtc3SensorService = shtc3SensorService ?? throw new ArgumentNullException(nameof(shtc3SensorService));
            _logger = loggerFactory?.CreateLogger(nameof(SensorController)) ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        [Route("/")]
        [Method("GET")]
        public void Index(WebServerEventArgs e)
        {
            this.HandleRequest(
                e,
                () =>
                    {
                        this.SendPage(e, "Welcome to the sensor API.");
                    });
        }

        [Route("api/temperature")]
        [Method("GET")]
        public void GetTemperature(WebServerEventArgs e)
        {
            this.HandleRequest(
                e,
                () =>
                    {
                        try
                        {
                            var temperature = this.FetchTemperature();
                            if (this.IsValidTemperature(temperature))
                            {
                                var jsonResponse = $"{{\"temperature\": {temperature:f2}}}";
                                this.SendResponse(e, jsonResponse, "application/json");
                            }
                            else
                            {
                                this.SendErrorResponse(e, "Temperature data is out of expected range.", HttpStatusCode.InternalServerError);
                                _logger.LogWarning("Temperature data is out of expected range.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.SendErrorResponse(e, "An unexpected error occurred.", HttpStatusCode.InternalServerError);
                            _logger.LogError(ex, "An unexpected error occurred.");
                        }
                    });
        }

        [Route("api/humidity")]
        [Method("GET")]
        public void GetHumidity(WebServerEventArgs e)
        {
            this.HandleRequest(
                e,
                () =>
                    {
                        try
                        {
                            var humidity = this.FetchHumidity();
                            if (!double.IsNaN(humidity))
                            {
                                var jsonResponse = $"{{\"humidity\": {humidity:f1}}}";
                                this.SendResponse(e, jsonResponse, "application/json");
                            }
                            else
                            {
                                this.SendErrorResponse(e, "Humidity data is unavailable.", HttpStatusCode.InternalServerError);
                                _logger.LogWarning("Humidity data is unavailable.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.SendErrorResponse(e, $"An unexpected error occurred: {ex.Message}", HttpStatusCode.InternalServerError);
                            _logger.LogError(ex, "An unexpected error occurred.");
                        }
                    });
        }

        [Route("api/data")]
        [Method("GET")]
        public void GetData(WebServerEventArgs e)
        {
            this.HandleRequest(
                e,
                () =>
                    {
                        try
                        {
                            var temperature = double.Parse($"{this.FetchTemperature():f2}");
                            var humidity = this.FetchHumidity();
                            if (!double.IsNaN(temperature) && !double.IsNaN(humidity))
                            {
                                var sensorData = new Sensor
                                {
                                    Data = new Data
                                    {
                                        Date = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                                        Time = DateTime.UtcNow.ToString("HH:mm:ss"),
                                        Temp = temperature,
                                        Humid = (int)humidity
                                    }
                                };

                                var jsonResponse = JsonSerializer.SerializeObject(sensorData);
                                this.SendResponse(e, jsonResponse, "application/json");
                            }
                            else
                            {
                                this.SendErrorResponse(e, "Sensor data is unavailable.", HttpStatusCode.InternalServerError);
                                _logger.LogWarning("Sensor data is unavailable.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.SendErrorResponse(e, $"An unexpected error occurred: {ex.Message}", HttpStatusCode.InternalServerError);
                            _logger.LogError(ex, "An unexpected error occurred.");
                        }
                    });
        }

        private double FetchTemperature()
        {
            try
            {
                //return _dhtService.GetTemp();
                //return _ahtSensorService.GetTemp();
                return _shtc3SensorService.GetTemp();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to fetch temperature.");
                return double.NaN;
            }
        }

        private double FetchHumidity()
        {
            try
            {
                //return _dhtService.GetHumidity();
                //return _ahtSensorService.GetHumidity();
                return _shtc3SensorService.GetHumidity();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to fetch humidity.");
                return double.NaN;
            }
        }

        private bool IsValidTemperature(double temperature)
        {
            return temperature >= -40 && temperature <= 85;
        }
    }
}
