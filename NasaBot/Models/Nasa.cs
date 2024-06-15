using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasaBot.Objects
{
    public class Nasa
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }


    }

    public class NasaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "TSLCHxDtNa5s4U9BW0iQA8G2ynOqiT9uT9VdvGjf";
        //https://api.nasa.gov/planetary/apod?api_key=TSLCHxDtNa5s4U9BW0iQA8G2ynOqiT9uT9VdvGjf
        public NasaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<Nasa> GetTodayAstronomyPictureAsync()
        {
            string apiUrl = "https://api.nasa.gov/planetary/apod";
            string requestUrl = $"{apiUrl}?api_key={_apiKey}";

            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(requestUrl))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var nasaData = JsonConvert.DeserializeObject<Nasa>(json);

                    return nasaData;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error making API request: {ex.Message}");
                return null;
            }
        }

        public async Task<Nasa> GetAstronomyPictureAsync()
        {
            string apiUrl = "https://api.nasa.gov/planetary/apod";
            string requestUrl = $"{apiUrl}?date={GetRandomDateString()}&api_key={_apiKey}";

            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(requestUrl))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var nasaData = JsonConvert.DeserializeObject<Nasa>(json);

                    return nasaData;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error making API request: {ex.Message}");
                return null;
            }
        }
        private static string GetRandomDateString()
        {
            Random random = new Random();
            int year = random.Next(2000, 2023);
            int month = random.Next(1, 13);
            int maxDaysInMonth = DateTime.DaysInMonth(year, month);
            int day = random.Next(1, maxDaysInMonth + 1);

            DateTime randomDate = new DateTime(year, month, day);
            return randomDate.ToString("yyyy-MM-dd");
        }
    }
}
