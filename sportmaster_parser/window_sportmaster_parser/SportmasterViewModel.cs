using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using static window_sportmaster_parser.SportmasterModel;
using static window_sportmaster_parser.SportmasterDbController;
using static window_sportmaster_parser.SportmasterParseController;

namespace window_sportmaster_parser
{
    public class SportmasterViewModel: INotifyPropertyChanged
    {
        public SportmasterViewModel()
        {
            Manufacturers = FillManufacturers();
            IsExecuteParsing = false;
        }

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
                IsExecuteParsing = false;
                SelectedProduct = null;
                CountSearchResults = SearchResult.Count.ToString();
            }
        }

        private ICommand _searchProductCommand;
        public ICommand SearchProductCommand
        {
            get
            {
                return _searchProductCommand ?? (_searchProductCommand = new RelayCommand(async () =>
                 {
                     IsExecuteParsing = true;
                     SearchResult = await Task.Run(() => GetProductsWithSearch(_searchQuery) );
                     IsExecuteParsing = false;
                 }));
            }
        }

        private ICommand _getProductCommand;
        public ICommand GetProductCommand
        {
            get
            {
                return _getProductCommand ?? (_getProductCommand = new RelayCommand(async () =>
                {
                        DisplayedProduct = await Task.Run(() => GetProductFromId(_selectedProduct.SportmasterId));
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
                if (_selectedProduct != null)
                {
                    IsExecuteParsing = true;
                    GetProductCommand.Execute(_selectedProduct);
                    IsExecuteParsing = false;
                }
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
                var checkNewManufacturer = AddFieldToDb(_displayedProduct);
                if (checkNewManufacturer)
                {
                    Manufacturers = FillManufacturers();
                }
                IsExecuteParsing = false;
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
                IsExecuteParsing = false;
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
                ProductsOfManufacturer = FillProductsOfManufacturer(_selectedManufacturer);
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

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}