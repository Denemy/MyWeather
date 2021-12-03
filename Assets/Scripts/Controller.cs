using System;
using System.IO;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Globalization;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class Controller : MonoBehaviour
{
    [SerializeField] private GameObject[] Pages;                    // Array with pages of the game (Settings, Home, More, Week weather).

    [SerializeField] private List<GameObject> closeButtons;         // Removal buttons for added locations (page of the More).
    [SerializeField] private GameObject[] Shelfs;                   // Array with sample of the locations. (page of the More).

    // Properties of today and tomorrow for added locations (page of the More).
    [SerializeField] private List<Text> citiesAndCountrys;          
    [SerializeField] private List<Text> temperatureToday;           
    [SerializeField] private List<Text> humidityToday;              
    [SerializeField] private List<Image> weatherToday;
    [SerializeField] private List<Image> weatherTomorrow;
    [SerializeField] private List<Text> temperatureTomorrow;
    [SerializeField] private List<Text> humidityTomorrow;

    [SerializeField] private List<Sprite> weatherIcons;             // All weather icons.

    // Input field to add new locations (page of the More). 
    [SerializeField] private GameObject addLocation;                
    [SerializeField] private GameObject searchPanel;                
    [SerializeField] private InputField searchPanelText;            
    [SerializeField] private Text enteredText;

    // Properties for a week (page of the Week weather).
    [SerializeField] private Text dayAndDate;
    [SerializeField] private Text time;
    [SerializeField] private Text cityAndCountry;
    [SerializeField] private Text location;
    [SerializeField] private List<Image> weatherForWeek;
    [SerializeField] private List<Text> daysOfWeek;
    [SerializeField] private List<Text> temperatureOfWeek;
    [SerializeField] private List<Text> humidityOfWeek;
    [SerializeField] private List<Text> windSpeedOfWeek;

    [SerializeField] private Text myLocation;                       // My coordinates (page of the Home).

    // Page of the Settings.
    [SerializeField] private Image bg;                              
    [SerializeField] private Sprite lightBg;
    [SerializeField] private Sprite nightBg;
    [SerializeField] private Toggle bgMode;

    [SerializeField] private Text theme;
    [SerializeField] private Text nightMode;
    [SerializeField] private Text languageText;

    // Menu buttons.
    [SerializeField] private List<Button> buttons;
    [SerializeField] private List<Sprite> buttonsSpriteLight;
    [SerializeField] private List<Sprite> buttonsSpriteNight;

    // Removal buttons for added locations (page of the More).
    [SerializeField] private List<Button> buttonsClose;
    [SerializeField] private Sprite buttonCloseSpriteLight;
    [SerializeField] private Sprite buttonCloseSpriteNight;

    // Frames for added locations (page of the More).
    [SerializeField] private List<Image> frames;
    [SerializeField] private Sprite frameLight;
    [SerializeField] private Sprite frameNight;

    // Properties weather for my location (page of the Home).
    [SerializeField] private Text myCityAndcountry;
    [SerializeField] private Text myTemperature;
    [SerializeField] private Text myHumidity;
    [SerializeField] private Text myWindSpeed;


    private Save sv = new Save();
    private string path;

    private DateTime dt = DateTime.Now;

    private List<string> citiesSave = new List<string>();

    private bool load;
    private bool loadHumidityAndWind;

    private static readonly string subscriptionKey = "c4d8da73016846a1ae3e839fbe637007";
    private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";

    private static readonly string regionLocation = "northeurope";

    [SerializeField] private Text testText;
    public static string tempText;
    public static string tempText2;
    const string quote = "\"";

    [SerializeField] private Text language;
    [SerializeField] private Button changeLanguage;
    [SerializeField] private Text changeLanguageText;

    public static bool pleaseWait = false;

    [SerializeField] private GameObject loading;

    [SerializeField] private Text addLocationText;
    [SerializeField] private Text placeHolderText;
    [SerializeField] private Text invalidText;
    [SerializeField] private Text nonLocText;
    [SerializeField] private Text noPlaceText;

    private string todayEN = "Today";
    private string tomorrowEN = "Tomorrow";
    private string todayRU = "Сегодня";
    private string tomorrowRu = "Завтра";
    [SerializeField] private List<Text> todaysText;
    [SerializeField] private List<Text> tomorrowsText;

    private void Start()
    {
        GetDayAndDate(dayAndDate, "en");
        GetTime(time);

        for (int i = 0; i < citiesAndCountrys.Count; i++)
        {
            citiesSave.Add("");
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Save.json");
#else
        path = Path.Combine(Application.dataPath, "Save.json");
#endif
        if (File.Exists(path))
        {
            sv = JsonUtility.FromJson<Save>(File.ReadAllText(path));

            citiesSave[0] = sv.firstShelf;
            citiesSave[1] = sv.secondShelf;
            citiesSave[2] = sv.thirdShelf;
            citiesSave[3] = sv.fourthShelf;
            citiesSave[4] = sv.fifthShelf;

            for (int i = 0; i < sv.countShowerShelfs; i++)
            {
                ShowShelfs(citiesSave[i]);
            }

            if (sv.bgNight)
            {
                ChangeBgMode(true);

                load = true;
                bgMode.isOn = true;
                load = false;
            }
            else
            {
                ChangeBgMode(false);
                load = false;
            }

            if (sv.language == "Русский")
            {
                ChangeLanguage();
            }
        }
    }

    private void Update()
    {
        GetTime(time);

        if (pleaseWait)
        {
            LoadOtherText();
            pleaseWait = false;
        }
    }

    // ============================== Fill the "Home" page ==============================
    private void LoadOtherText()
    {
        Translator(myCityAndcountry.text, "en", "ru");
        System.Threading.Thread.Sleep(3000);
        myCityAndcountry.text = tempText;

        tempText = myHumidity.text;
        myHumidity.text = "Влажность: " + tempText.Substring(11, 3);

        tempText = myWindSpeed.text;
        myWindSpeed.text = "Скорость ветра: " + tempText.Substring(16, 1) + " м/с";
    }
    // ----------------------------------------------------------------------------------


    // ======================== Gettings weather on day or week =========================
    private object GetWeatherForCity (string city, bool oneDay)
    {
        HttpWebResponse httpWebResponse = null;
        string url;
        WeatherResponse weatherResponse = new WeatherResponse();
        FiveDayWeatherResponse weatherResponseFive = new FiveDayWeatherResponse();
        string response = "";

        if (oneDay)
        {
            url = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&units=metric&appid=c5c264c00bd71547390791c9cfac2283";
        }
        else
        {
            url = "http://api.openweathermap.org/data/2.5/forecast?q=" + city + "&units=metric&appid=c5c264c00bd71547390791c9cfac2283";
        }

        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

        if (IsPageExists(url))
        {
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }
        else
        {
            return null;
        }
        
        using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
        {
            response = streamReader.ReadToEnd();
        }

        if (oneDay)
        {
            weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);
            return weatherResponse;
        }
        else
        {
            weatherResponseFive = JsonConvert.DeserializeObject<FiveDayWeatherResponse>(response);
            return weatherResponseFive;
        }
    }
    // ----------------------------------------------------------------------------------


    // ============================ Check to existence page =============================
    bool IsPageExists(string url)
    {
        try
        {
            WebClient client = new WebClient();
            client.DownloadString(url);
        }
        catch (WebException ex)
        {
            HttpWebResponse response = ex.Response != null ? ex.Response as HttpWebResponse : null;
            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        return true;
    }
    //-----------------------------------------------------------------------------------


    // ========================== Filling text "Day and Date" ===========================
    private void GetDayAndDate(Text text, string language)
    {
        if (language == "ru")
        {
            string ruDay = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(dt.DayOfWeek);
            ruDay = char.ToUpper(ruDay[0]) + ruDay.Substring(1);
            text.text = ruDay + ", " + dt.ToShortDateString();
        }
        else
        {
            text.text = dt.DayOfWeek + ", " + dt.ToShortDateString();
        }
    }
    // ----------------------------------------------------------------------------------


    // ============================== Filling text "Time" ===============================
    private void GetTime(Text text)
    {
        text.text = dt.ToShortTimeString();
    }
    // ----------------------------------------------------------------------------------


    // =============================== Go to page "More" ================================
    public void ButtonMore ()
    {


        for (int i = 0; i < Pages.Length; i++)
        {
            if (Pages[i].activeSelf)
            {
                Pages[i].SetActive(false);
            }
        }

        Pages[0].SetActive(true);
    }
    // ----------------------------------------------------------------------------------


    // =============================== Go to page "Home" ================================
    public void ButtonHome()
    {
        for (int i = 0; i < Pages.Length; i++)
        {
            if (Pages[i].activeSelf)
            {
                Pages[i].SetActive(false);
            }
        }

        Pages[1].SetActive(true);
    }
    // ----------------------------------------------------------------------------------


    // =========================== Go to page "Week weather" ============================
    public void ButtonWeather()
    {
        for (int i = 0; i < Pages.Length; i++)
        {
            if (Pages[i].activeSelf)
            {
                Pages[i].SetActive(false);
            }
        }

        Pages[3].SetActive(true);

        FiveDayWeatherResponse weatherWeek = (FiveDayWeatherResponse)GetWeatherForCity(GetMyLocationProperty.cityWeather.name, false);

        cityAndCountry.text = myCityAndcountry.text;
        location.text = myLocation.text;

        List<string> nameDays = new List<string>();
        nameDays.Add("Monday"); nameDays.Add("Tuesday"); nameDays.Add("Wednesday");
        nameDays.Add("Thursday"); nameDays.Add("Friday"); nameDays.Add("Saturday"); nameDays.Add("Sunday");

        int numDay = 0;
        for (int i = 0; i < nameDays.Count; i++)
        {
            if (nameDays[i] == dt.DayOfWeek.ToString())
            {
                if (CalculationTimeTomorrow() < 4)
                {
                    numDay = i;
                }
                else
                {
                    numDay = i + 1;
                }
                break;
            }
        }

        if (changeLanguageText.text == "Русский")
        {
            nameDays[0] = "Понедельник"; nameDays[1] = "Вторник"; nameDays[2] = "Среда"; nameDays[3] = "Четверг";
            nameDays[4] = "Пятница"; nameDays[5] = "Субота"; nameDays[6] = "Воскресенье";
        }

        int j = 0;
        for (int i = 0; i < daysOfWeek.Count; i++)
        {
            if ((i + numDay) < nameDays.Count)
                daysOfWeek[i].text = nameDays[i + numDay];
            else
            {
                daysOfWeek[i].text = nameDays[j];
                j++;
            }
        }

        for (int i = 0; i < weatherForWeek.Count; i++)
        {
            if (i == 0)
            {
                weatherForWeek[i].sprite = GetWeatherIcon(weatherWeek.list[CalculationTimeTomorrow()].weather[0].icon);
                temperatureOfWeek[i].text = (int)weatherWeek.list[CalculationTimeTomorrow()].main.temp + "° /";
                humidityOfWeek[i].text = (int)weatherWeek.list[CalculationTimeTomorrow()].main.humidity + "% /";

                if (changeLanguageText.text == "Русский")
                    windSpeedOfWeek[i].text = ((int)weatherWeek.list[CalculationTimeTomorrow()].wind.speed).ToString() + " м/с";
                else
                    windSpeedOfWeek[i].text = ((int)weatherWeek.list[CalculationTimeTomorrow()].wind.speed).ToString() + " m/s";
            }
            else
            {
                weatherForWeek[i].sprite = GetWeatherIcon(weatherWeek.list[8 * i + CalculationTimeTomorrow()].weather[0].icon);
                temperatureOfWeek[i].text = (int)weatherWeek.list[8 * i + CalculationTimeTomorrow()].main.temp + "° /";
                humidityOfWeek[i].text = (int)weatherWeek.list[8 * i + CalculationTimeTomorrow()].main.humidity + "% /";

                if (changeLanguageText.text == "Русский")
                    windSpeedOfWeek[i].text = ((int)weatherWeek.list[8 * i + CalculationTimeTomorrow()].wind.speed).ToString() + " м/с";
                else
                    windSpeedOfWeek[i].text = ((int)weatherWeek.list[8 * i + CalculationTimeTomorrow()].wind.speed).ToString() + " m/s";
            }
        }
    }
    // ----------------------------------------------------------------------------------


    // ============================= Go to page "Settings" ==============================
    public void ButtonSettings()
    {
        for (int i = 0; i < Pages.Length; i++)
        {
            if (Pages[i].activeSelf)
            {
                Pages[i].SetActive(false);
            }
        }

        Pages[2].SetActive(true);
    }
    // ----------------------------------------------------------------------------------


    // ===================== Show inpud field to add new locations ======================
    public void ButtonAddLocation()
    {
        addLocation.SetActive(false);
        searchPanel.SetActive(true);
    }
    // ----------------------------------------------------------------------------------


    // ========================== Reactions to incorrect input ==========================
    public void StartInput()
    {
        if (invalidText.gameObject.activeSelf && searchPanelText.text.Length > 0)
            invalidText.gameObject.SetActive(false);

        if (nonLocText.gameObject.activeSelf && searchPanelText.text.Length > 0)
            nonLocText.gameObject.SetActive(false);

        if (noPlaceText.gameObject.activeSelf && searchPanelText.text.Length > 0)
            noPlaceText.gameObject.SetActive(false);
    }
    // ----------------------------------------------------------------------------------


    //===================== Getting weather from the introduced text ====================
    public void EndInput()
    {
        bool flag = true;
        int enumenator = 0;

        for (int i = 0; i < enteredText.text.Length; i++)
        {
            if (!(Char.IsDigit(enteredText.text[i]) || enteredText.text[i] >= 'a' &&
                enteredText.text[i] <= 'z' || enteredText.text[i] >= 'A' && enteredText.text[i] <= 'Z'))
            {
                searchPanelText.text = "";
                invalidText.gameObject.SetActive(true);
                flag = false;
                break;
            }
        }

        if (!flag) return;

        if (GetWeatherForCity(enteredText.text, true) != null)
        {
            for (int i = 0; i < Shelfs.Length; i++)
            {
                if (Shelfs[i].activeSelf) enumenator++;

                if (enumenator == 5)
                {
                    searchPanelText.text = "";
                    noPlaceText.gameObject.SetActive(true);
                }

                if (!Shelfs[i].activeSelf)
                {
                    enumenator++;
                    Shelfs[i].SetActive(true);

                    WeatherResponse wrOneDay = (WeatherResponse)GetWeatherForCity(enteredText.text, true);
                    FiveDayWeatherResponse wrFiveDay = (FiveDayWeatherResponse)GetWeatherForCity(enteredText.text, false);

                    citiesSave[i] = wrOneDay.name;

                    citiesAndCountrys[i].text = "city " + wrOneDay.name + ", " + wrOneDay.sys.country;
                    temperatureToday[i].text = (int)wrOneDay.main.Temp + "° / ";
                    humidityToday[i].text = wrOneDay.main.humidity + "%";
                    
                    weatherToday[i].sprite = GetWeatherIcon(wrOneDay.weather[0].icon);

                    weatherTomorrow[i].sprite = GetWeatherIcon(wrFiveDay.list[CalculationTimeTomorrow()].weather[0].icon);

                    temperatureTomorrow[i].text = ((int)wrFiveDay.list[CalculationTimeTomorrow()].main.temp).ToString() + "° / ";
                    humidityTomorrow[i].text = ((int)wrFiveDay.list[CalculationTimeTomorrow()].main.humidity).ToString() + "%";

                    if (changeLanguageText.text == "Русский")
                    {
                        tempText = "";
                        Translator(citiesAndCountrys[i].text, "en", "ru");
                        System.Threading.Thread.Sleep(3000);
                        citiesAndCountrys[i].text = tempText;
                    }

                    searchPanelText.text = "";
                    searchPanel.SetActive(false);
                    addLocation.SetActive(true);

                    break;
                }
            }
        }
        else
        {
            nonLocText.gameObject.SetActive(true);
            searchPanelText.text = "";
        }
    }
    // ----------------------------------------------------------------------------------


    // ============================ Loading added locations =============================
    private void ShowShelfs(string city)
    {
        int enumenator = 0;

        if (GetWeatherForCity(city, true) != null)
        {
            for (int i = 0; i < Shelfs.Length; i++)
            {
                if (Shelfs[i].activeSelf) enumenator++;

                if (enumenator == 5)
                {
                    searchPanelText.text = "";
                    noPlaceText.gameObject.SetActive(true);
                }

                if (!Shelfs[i].activeSelf)
                {
                    enumenator++;
                    Shelfs[i].SetActive(true);

                    WeatherResponse wrOneDay = (WeatherResponse)GetWeatherForCity(city, true);
                    FiveDayWeatherResponse wrFiveDay = (FiveDayWeatherResponse)GetWeatherForCity(city, false);

                    citiesSave[i] = wrOneDay.name;
                    citiesAndCountrys[i].text = "city " + wrOneDay.name + ", " + wrOneDay.sys.country;
                    temperatureToday[i].text = (int)wrOneDay.main.Temp + "° / ";
                    humidityToday[i].text = wrOneDay.main.humidity + "% ";

                    weatherToday[i].sprite = GetWeatherIcon(wrOneDay.weather[0].icon);

                    weatherTomorrow[i].sprite = GetWeatherIcon(wrFiveDay.list[CalculationTimeTomorrow()].weather[0].icon);

                    temperatureTomorrow[i].text = ((int)wrFiveDay.list[CalculationTimeTomorrow()].main.temp).ToString() + "° / ";
                    humidityTomorrow[i].text = ((int)wrFiveDay.list[CalculationTimeTomorrow()].main.humidity).ToString() + "%";

                    searchPanelText.text = "";
                    searchPanel.SetActive(false);
                    addLocation.SetActive(true);

                    break;
                }
            }
        }
        else
        {
            nonLocText.gameObject.SetActive(true);
            searchPanelText.text = "";
        }
    }
    // ----------------------------------------------------------------------------------


    // =================== Change the position of the added locations ===================
    private void ChangeShelfs(int indexW, int indexM)
    {
        citiesSave[indexW] = citiesSave[indexM];
        citiesAndCountrys[indexW].text = citiesAndCountrys[indexM].text;
        temperatureToday[indexW].text = temperatureToday[indexM].text;
        humidityToday[indexW].text = humidityToday[indexM].text;
        weatherToday[indexW].sprite = weatherToday[indexM].sprite;
        weatherTomorrow[indexW].sprite = weatherTomorrow[indexM].sprite;
        temperatureTomorrow[indexW].text = temperatureTomorrow[indexM].text;
        humidityTomorrow[indexW].text = humidityTomorrow[indexM].text;
    }
    // ----------------------------------------------------------------------------------


    // ======================= Removing the first added location ========================
    public void ButtonCloseOne ()
    {
        int countActiveShelf = 0;

        citiesSave[0] = "";

        for (int i = 0; i < Shelfs.Length; i++)
        {
            if (Shelfs[i].activeSelf)
            {
                countActiveShelf++;
            }
        }

        if (countActiveShelf == 1)
            Shelfs[0].SetActive(false);
        else if (countActiveShelf == 2)
        {
            ChangeShelfs(0, 1);
            Shelfs[1].SetActive(false);
        }
        else if (countActiveShelf == 3)
        {
            ChangeShelfs(0, 1);
            ChangeShelfs(1, 2);
            Shelfs[2].SetActive(false);
        }
        else if (countActiveShelf == 4)
        {
            ChangeShelfs(0, 1);
            ChangeShelfs(1, 2);
            ChangeShelfs(2, 3);

            Shelfs[3].SetActive(false);
        }
        else if (countActiveShelf == 5)
        {
            ChangeShelfs(0, 1);
            ChangeShelfs(1, 2);
            ChangeShelfs(2, 3);
            ChangeShelfs(3, 4);

            Shelfs[4].SetActive(false);
        }
    }
    // ----------------------------------------------------------------------------------


    // ======================= Removing the second added location ========================
    public void ButtonCloseTwo()
    {
        int countActiveShelf = 0;

        citiesSave[1] = "";

        for (int i = 0; i < Shelfs.Length; i++)
        {
            if (Shelfs[i].activeSelf)
            {
                countActiveShelf++;
            }
        }

        if (countActiveShelf == 2)
            Shelfs[1].SetActive(false);
        else if (countActiveShelf == 3)
        {
            ChangeShelfs(1, 2);
            Shelfs[2].SetActive(false);
        }
        else if (countActiveShelf == 4)
        {
            ChangeShelfs(1, 2);
            ChangeShelfs(2, 3);
            Shelfs[3].SetActive(false);
        }
        else if (countActiveShelf == 5)
        {
            ChangeShelfs(1, 2);
            ChangeShelfs(2, 3);
            ChangeShelfs(3, 4);

            Shelfs[4].SetActive(false);
        }
    }
    // ----------------------------------------------------------------------------------


    // ======================= Removing the third added location ========================
    public void ButtonCloseThree()
    {
        int countActiveShelf = 0;

        citiesSave[2] = "";

        for (int i = 0; i < Shelfs.Length; i++)
        {
            if (Shelfs[i].activeSelf)
            {
                countActiveShelf++;
            }
        }

        if (countActiveShelf == 3)
            Shelfs[2].SetActive(false);
        else if (countActiveShelf == 4)
        {
            ChangeShelfs(2, 3);
            Shelfs[3].SetActive(false);
        }
        else if (countActiveShelf == 5)
        {
            ChangeShelfs(2, 3);
            ChangeShelfs(3, 4);
            Shelfs[4].SetActive(false);
        }
    }
    // ----------------------------------------------------------------------------------


    // ======================= Removing the fourth added location ========================
    public void ButtonCloseFour()
    {
        int countActiveShelf = 0;

        citiesSave[3] = "";

        for (int i = 0; i < Shelfs.Length; i++)
        {
            if (Shelfs[i].activeSelf)
            {
                countActiveShelf++;
            }
        }

        if (countActiveShelf == 4)
            Shelfs[3].SetActive(false);
        else if (countActiveShelf == 5)
        {
            ChangeShelfs(3, 4);
            Shelfs[4].SetActive(false);
        }
    }
    // ----------------------------------------------------------------------------------


    // ======================= Removing the fifth added location ========================
    public void ButtonCloseFive()
    {
        citiesSave[4] = "";

        Shelfs[4].SetActive(false);
    }
    // ----------------------------------------------------------------------------------


    // =========================== Button change background =============================
    public void ToggleChangeBg()
    {
        if (load)
            return;

        if (bgMode.isOn)
        {
            ChangeBgMode(true);
        }
        else if (!bgMode.isOn)
        {
            ChangeBgMode(false);
        }
    }
    // ----------------------------------------------------------------------------------


    // =============================== Change background ================================
    private void ChangeBgMode(bool night)
    {
        Color col;

        if (night)
        {
            col = Color.white;

            bg.sprite = nightBg;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].GetComponent<Image>().sprite = buttonsSpriteNight[i];
            }

            for (int i = 0; i < buttonsClose.Count; i++)
            {
                todaysText[i].GetComponent<Text>().color = col;
                tomorrowsText[i].GetComponent<Text>().color = col;

                buttonsClose[i].GetComponent<Image>().sprite = buttonCloseSpriteNight;
                frames[i].GetComponent<Image>().sprite = frameNight;
            }
        }
        else
        {
            col = Color.black;

            bg.sprite = lightBg;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].GetComponent<Image>().sprite = buttonsSpriteLight[i];
            }

            for (int i = 0; i < buttonsClose.Count; i++)
            {
                todaysText[i].GetComponent<Text>().color = col;
                tomorrowsText[i].GetComponent<Text>().color = col;

                buttonsClose[i].GetComponent<Image>().sprite = buttonCloseSpriteLight;
                frames[i].GetComponent<Image>().sprite = frameLight;
            }
        }

        theme.GetComponent<Text>().color = col;
        nightMode.GetComponent<Text>().color = col;
        languageText.GetComponent<Text>().color = col;

        myLocation.GetComponent<Text>().color = col;
        cityAndCountry.GetComponent<Text>().color = col;
        location.GetComponent<Text>().color = col;
        dayAndDate.GetComponent<Text>().color = col;
        time.GetComponent<Text>().color = col;

        myCityAndcountry.GetComponent<Text>().color = col;
        myHumidity.GetComponent<Text>().color = col;
        myTemperature.GetComponent<Text>().color = col;
        myWindSpeed.GetComponent<Text>().color = col;

        for (int i = 0; i < citiesAndCountrys.Count; i++)
        {
            citiesAndCountrys[i].GetComponent<Text>().color = col;
            temperatureToday[i].GetComponent<Text>().color = col;
            humidityToday[i].GetComponent<Text>().color = col;
            temperatureTomorrow[i].GetComponent<Text>().color = col;
            humidityTomorrow[i].GetComponent<Text>().color = col;
            daysOfWeek[i].GetComponent<Text>().color = col;
            temperatureOfWeek[i].GetComponent<Text>().color = col;
            humidityOfWeek[i].GetComponent<Text>().color = col;
            windSpeedOfWeek[i].GetComponent<Text>().color = col;
        }
    }
    // ----------------------------------------------------------------------------------


    // =================== Сalculating tomorrow's middle of the day =====================
    private int CalculationTimeTomorrow()
    {
        string time = dt.ToShortTimeString();

        if (time.Length == 4)
        {
            if (time.Substring(2, 1) == "3" || time.Substring(2, 1) == "4" || time.Substring(2, 1) == "5")
            {
                int hour = Int32.Parse(time.Substring(0, 1)) + 1;
                time = hour.ToString() + time.Substring(1, 3);
            }
        }
        else
        {
            if (time.Substring(3, 1) == "3" || time.Substring(3, 1) == "4" || time.Substring(3, 1) == "5")
            {
                if (time.Substring(0, 2) != "23")
                {
                    int hour = Int32.Parse(time.Substring(0, 2)) + 1;
                    time = hour.ToString() + time.Substring(2, 3);
                }
                else
                {
                    time = "0" + time.Substring(2, 3);
                }
            }
        }

        if (time.Substring(0, 2) == "11" || time.Substring(0, 2) == "12" || time.Substring(0, 2) == "13") return 7;
        if (time.Substring(0, 2) == "14" || time.Substring(0, 2) == "15" || time.Substring(0, 2) == "16") return 6;
        if (time.Substring(0, 2) == "17" || time.Substring(0, 2) == "18" || time.Substring(0, 2) == "19") return 5;
        if (time.Substring(0, 2) == "20" || time.Substring(0, 2) == "21" || time.Substring(0, 2) == "22") return 4;
        if (time.Substring(0, 2) == "23" || time.Substring(0, 2) == "0:" || time.Substring(0, 2) == "1:") return 3;
        if (time.Substring(0, 2) == "2:" || time.Substring(0, 2) == "3:" || time.Substring(0, 2) == "4:") return 2;
        if (time.Substring(0, 2) == "5:" || time.Substring(0, 2) == "6:" || time.Substring(0, 2) == "7:") return 1;
        if (time.Substring(0, 2) == "8:" || time.Substring(0, 2) == "9:" || time.Substring(0, 2) == "10") return 0;

        return 0;
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


    // ======================= Changing the language of all text ========================
    public void ChangeLanguage()
    {
        if (changeLanguageText.text == "English")
        {
            changeLanguageText.text = "Русский";

            addLocationText.text = "ДОБАВИТЬ ГОРОД";
            placeHolderText.text = "Введите текст...";
            invalidText.text = "Неправельные символы.";
            nonLocText.text = "Такого города не существует.";
            noPlaceText.text = "Нету места для новой локации.";

            theme.text = "Тема:";
            nightMode.text = "Ночной режим";
            language.text = "Язык:";

            GetDayAndDate(dayAndDate, "ru");

            tempText = "";
            Translator(myCityAndcountry.text, "en", "ru");
            System.Threading.Thread.Sleep(3000);
            myCityAndcountry.text = tempText;

            if (loadHumidityAndWind)
            {
                tempText = myHumidity.text;
                myHumidity.text = "Влажность: " + tempText.Substring(10, 3);

                tempText = myWindSpeed.text;
                myWindSpeed.text = "Скорость ветра: " + tempText.Substring(12, 1) + " м/с";
            }

            for (int i = 0; i < Shelfs.Length; i++)
            {
                todaysText[i].text = todayRU;
                tomorrowsText[i].text = tomorrowRu;

                if (citiesAndCountrys[i].text != "")
                {
                    tempText = "";
                    Translator(citiesAndCountrys[i].text, "en", "ru");
                    System.Threading.Thread.Sleep(3000);
                    citiesAndCountrys[i].text = tempText;
                }
            }
        }
        else
        {
            changeLanguageText.text = "English";

            addLocationText.text = "ADD LOCATION";
            placeHolderText.text = "Enter text...";
            invalidText.text = "Invalid characters are introduced.";
            nonLocText.text = "Non-existent location.";
            noPlaceText.text = "No place for new locations.";

            theme.text = "Theme:";
            nightMode.text = "Night mode";
            language.text = "Language:";

            GetDayAndDate(dayAndDate, "en");

            tempText = "";
            Translator(myCityAndcountry.text, "ru", "en");
            System.Threading.Thread.Sleep(3000);
            myCityAndcountry.text = tempText;

            tempText = myHumidity.text;
            myHumidity.text = "Humidity: " + tempText.Substring(11, 3);

            tempText = myWindSpeed.text;
            myWindSpeed.text = "Wind speed: " + tempText.Substring(16, 1) + " m/s";

            for (int i = 0; i < Shelfs.Length; i++)
            {
                todaysText[i].text = todayEN;
                tomorrowsText[i].text = tomorrowEN;

                if (citiesAndCountrys[i].text != "")
                {
                    tempText = "";
                    Translator(citiesAndCountrys[i].text, "ru", "en");
                    System.Threading.Thread.Sleep(3000);
                    citiesAndCountrys[i].text = tempText;
                }
            }
        }

        loadHumidityAndWind = true;
    }
    // ----------------------------------------------------------------------------------


    // ================================ Translator word =================================
    public async Task Translator(string word, string fromLanguage, string toLanguage)
    {
        string route = "/translate?api-version=3.0&from=" + fromLanguage + "&to=" + toLanguage;
        string textToTranslate = word;
        word = "";
        object[] body = new object[] { new { Text = textToTranslate } };
        var requestBody = JsonConvert.SerializeObject(body);

        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", regionLocation);

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync();
            TreatmentResult(result);
        }
    }

    private void TreatmentResult(string result)
    {
        tempText = "";
        for (int i = 0; i < result.Length; i++)
        {
            if (result.Substring(27 + i, 1) == quote)
                break;
            else
                tempText += result.Substring(27 + i, 1);
        }
    }
    // ----------------------------------------------------------------------------------


    // ==================== What to do when closing the application =====================
