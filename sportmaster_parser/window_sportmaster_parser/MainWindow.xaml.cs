using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HtmlAgilityPack;

namespace window_sportmaster_parser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ProductOfSearch> ResultSearch;
        private List<Product> ProductsOfManufacturer;
        public MainWindow()
        {
            ResultSearch = new List<ProductOfSearch>
            {
                new ProductOfSearch() {Name = "312", SportmasterId = "31"}
            };
            ProductsOfManufacturer = new List<Product>
            {
                new Product()
            };
            InitializeComponent();
            BindingManufacturers();
        }

        private void SearchQuery_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchProducts();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchProducts();
        }

        private void ResultListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductOfSearch SelectedProduct = (ProductOfSearch) ResultListView.SelectedValue;
            GetProductFromId(SelectedProduct.SportmasterId);
        }

        private void ManufacturerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetProductsOfManufacturer((string) ManufacturerComboBox.SelectedValue);
            ProductsOfManufacturerListView.ItemsSource = ProductsOfManufacturer;
            CountProductsOfManufacturer.Text = "Всего у " + ManufacturerComboBox.SelectedValue + ": " + ProductsOfManufacturer.Count.ToString();
        }

        private void SearchProducts()
        {
            ResultSearch.Clear();
            GetProductsWithSearch(SearchQuery.Text);
            ResultListView.ItemsSource = ResultSearch;
            CountSearch.Text = "Найдено " + ResultSearch.Count.ToString() + " товаров";
            ResultListView.Items.Refresh();
        }

        private void BindingManufacturers()
        {
            ProductContext Db = new ProductContext();
            var ManufacturersList = Db.Manufacturers.Select(m => m.ManufacturerName).ToList();
            ManufacturerComboBox.ItemsSource = ManufacturersList;
        }

        private void GetProductFromId(string id)
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
                SelectedName.Text = NameProductNode.InnerText;
                SelectedManufacturer.Text = ManufacturerProductNode.Attributes["alt"].Value;
                SelectedPrice.Text = PriceProductNode.Attributes["content"].Value;
                using (var Db = new ProductContext())
                {
                    var NewManufacturer = Db.Manufacturers.Find(ManufacturerProductNode.Attributes["alt"].Value);
                    if (NewManufacturer == null)
                    {
                        NewManufacturer = new Manufacturer()
                        {
                            ManufacturerName = ManufacturerProductNode.Attributes["alt"].Value
                        };
                        Db.Manufacturers.Add(NewManufacturer);
                        Db.SaveChanges();
                        BindingManufacturers();
                    }
                    var NewProduct = new Product()
                    {
                        Manufacturer = NewManufacturer,
                        ProductName = NameProductNode.InnerText,
                        ProductPrice = PriceProductNode.Attributes["content"].Value,
                        SportmasterId = id
                    };
                    Db.Products.Add(NewProduct);
                    Db.SaveChanges();
                }
            }
            catch (WebException)
            {
                MessageForUser.Text = "Проблемы с доступом к сайту";
            }
            catch (Exception)
            {
                MessageForUser.Text = "Возникла проблема при попытке извлечь информацию по ID: \"" + id + "\"";
            }
        }

        private void GetProductsWithSearch(string searchQuery)
        {
            string Url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + searchQuery + "&pageSize=120";
            try
            {
                HtmlWeb Web = new HtmlWeb();
                HtmlDocument HtmlDoc = Web.Load(Url);
                HtmlNodeCollection Products = HtmlDoc.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                if (Products != null)
                {
                    foreach (HtmlNode Product in Products)
                    {
                        int StartSymbolId = Product.Attributes["href"].Value.IndexOf("/product/") + 9;
                        int EndSymbolId = Product.Attributes["href"].Value.Substring(StartSymbolId).IndexOf("/");
                        string Id = Product.Attributes["href"].Value.Substring(StartSymbolId, EndSymbolId);
                        ProductOfSearch NewProduct = new ProductOfSearch()
                        {
                            Name = Product.InnerText,
                            SportmasterId = Id
                        };
                        ResultSearch.Add(NewProduct);
                    }
                }
                else
                {
                    MessageForUser.Text = "Не найдено ничего по запросу \"" + searchQuery + "\"";
                }
            }
            catch (WebException)
            {
                MessageForUser.Text = "Проблемы с доступом к сайту";
            }
        }
        private void GetProductsOfManufacturer(string nameManufacturer)
        {
            using (var Db = new ProductContext())
            {
                var Manufacturer = Db.Manufacturers.Find(nameManufacturer);
                ProductsOfManufacturer = new List<Product>(Db.Products.Where(p => p.Manufacturer.ManufacturerName == nameManufacturer).Select(c => c).ToList());
            }
        }
    }
    public class ProductOfSearch
    {
        public string SportmasterId { get; set; }
        public string Name { get; set; }
    }
}
