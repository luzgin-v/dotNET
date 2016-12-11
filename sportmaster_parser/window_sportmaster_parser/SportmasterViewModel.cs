using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using HtmlAgilityPack;
using static window_sportmaster_parser.SportmasterModel;

namespace window_sportmaster_parser
{
    public class SportmasterViewModel: INotifyPropertyChanged
    {
        public SportmasterViewModel()
        {
            _context = new ProductContext();
            IsExecuteParsing = false;
            Manufacturers = new ObservableCollection<Manufacturer>(_context.Manufacturers.ToList());
        }

        private ProductContext _context;

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                RaisePropertyChanged("SearchQuery");
            }
        }

        private ObservableCollection<ProductOfSearch> _searchResult;
        public ObservableCollection<ProductOfSearch> SearchResult
        {
            get { return _searchResult; } 
            set
            {
                _searchResult = value;
                RaisePropertyChanged("SearchResult");
            }
        }

        private ICommand _searchProductCommand;
        public ICommand SearchProductCommand
        {
            get
            {
                return _searchProductCommand ?? (_searchProductCommand = new RelayCommand(async () =>
                {
                    await Task.Run(() => GetProductsWithSearch());
                }));
            }
        }

        private ProductOfSearch _selectedProduct;
        public ProductOfSearch SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged("SelectedProduct");
                Task.Run(() => GetProductFromId());
            }
        }

        private Product _displayedProduct;
        public Product DisplayedProduct
        {
            get { return _displayedProduct; }
            set
            {
                _displayedProduct = value;
                RaisePropertyChanged("DisplayedProduct");
                AddFieldToDb();
            }
        }

        private bool _isExecuteParsing;
        public bool IsExecuteParsing
        {
            get { return _isExecuteParsing; }
            set
            {
                _isExecuteParsing = value;
                RaisePropertyChanged("IsExecuteParsing");
            }
        }

        private ObservableCollection<Manufacturer> _manufacturers;
        public ObservableCollection<Manufacturer> Manufacturers
        {
            get { return _manufacturers; }
            set
            {
                _manufacturers = value;
                RaisePropertyChanged("Manufacturers");
            }
        }

        private Manufacturer _selectedManufacturer;
        public Manufacturer SelectedManufacturer
        {
            get { return _selectedManufacturer; }
            set
            {
                _selectedManufacturer = value;
                RaisePropertyChanged("SelectedManufacturer");
                FillProductsOfManufacturer();
                CountProductsOfManufacturer = ProductsOfManufacturer.Count.ToString();
            }
        }

        private ObservableCollection<Product> _productsOfManufacturer;
        public ObservableCollection<Product> ProductsOfManufacturer
        {
            get { return _productsOfManufacturer; }
            set
            {
                _productsOfManufacturer = value;
                RaisePropertyChanged("ProductsOfManufacturer");
            }
        }

        private string _countSearchResults;
        public string CountSearchResults
        {
            get { return _countSearchResults; }
            set
            {
                _countSearchResults = value;
                RaisePropertyChanged("CountSearchResults");
            }
        }

        private string _countProductsOfManufacturer;
        public string CountProductsOfManufacturer
        {
            get { return _countProductsOfManufacturer; }
            set
            {
                _countProductsOfManufacturer = value;
                RaisePropertyChanged("CountProductsOfManufacturer");
            }
        }

        public void GetProductsWithSearch()
        {
            string url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + _searchQuery + "&pageSize=120";
            ObservableCollection<ProductOfSearch> resultSearch = new ObservableCollection<ProductOfSearch>();
            IsExecuteParsing = true;
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument htmlDoc = web.Load(url);
                HtmlNodeCollection products = htmlDoc.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
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
                    MessageBox.Show("Не найдено ничего по запросу \"" + _searchQuery + "\"");
                }
            }
            catch (WebException)
            {
                MessageBox.Show("Проблемы с доступом к сайту");
            }
            IsExecuteParsing = false;
            SearchResult = resultSearch;
            SelectedProduct = null;
            CountSearchResults = SearchResult.Count.ToString();
        }
      
        public void GetProductFromId()
        {
            if (_selectedProduct != null)
            {
                IsExecuteParsing = true;
                string url = "http://www.sportmaster.ru/product/" + _selectedProduct.SportmasterId;
                try
                {
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument htmlDoc = web.Load(url);
                    HtmlNode nameProductNode =
                        htmlDoc.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
                    HtmlNode manufacturerProductNode =
                        htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sm-goods_main_logo-holder']/a/img");
                    HtmlNode priceProductNode =
                        htmlDoc.DocumentNode.SelectSingleNode("//meta[@itemprop='price']");
                    var newProduct = new Product
                    {
                        Manufacturer = new Manufacturer()
                        {
                            ManufacturerName = manufacturerProductNode.Attributes["alt"].Value
                        },
                        ProductName = nameProductNode.InnerText,
                        ProductPrice = priceProductNode.Attributes["content"].Value,
                        SportmasterId = _selectedProduct.SportmasterId
                    };
                    DisplayedProduct = newProduct;
            }
                catch (WebException)
            {
                MessageBox.Show("Проблемы с доступом к сайту");
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла проблема при попытке извлечь информацию по ID: \"" + _selectedProduct.SportmasterId + "\"");
            }
            IsExecuteParsing = false;
            }
        }

        public void AddFieldToDb()
        {
            if (_selectedProduct != null)
            {
                var newManufacturer = _context.Manufacturers.Find(_displayedProduct.Manufacturer.ManufacturerName);
                if (newManufacturer == null)
                {
                    newManufacturer = new Manufacturer
                    {
                        ManufacturerName = _displayedProduct.Manufacturer.ManufacturerName
                    };
                    _context.Manufacturers.Add(newManufacturer);
                    _context.SaveChanges();
                    Manufacturers = new ObservableCollection<Manufacturer>(_context.Manufacturers.ToList());
                }
                var newProduct = _displayedProduct;
                newProduct.Manufacturer = newManufacturer;
                if (
                    !_context.Products.Where(p => p.SportmasterId == _displayedProduct.SportmasterId)
                        .Select(c => c)
                        .Any())
                {
                    _context.Products.Add(newProduct);
                    _context.SaveChanges();
                }
            }
        }

        public void FillProductsOfManufacturer()
        {
            ProductsOfManufacturer = new ObservableCollection<Product>(_context.Products.Where(p => p.Manufacturer.ManufacturerName == _selectedManufacturer.ManufacturerName).Select(c => c).ToList());

        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}