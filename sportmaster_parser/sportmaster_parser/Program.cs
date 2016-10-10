using System;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace sportmaster_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetProductFromID("1779410");
            //GetProductsWithSearch("гиря");
            Console.WriteLine();
            while (true)
            {
                Console.WriteLine("Выберите пункт из меню: ");
                Console.WriteLine("1. Получить инфомрацию о товаре по ID");
                Console.WriteLine("2. Получить список товаров по искомому слову");
                Console.WriteLine("3. Выйти из программы\n");
                Console.Write("Введите на клавиатуре 1, 2 или 3: ");
                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Введите ID: ");
                        GetProductFromID(Console.ReadLine());
                        break;
                    case "2":
                        Console.Write("Введите поисковый запрос: ");
                        GetProductsWithSearch(Console.ReadLine());
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Неправильно введен пункт меню. Попробуйте еще раз.");
                        break;
                }
            }
        }

        private static void GetProductsWithSearch(string SearchQuery)
        {
            StringBuilder url = new StringBuilder("http://www.sportmaster.ru/catalog/product/search.do?SearchQuery=");
            url.Append(SearchQuery);
            url.Append("&pageSize=120");
            url.ToString();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmldoc = web.Load(url.ToString());
                HtmlNodeCollection Products =
                    htmldoc.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                Console.WriteLine("\n");
                if (Products != null)
                {
                    Console.WriteLine("{0,8} | {1}\n--------------------------------------------------------------",
                        "ID", "Название товара");
                    foreach (HtmlNode Product in Products)
                    {
                        int StartSymbolID = Product.Attributes["href"].Value.IndexOf("/product/") + 9;
                        int EndSymbolID = Product.Attributes["href"].Value.Substring(StartSymbolID).IndexOf("/");
                        string id = Product.Attributes["href"].Value.Substring(StartSymbolID, EndSymbolID);
                        Console.WriteLine("{0, 8} | {1}", id, Product.InnerText);
                    }

                }
                else
                {
                    Console.WriteLine("Не найдено ничего по запросу \"{0}\"", SearchQuery);
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("\nПроблемы с доступом к сайту");
            }
            finally
            {
                Console.WriteLine("\n");
            }
        }

        private static void GetProductFromID(string id)
        {
            StringBuilder url = new StringBuilder("http://www.sportmaster.ru/product/");
            url.Append(id);
            url.ToString();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmldoc = web.Load(url.ToString());
                HtmlNode NameProductNode =
                    htmldoc.DocumentNode.SelectSingleNode("/html/body/div[4]/div[2]/div[2]/div[3]/h1");
                HtmlNode ManufacturerProductNode =
                    htmldoc.DocumentNode.SelectSingleNode("/ html/body/div[4]/div[2]/div[2]/div[2]/div/div[1]/a/img");
                HtmlNode PriceProductNode =
                    htmldoc.DocumentNode.SelectSingleNode(
                        "/html/body/div[4]/div[2]/div[2]/div[3]/div[3]/div/div/div/div/text()");

                Console.WriteLine("\n");
                Console.WriteLine("{0, 15}: {1}", "Наименование", NameProductNode.InnerText);
                Console.WriteLine("{0, 15}: {1}", "Производитель", ManufacturerProductNode.Attributes["alt"].Value);
                Console.WriteLine("{0, 15}: {1}", "Цена", PriceProductNode.InnerText.Replace(",", ""));
            }
            catch (WebException ex)
            {
                Console.WriteLine("\nПроблемы с доступом к сайту");
            }
            catch (Exception e)
            {
                Console.WriteLine("\nВозникла проблема при попытке извлечь информацию по ID: \"{0}\"", id);
            }
            finally
            {
                Console.WriteLine("\n");
            }
        }

    }
}