#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (bgMode.isOn)
            sv.bgNight = true;
        else
            sv.bgNight = false;

        sv.countShowerShelfs = 0;
        for (int i = 0; i < Shelfs.Length; i++)
        {
            if (Shelfs[i].activeSelf)
            {
                sv.countShowerShelfs++;
            }
        }

        sv.firstShelf = citiesSave[0];
        sv.secondShelf = citiesSave[1];
        sv.thirdShelf = citiesSave[2];
        sv.fourthShelf = citiesSave[3];
        sv.fifthShelf = citiesSave[4];

        sv.language = changeLanguageText.text;

        File.WriteAllText(path, JsonUtility.ToJson(sv));
    }
#endif
    private void OnApplicationQuit()
    {
        if (bgMode.isOn)
            sv.bgNight = true;
        else
            sv.bgNight = false;

        sv.countShowerShelfs = 0;
        for (int i = 0; i < Shelfs.Length; i++)
        {
            if (Shelfs[i].activeSelf)
            {
                sv.countShowerShelfs++;
            }
        }

        sv.firstShelf = citiesSave[0];
        sv.secondShelf = citiesSave[1];
        sv.thirdShelf = citiesSave[2];
        sv.fourthShelf = citiesSave[3];
        sv.fifthShelf = citiesSave[4];

        sv.language = changeLanguageText.text;

        File.WriteAllText(path, JsonUtility.ToJson(sv));
    }
    // ----------------------------------------------------------------------------------
}

[Serializable]
public class Save
{
    public bool bgNight;
    public int countShowerShelfs;
    public string firstShelf;
    public string secondShelf;
    public string thirdShelf;
    public string fourthShelf;
    public string fifthShelf;
    public string language;
}
