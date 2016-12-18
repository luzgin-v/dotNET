using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using HtmlAgilityPack;
using webAPI_sportmaster_parser.Models;

namespace webAPI_sportmaster_parser.Controllers
{
    public class SportmasterController : ApiController
    {
        public IHttpActionResult GetProductsByQuery(string query)
        {
            _url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + query + "&pageSize=120";
            List<ProductOfSearch> resultSearch = new List<ProductOfSearch>();
            try
            {
                GetWebPage();
                var products = _page.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                if (products != null)
                {
                    foreach (HtmlNode product in products)
                    {
                        int startSymbolId = product.Attributes["href"].Value.IndexOf("/product/", StringComparison.Ordinal) + 9;
                        int endSymbolId = product.Attributes["href"].Value.Substring(startSymbolId).IndexOf("/", StringComparison.Ordinal);
                        string id = product.Attributes["href"].Value.Substring(startSymbolId, endSymbolId);
                        resultSearch.Add(new ProductOfSearch
                        {
                            Name = product.InnerText,
                            SportmasterId = id
                        });
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (WebException)
            {
                return NotFound();
            }
            return Ok(resultSearch);
        }

        public IHttpActionResult GetProductById(string id)
        {
            var newProduct = SportmasterDbController.GetCachedProduct(id);
            if (newProduct == null)
            {
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
                    newProduct = new CachedProduct
                    {
                        Manufacturer = manufacturerProductNode.Attributes["alt"].Value,
                        Name = nameProductNode.InnerText,
                        Price = priceProductNode.Attributes["content"].Value,
                        SportmasterId = id
                    };

                }
                catch (WebException)
                {
                    return NotFound();
                }
                catch (Exception)
                {
                    return NotFound();
                }
                SportmasterDbController.AddNewProduct(newProduct);
            }
            return Ok(newProduct);
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