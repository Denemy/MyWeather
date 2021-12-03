using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

public class GetMyLocationProperty : MonoBehaviour
{
    // Properties weather for a my location (page of the Home).
    [SerializeField] private Text cityAndcountry;
    [SerializeField] private Text myLocation;
    [SerializeField] private Text temperature;
    [SerializeField] private Text humidity;
    [SerializeField] private Text windSpeed;

    // button go to page "Week weather" (page of the Home).
    [SerializeField] private Image weatherButton;
    [SerializeField] private List<Sprite> weatherIcons;

    private string responseMyLoc = "";

    public static WeatherResponse cityWeather;

    WeatherResponse weatherMyLocResponse = new WeatherResponse();

    private Save sv = new Save();
    private string path;

    private string language = "en";

    // ============================= Search for my location =============================
    IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Save.json");
#else
        path = Path.Combine(Application.dataPath, "Save.json");
#endif
        if (File.Exists(path))
        {
            sv = JsonUtility.FromJson<Save>(File.ReadAllText(path));

            language = sv.language;
        }

        // First, check if user has location service enabled
        //if (!Input.location.isEnabledByUser)                                                
        //yield break;

        Input.location.Start();

        int maxWait = 30;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            yield break;
        }
        else
        {
            myLocation.text = "48.51315" + ", " + "32.26908";
            GetInfoForLocation(48.51315, 32.26908);

            //myLocation.text = Input.location.lastData.latitude.ToString() + ", " + Input.location.lastData.longitude.ToString();    
            //GetInfoForLocation(Input.location.lastData.latitude, Input.location.lastData.longitude);                                
        }
        Input.location.Stop();
    }
    // ----------------------------------------------------------------------------------


    // =================== Getting weather properties by coordinates ===================
    private void GetInfoForLocation(double lat, double lon)
    {
        string urlMyLoc = "http://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&" + "lon=" + lon + "&units=metric&appid=c5c264c00bd71547390791c9cfac2283";

        HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(urlMyLoc);
        HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();

        using (StreamReader streamReader = new StreamReader(Response.GetResponseStream()))
        {
            responseMyLoc = streamReader.ReadToEnd();
        }

        weatherMyLocResponse = JsonConvert.DeserializeObject<WeatherResponse>(responseMyLoc);
        
        WriteAllText(language, weatherMyLocResponse);

        cityWeather = weatherMyLocResponse;
    }
    // ----------------------------------------------------------------------------------


    // ========================== Fill all weather properties ===========================
    private void WriteAllText(string language, WeatherResponse response)
    {
        if (language == "English")
        {
            cityAndcountry.text = "city " + weatherMyLocResponse.name + ", " + weatherMyLocResponse.sys.country;
            temperature.text = (int)weatherMyLocResponse.main.Temp + "°";
            weatherButton.sprite = GetWeatherIcon(weatherMyLocResponse.weather[0].icon);
            humidity.text = "Humidity: " + weatherMyLocResponse.main.humidity + "%";
            windSpeed.text = "Wind speed: " + (int)weatherMyLocResponse.wind.speed + " m/s";
        }
        else
        {
            cityAndcountry.text = "город " + weatherMyLocResponse.name + ", " + weatherMyLocResponse.sys.country;
            temperature.text = (int)weatherMyLocResponse.main.Temp + "°";
            weatherButton.sprite = GetWeatherIcon(weatherMyLocResponse.weather[0].icon);
            humidity.text = "Влажность: " + weatherMyLocResponse.main.humidity + "%";
            windSpeed.text = "Скорость ветра: " + (int)weatherMyLocResponse.wind.speed + " м/с";

            Controller.pleaseWait = true;
        }
    }
    // ----------------------------------------------------------------------------------


    // ============================= Getting weather icon ===============================
    private Sprite GetWeatherIcon(string idIcon)
    {
        if (idIcon == "01d") return weatherIcons[0];
        if (idIcon == "01n") return weatherIcons[1];
        if (idIcon == "02d") return weatherIcons[2];
        if (idIcon == "02n") return weatherIcons[3];
        if (idIcon == "03d") return weatherIcons[4];
        if (idIcon == "03n") return weatherIcons[5];
        if (idIcon == "04d") return weatherIcons[6];
        if (idIcon == "04n") return weatherIcons[7];
        if (idIcon == "09d") return weatherIcons[8];
        if (idIcon == "09n") return weatherIcons[9];
        if (idIcon == "10d") return weatherIcons[10];
        if (idIcon == "10n") return weatherIcons[11];
        if (idIcon == "11d") return weatherIcons[12];
        if (idIcon == "11n") return weatherIcons[13];
        if (idIcon == "13d") return weatherIcons[14];
        if (idIcon == "13n") return weatherIcons[15];
        if (idIcon == "50d") return weatherIcons[16];
        if (idIcon == "50n") return weatherIcons[17];

        return null;
    }
    // ----------------------------------------------------------------------------------
}