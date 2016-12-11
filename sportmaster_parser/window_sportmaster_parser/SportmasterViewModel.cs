using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
                //if (_searchProductCommand == null)
                //{
                //    _searchProductCommand = new AsyncDelegateCommand(GetProductsWithSearchAsync);
                //}
                //return _searchProductCommand;

                //return _searchProductCommand ?? (_searchProductCommand = new RelayCommand(() =>
                //{
                //    SearchResult = new ObservableCollection<ProductOfSearch>(SportmasterParserFunctions.GetProductsWithSearch(SearchQuery));
                //}));
                
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
                IsExecuteParsing = true;
                Task.Run(() => GetProductFromId());
                IsExecuteParsing = false;
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
                ProductsOfManufacturer = new ObservableCollection<Product>(_context.Products.Where(p => p.Manufacturer.ManufacturerName == _selectedManufacturer.ManufacturerName).Select(c => c).ToList());
                CountProductsOfManufacturer = "Всего: " + ProductsOfManufacturer.Count.ToString();
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
            string Url = "http://www.sportmaster.ru/catalog/product/search.do?text=" + _searchQuery + "&pageSize=120";
            ObservableCollection<ProductOfSearch> ResultSearch = new ObservableCollection<ProductOfSearch>();
            IsExecuteParsing = true;
            try
            {
                HtmlWeb Web = new HtmlWeb();
                HtmlDocument HtmlDoc = Web.Load(Url);
                HtmlNodeCollection Products = HtmlDoc.DocumentNode.SelectNodes("//div[@class=\"sm-category__item \"]/h2/a");
                if (Products != null)
                {
                    foreach (HtmlNode Product in Products)
                    {
                        int StartSymbolId = Product.Attributes["href"].Value.IndexOf("/product/", StringComparison.Ordinal) + 9;
                        int EndSymbolId = Product.Attributes["href"].Value.Substring(StartSymbolId).IndexOf("/", StringComparison.Ordinal);
                        string Id = Product.Attributes["href"].Value.Substring(StartSymbolId, EndSymbolId);
                        ProductOfSearch NewProduct = new ProductOfSearch
                        {
                            Name = Product.InnerText,
                            SportmasterId = Id
                        };
                        ResultSearch.Add(NewProduct);
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
            SearchResult = ResultSearch;
            SelectedProduct = null;
            CountSearchResults = "Всего: " + SearchResult.Count.ToString();
        }

        public void GetProductFromId()
        {
            if (_selectedProduct != null)
            {
                IsExecuteParsing = true;
                string Url = "http://www.sportmaster.ru/product/" + _selectedProduct.SportmasterId;
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
                    Product NewProduct;

                    var NewManufacturer = _context.Manufacturers.Find(ManufacturerProductNode.Attributes["alt"].Value);
                    if (NewManufacturer == null)
                    {
                        NewManufacturer = new Manufacturer
                        {
                            ManufacturerName = ManufacturerProductNode.Attributes["alt"].Value
                        };
                        _context.Manufacturers.Add(NewManufacturer);
                        _context.SaveChanges();
                        Manufacturers = new ObservableCollection<Manufacturer>(_context.Manufacturers.ToList());
                    }
                    NewProduct = new Product
                    {
                        Manufacturer = NewManufacturer,
                        ProductName = NameProductNode.InnerText,
                        ProductPrice = PriceProductNode.Attributes["content"].Value,
                        SportmasterId = _selectedProduct.SportmasterId
                    };
                    if (! _context.Products.Where(p => p.SportmasterId == _selectedProduct.SportmasterId).Select(c => c).Any())
                    {
                        _context.Products.Add(NewProduct);
                        _context.SaveChanges();
                    }
                    DisplayedProduct = NewProduct;
                }
                catch (WebException)
                {
                    MessageBox.Show("Проблемы с доступом к сайту");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Возникла проблема при попытке извлечь информацию по ID: \"" + _selectedProduct.SportmasterId + "\"");
                }
                IsExecuteParsing = false;
            }
        }

        //public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //}

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}