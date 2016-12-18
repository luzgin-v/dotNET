using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

namespace WcfSportmaster
{
    public static class SportmasterParseController
    {
        public static List<SportmasterModel.ProductOfSearch> GetProductsWithSearch(string searchQuery)
        {
            _url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + searchQuery + "&pageSize=120";
            List<SportmasterModel.ProductOfSearch> resultSearch = new List<SportmasterModel.ProductOfSearch>();
            try
            {
                GetWebPage();
                HtmlNodeCollection products = _page.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                if (products != null)
                {
                    foreach (HtmlNode product in products)
                    {
                        int startSymbolId = product.Attributes["href"].Value.IndexOf("/product/", StringComparison.Ordinal) + 9;
                        int endSymbolId = product.Attributes["href"].Value.Substring(startSymbolId).IndexOf("/", StringComparison.Ordinal);
                        string id = product.Attributes["href"].Value.Substring(startSymbolId, endSymbolId);
                        var newProduct = new SportmasterModel.ProductOfSearch
                        {
                            Name = product.InnerText,
                            SportmasterId = id
                        };
                        resultSearch.Add(newProduct);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (WebException)
            {
                return null;
            }
            return resultSearch;
        }

        public static SportmasterModel.CachedProduct GetProductFromId(string id)
        {
            SportmasterModel.CachedProduct newProduct;
            _url = "http://www.sportmaster.ru/product/" + id;
                try
                {
                    GetWebPage();
                    HtmlNode nameProductNode =
                        _page.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
                    HtmlNode manufacturerProductNode =
                        _page.DocumentNode.SelectSingleNode("//div[@class='sm-goods_main_logo-holder']/a/img");
                    HtmlNode priceProductNode =
                        _page.DocumentNode.SelectSingleNode("//meta[@itemprop='price']");
                    newProduct = new SportmasterModel.CachedProduct()
                    {
                        Manufacturer = manufacturerProductNode.Attributes["alt"].Value,
                        Name = nameProductNode.InnerText,
                        Price = priceProductNode.Attributes["content"].Value,
                        SportmasterId = id
                    };
                }
                catch (WebException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
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