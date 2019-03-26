using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CarUpdaterBot.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PuppeteerSharp;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarUpdaterBot.Controllers
{
    public class CarUpdateController : Controller
    {
        private IWebDriver Driver { get; set; }
        private string _uri = @"https://cars.av.by/search?brand_id%5B0%5D=634&model_id%5B0%5D=637&generation_id%5B0%5D=&year_from=2001&year_to=2008&currency=USD&price_from=3300&price_to=4700&body_id=5&engine_volume_min=&engine_volume_max=&driving_id=&mileage_min=&mileage_max=&region_id=&interior_material=&interior_color=&exchange=&search_time=";
        CarContext carContext;
        private ITelegramBotClient bot = BotModel.BotClient;

        public CarUpdateController()
        {
            carContext = new CarContext();
        }

        [Route(@"api/carupdate/index")]
        public async Task Index()
        {
            Logic();
        }

        private void Logic()
        {
            #region Initialize


            Driver = new ChromeDriver(Server.MapPath("~/WebDriver/"));
            Driver.Url = _uri;
            var carList = new List<CarInfo>();

            #endregion

            carList.AddRange(GetCarInfos());

            while (Driver.FindElement(By.XPath("//li[@class='pages-arrows-item']/a")).Text != "←")
            {
                Driver.FindElement(By.XPath("//li[@class='pages-arrows-item']/a")).Click();

                carList.AddRange(GetCarInfos());
            }

            Driver.Quit();

            foreach (var dbCar in carContext.Cars)
            {
                if (carList.Exists(car => car.Url == dbCar.Url))
                {
                    var actualCar = carList.Find(car => car.Url == dbCar.Url);
                    if (!actualCar.Price.Equals(dbCar.Price))
                    {
                        var priceDifference = Convert.ToInt32(actualCar.Price.Replace(" ", "")) -
                                              Convert.ToInt32(dbCar.Price.Replace(" ", ""));
                        var priceDifferenceString = priceDifference > 0
                            ? priceDifference.ToString().Insert(0, "+")
                            : priceDifference.ToString();

                        SendMessage($"{actualCar.Name}\n{actualCar.Price}$\t{priceDifferenceString}$" +
                                          $"\n{actualCar.Description}\n{actualCar.Year}\n{actualCar.Url}");

                        carContext.Cars.Add(actualCar);
                        carContext.Cars.Remove(dbCar);

                        
                    }

                    carList.Remove(actualCar);
                }
            }

            if (carList.Count > 0)
            {
                carContext.Cars.AddRange(carList);
                carContext.SaveChanges();

                foreach (var newCar in carList)
                {
                    SendMessage($"{newCar.Name}\n{newCar.Price}$\n{newCar.Description}\n{newCar.Year}\n{newCar.Url}");
                }
            }
            else
            {
                SendMessage("No new cars appeared");
            }
        }

        private void SendMessage(string message)
        {
             Task.WaitAll(bot.SendTextMessageAsync(
                chatId: new ChatId(355738528),
                text: message
            ));
        }

        private IList<CarInfo> GetCarInfos()
        {
            var carList = new List<CarInfo>();
            var urlList = new List<string>();
            var priceList = new List<string>();
            var nameList = new List<string>();
            var yearList = new List<string>();
            var descrList = new List<string>();

            // Url
            urlList.AddRange(GetElements(@"//div[@class='listing-item-title']/h4/a", "href"));

            // Price
            priceList.AddRange(GetElements("//div[@class='listing-item-price']/small"));

            // Name
            nameList.AddRange(GetElements("//div[@class='listing-item-title']/h4/a"));

            // Year
            yearList.AddRange(GetElements("//div[@class='listing-item-price']/span"));

            // Descr
            descrList.AddRange(GetElements("//div[@class='listing-item-desc']"));

            for (int i = 0; i < urlList.Count; i++)
            {
                carList.Add(new CarInfo
                {
                    Url = urlList[i],
                    Price = priceList[i],
                    Name = nameList[i],
                    Year = yearList[i],
                    Description = descrList[i]
                });
            }

            return carList;
        }

        private IList<string> GetElements(string xpath, string tag = null)
        {
            var list = new List<string>();
            var carUrlElements = Driver.FindElements(By.XPath(xpath));

            foreach (var item in carUrlElements)
            {
                list.Add(tag == null ? item.Text : item.GetAttribute(tag));
            }

            return list;
        }
    }
}