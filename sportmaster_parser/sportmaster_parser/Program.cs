using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using LiteDB;


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
                Console.WriteLine("3. Получить список производителей сохраненных в базе данных");
                Console.WriteLine("4. Выйти из программы\n");
                Console.Write("Введите на клавиатуре 1, 2, 3 или 4: ");
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
                        GetManufacturersList();
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Неправильно введен пункт меню. Попробуйте еще раз.");
                        break;
                }
            }
        }

        private static void GetManufacturersList()
        {
            using (var db = new ProductsContext())
            {
                var ManufacturersList = db.Manufacturers;
                if (ManufacturersList != null)
                {
                    Console.WriteLine("\nСписок производителей:");
                    int i = 0;
                    Dictionary<int, string> ManufacturersDict = new Dictionary<int, string>();
                    foreach (var Manufacturer in ManufacturersList)
                    {
                        i++;
                        ManufacturersDict.Add(i, Manufacturer.ManufacturerName);
                    }
                    foreach (var Key in ManufacturersDict.Keys)
                    {
                        Console.WriteLine("{0}. {1}", Key, ManufacturersDict[Key]);
                    }
                    Console.Write("\nДалее можно посмотреть все товары данного производителя. Для этого выберите номер производителя: ");
                    int Number;
                    if (Int32.TryParse(Console.ReadLine(), out Number))
                    {
                        if (!ManufacturersDict.ContainsKey(Number))
                        {
                            Console.WriteLine("Key \"doc\" is not found.");
                        }
                        else
                        {
                            GetProductsOfManufacturer(ManufacturersDict[Number]);
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("Введенная строка не является числом");
                    }
                }
                else
                {
                    Console.WriteLine("\nСписок производителей пуст");
                }

            }
        }

        private static void GetProductsOfManufacturer(string NameManufacturer)
        {
            using (var db = new ProductsContext())
            {
                var Manufacturer = db.Manufacturers.Find(NameManufacturer);
                var ProductsOfManufacturer = from p in db.Products
                                             where p.Manufacturer.ManufacturerName == NameManufacturer
                                             select p;
                Console.WriteLine("\nСписок товаров производителя {0}:", NameManufacturer);
                foreach (var Product in ProductsOfManufacturer)
                {
                    Console.WriteLine("{0}", Product.ProductName);
                }
                Console.WriteLine();
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
                Console.WriteLine();
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
                Console.WriteLine();
            }
        }

        private static void GetProductFromID(string id)
        {
            bool ProductHasCached = false;
            using (var db = new LiteDatabase(@"cache.db"))
            {
                var Products = db.GetCollection<CachedProduct>("products");
                var result = Products.Find(x => x.SportmasterID.Equals(id));
                ProductHasCached = result.Any();
                if (ProductHasCached)
                {
                    Console.WriteLine("\nИнформация о товаре получена из кэша");
                    Products.FindOne(x => x.SportmasterID.Equals(id)).WriteProductInfo();
                }
            }
            
            if (!ProductHasCached)
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

                    var NewCachedProduct = new CachedProduct()
                    {
                        SportmasterID = id,
                        Name = NameProductNode.InnerText,
                        Manufacturer = ManufacturerProductNode.Attributes["alt"].Value,
                        Price = PriceProductNode.InnerText.Replace(",", "")
                    };

                    using (var db = new LiteDatabase(@"cache.db"))
                    {
                        var Products = db.GetCollection<CachedProduct>("products");
                        Products.Insert(NewCachedProduct);
                        NewCachedProduct.WriteProductInfo();
                    }

                    using (var db = new ProductsContext())
                    {
                        var NewManufacturer = db.Manufacturers.Find(NewCachedProduct.Manufacturer);
                        if (NewManufacturer == null)
                        {
                            NewManufacturer = new Manufacturer()
                            {
                                ManufacturerName = NewCachedProduct.Manufacturer
                            };
                            db.Manufacturers.Add(NewManufacturer);
                        }
                        var NewProduct = new Product()
                        {
                            Manufacturer = NewManufacturer,
                            ProductName = NewCachedProduct.Name,
                            ProductPrice = NewCachedProduct.Price,
                            SportmasterID = NewCachedProduct.SportmasterID
                        };
                        db.Products.Add(NewProduct);
                        db.SaveChanges();
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("\nПроблемы с доступом к сайту");
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nВозникла проблема при попытке извлечь информацию по ID: \"{0}\"", id);
                }
            }
            Console.WriteLine("\n");
        }
    }

    public class Product
    {
        [Key]
        public string SportmasterID { get; set; }
        public string ProductName { get; set; }
        public string ProductPrice { get; set; }


        public virtual Manufacturer Manufacturer { get; set; }
    }


    public class Manufacturer
    {
        [Key]
        public string ManufacturerName { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

    public class ProductsContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
    }

    public class CachedProduct
    {
        public string SportmasterID { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Price { get; set; }

        public void WriteProductInfo()
        {
            Console.WriteLine();
            Console.WriteLine("{0, 15}: {1}", "Наименование", this.Name);
            Console.WriteLine("{0, 15}: {1}", "Производитель", this.Manufacturer);
            Console.WriteLine("{0, 15}: {1}", "Цена", this.Price);
        }
    }
}
