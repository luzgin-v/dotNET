using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using static window_sportmaster_parser.SportmasterModel;

namespace window_sportmaster_parser
{
    public static class SportmasterParseController
    {
        public async static Task<ObservableCollection<ProductOfSearch>> GetProductsWithSearch(string searchQuery)
        {
            _url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + searchQuery + "&pageSize=120";
            ObservableCollection<ProductOfSearch> resultSearch = new ObservableCollection<ProductOfSearch>();
            try
            {
                await Task.Run(() => GetWebPage());
                HtmlNodeCollection products = _page.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                if (products != null)
                {
                    foreach (HtmlNode product in products)
                    {
                        int startSymbolId = product.Attributes["href"].Value.IndexOf("/product/", StringComparison.Ordinal) + 9;
                        int endSymbolId = product.Attributes["href"].Value.Substring(startSymbolId).IndexOf("/", StringComparison.Ordinal);
                        string id = product.Attributes["href"].Value.Substring(startSymbolId, endSymbolId);
                        var newProduct = new ProductOfSearch
                        {
                            Name = product.InnerText,
                            SportmasterId = id
                        };
                        resultSearch.Add(newProduct);
                    }
                }
                else
                {
                    MessageBox.Show("Не найдено ничего по запросу \"" + searchQuery + "\"");
                }
            }
            catch (WebException)
            {
                MessageBox.Show("Проблемы с доступом к сайту");
            }
            return resultSearch;
        }

        public async static Task<Product> GetProductFromId(string id)
        {
            Product newProduct = null;
            _url = "http://www.sportmaster.ru/product/" + id;
                try
                {
                    await Task.Run(() => GetWebPage());
                    HtmlNode nameProductNode =
                        _page.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
                    HtmlNode manufacturerProductNode =
                        _page.DocumentNode.SelectSingleNode("//div[@class='sm-goods_main_logo-holder']/a/img");
                    HtmlNode priceProductNode =
                        _page.DocumentNode.SelectSingleNode("//meta[@itemprop='price']");
                    newProduct = new Product
                    {
                        Manufacturer = new Manufacturer()
                        {
                            ManufacturerName = manufacturerProductNode.Attributes["alt"].Value
                        },
                        ProductName = nameProductNode.InnerText,
                        ProductPrice = priceProductNode.Attributes["content"].Value,
                        SportmasterId = id
                    };
                    
                }
                catch (WebException)
                {
                    MessageBox.Show("Проблемы с доступом к сайту");
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла проблема при попытке извлечь информацию по ID: \"" + id + "\"");
                }
            return newProduct;
        }

        private static string _url;
        private static HtmlDocument _page;
        private static void GetWebPage()
        {
            HtmlWeb web = new HtmlWeb();
            _page = web.Load(_url);
        }
    }
}