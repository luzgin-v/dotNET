using System;
using System.Linq;
using System.Net;
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
                        GetProductFromId(Console.ReadLine());
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
            using (var Db = new ProductContext())
            {
                var ManufacturersList = Db.Manufacturers.Select(m => m.ManufacturerName).ToList();
                if (ManufacturersList.Any())
                {
                    Console.WriteLine("\nСписок производителей:");
                    for (int I = 0; I < ManufacturersList.Count; I++)
                    {
                        Console.WriteLine("{0}. {1}", I + 1, ManufacturersList[I]);
                    }
                    Console.Write("\nДалее можно посмотреть все товары данного производителя. Для этого выберите номер производителя: ");
                    int Number;
                    if (Int32.TryParse(Console.ReadLine(), out Number))
                    {
                        if ((Number + 1 > 0) &(Number - 1 < ManufacturersList.Count))
                        {
                            GetProductsOfManufacturer(ManufacturersList[Number-1]);
                        }
                        else
                        {
                            Console.WriteLine("Введенное значение некорректно.");
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

        private static void GetProductsOfManufacturer(string nameManufacturer)
        {
            using (var Db = new ProductContext())
            {
                var Manufacturer = Db.Manufacturers.Find(nameManufacturer);
                var ProductsOfManufacturer = Db.Products.Where(p => p.Manufacturer.ManufacturerName == nameManufacturer)
                    .Select(c=>c);
                Console.WriteLine("\nСписок товаров производителя {0}:", nameManufacturer);
                foreach (var Product in ProductsOfManufacturer)
                {
                    Console.WriteLine("{0}", Product.ProductName);
                }
                Console.WriteLine();
            }
        }

        private static void GetProductsWithSearch(string searchQuery)
        {
            // ЕСЛИ НЕ РАБОТАЕТ ПОИСК ПРОВЕРЬ ПЕРЕДАВАЕМЫЙ ПАРАМЕТР НА САЙТЕ СПОРТМАСТЕРА
            string Url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + searchQuery + "&pageSize=120";
            try
            {
                HtmlWeb Web = new HtmlWeb();
                HtmlDocument HtmlDoc = Web.Load(Url);
                HtmlNodeCollection Products = HtmlDoc.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                Console.WriteLine();
                if (Products != null)
                {
                    Console.WriteLine("{0,8} | {1}\n--------------------------------------------------------------",
                        "ID", "Название товара");
                    foreach (HtmlNode Product in Products)
                    {
                        int StartSymbolId = Product.Attributes["href"].Value.IndexOf("/product/") + 9;
                        int EndSymbolId = Product.Attributes["href"].Value.Substring(StartSymbolId).IndexOf("/");
                        string Id = Product.Attributes["href"].Value.Substring(StartSymbolId, EndSymbolId);
                        Console.WriteLine("{0, 8} | {1}", Id, Product.InnerText);
                    }
                }
                else
                {
                    Console.WriteLine("Не найдено ничего по запросу \"{0}\"", searchQuery);
                }
            }
            catch (WebException)
            {
                Console.WriteLine("\nПроблемы с доступом к сайту");
            }
            finally
            {
                Console.WriteLine();
            }
        }

        private static void GetProductFromId(string id)
        {
            bool ProductHasCached = false;
            using (var Db = new LiteDatabase(@"cache.db"))
            {
                var Products = Db.GetCollection<CachedProduct>("products");
                var Result = Products.FindOne(x => x.SportmasterId.Equals(id));
                if (Result != null)
                {
                    ProductHasCached = true;
                    Console.WriteLine("\nИнформация о товаре получена из кэша");
                    WriteProductInfo(Result);
                }
            }
            
            if (!ProductHasCached)
            {
                string Url = "http://www.sportmaster.ru/product/" + id;
                try
                {
                    HtmlWeb Web = new HtmlWeb();
                    HtmlDocument HtmlDoc = Web.Load(Url);
                    HtmlNode NameProductNode =
                        HtmlDoc.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
                    HtmlNode ManufacturerProductNode =
                        HtmlDoc.DocumentNode.SelectSingleNode("//div[@class='sm-goods_main_logo-holder']/a/img");
                    HtmlNode PriceProductNode =
                        HtmlDoc.DocumentNode.SelectSingleNode("//meta[@itemprop='price']");
                    
                    var NewCachedProduct = new CachedProduct()
                    {
                        SportmasterId = id,
                        Name = NameProductNode.InnerText,
                        Manufacturer = ManufacturerProductNode.Attributes["alt"].Value,
                        Price = PriceProductNode.Attributes["content"].Value
                    };
                    WriteProductInfo(NewCachedProduct);

                    
                    using (var Db = new LiteDatabase(@"cache.db"))
                    {
                        var Products = Db.GetCollection<CachedProduct>("products");
                        Products.Insert(NewCachedProduct);
                    }

                    using (var Db = new ProductContext())
                    {
                        var NewManufacturer = Db.Manufacturers.Find(NewCachedProduct.Manufacturer);
                        if (NewManufacturer == null)
                        {
                            NewManufacturer = new Manufacturer()
                            {
                                ManufacturerName = NewCachedProduct.Manufacturer
                            };
                        }
                        var NewProduct = new Product()
                        {
                            Manufacturer = NewManufacturer,
                            ProductName = NewCachedProduct.Name,
                            ProductPrice = NewCachedProduct.Price,
                            SportmasterId = NewCachedProduct.SportmasterId
                        };
                        Db.Products.Add(NewProduct);
                        Db.SaveChanges();
                    }
                }
                catch (WebException)
                {
                    Console.WriteLine("\nПроблемы с доступом к сайту");
                }
                catch (Exception)
                {
                    Console.WriteLine("\nВозникла проблема при попытке извлечь информацию по ID: \"{0}\"", id);
                }
            }
            Console.WriteLine("\n");
        }

        private static void WriteProductInfo(CachedProduct product)
        {
            Console.WriteLine();
            Console.WriteLine("{0, 15}: {1}", "Наименование", product.Name);
            Console.WriteLine("{0, 15}: {1}", "Производитель", product.Manufacturer);
            Console.WriteLine("{0, 15}: {1}", "Цена", product.Price);
        }
    }
}
