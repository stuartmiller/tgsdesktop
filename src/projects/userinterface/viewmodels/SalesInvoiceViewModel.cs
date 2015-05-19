using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace tgsdesktop.viewmodels {

    public interface ISalesInvoiceViewModel : IRoutableViewModel {
        ReactiveList<transaction.CustomerViewModel> Customers { get; }
        ReactiveList<ProductViewModel> Products { get; }

        transaction.CustomerViewModel SelectedCustomer { get; set; }
        ProductViewModel SelectedProduct { get; set; }
        SalesInvoiceItemViewModel CurrentCartItem { get; }

        ReactiveCommand<object> Save { get; }
    }

    public class SalesInvoiceViewModel : ViewModelBase, ISalesInvoiceViewModel {

        public SalesInvoiceViewModel(IScreen screen)
            : base(screen) {

            this.InvoiceNumber = DateTime.Now.ToString("Hmmssff");
            this.SeasonId = infrastructure.IocContainer.Resolve<infrastructure.IGlobalSettingsAccessor>().CurrentSeasonId;
            this.EffectiveDate = DateTime.Now;

            this.Customers = new ReactiveList<transaction.CustomerViewModel>();

            var posService = infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>();
            this.Products = new ReactiveList<ProductViewModel>();
            this.Products.AddRange(posService.GetProducts().Select(p => new ProductViewModel(p)));

            this.Items = new ReactiveList<SalesInvoiceItemViewModel>();
            this.Items.ChangeTrackingEnabled = true;

            var settingsAccessor = infrastructure.IocContainer.Resolve<infrastructure.IGlobalSettingsAccessor>();
            this.SalesTaxRate = settingsAccessor.SalesTaxRate;

            this.WhenAnyObservable(vm => vm.Items.CountChanged)
                .Select(_ => this.Items.Count == 0 ? 0m : this.Items.Sum(i => i.Tax ))
                .ToProperty(this, vm => vm.SalesTax, out _salesTax);
            this.WhenAnyObservable(vm => vm.Items.CountChanged)
                .Select(_ => this.Items.Count == 0 ? 0m : this.Items.Sum(i => i.Total))
                .ToProperty(this, vm => vm.SubTotal, out _subTotal);
            this.WhenAnyObservable(vm => vm.Items.CountChanged)
                .Select(_ => this.Items.Count == 0 ? 0m : this.SubTotal + this.SalesTax)
                .ToProperty(this, vm => vm.Total, out _total);

            this.WhenAnyValue(
                vm => vm.SelectedCustomer,
                vm => vm.Total)
                .Select(x => x.Item1 == null ? x.Item2 : x.Item1.Balance - x.Item2)
                .ToProperty(this, x => x.BalanceLessInvoice, out _balanceLessInvoice);

            this.WhenAnyValue(vm => vm.SelectedProduct)
                .Subscribe(_ => {
                    if (this.CurrentCartItem == null)
                        this.CurrentCartItem = new SalesInvoiceItemViewModel(this.SalesTaxRate);
                    this.CurrentCartItem.ProductId = this.SelectedProduct == null ? null : this.SelectedProduct.Id;
                    this.CurrentCartItem.Description = this.SelectedProduct == null ? this.ProductDescription : this.SelectedProduct.Name;
                    this.CurrentCartItem.UnitPrice = this.SelectedProduct == null ? null : this.SelectedProduct.Price;
                    this.CurrentCartItem.UnitCost = this.SelectedProduct == null ? null : this.SelectedProduct.Cost;
                    this.CurrentCartItem.IsTaxable = this.SelectedProduct == null ? true : this.SelectedProduct.IsTaxable;
                });

            this.SelectedProduct = new ProductViewModel();
            this.AddItem = ReactiveCommand.Create(this.WhenAny(
                vm => vm.CurrentCartItem,
                vm => { return true || !string.IsNullOrEmpty(this.CurrentCartItem.Description); }));
            this.AddItem.Subscribe(_ => {
                var cartItem = new SalesInvoiceItemViewModel(this.SalesTaxRate) {
                    ProductId = this.CurrentCartItem.ProductId,
                    Description = this.ProductDescription,
                    InvoiceId = this.CurrentCartItem.InvoiceId,
                    IsTaxable = this.CurrentCartItem.IsTaxable,
                    Quantity = this.CurrentCartItem.Quantity,
                    UnitCost = this.CurrentCartItem.UnitCost,
                    UnitPrice = this.CurrentCartItem.UnitPrice
                };
                this.Items.Add(cartItem);
                cartItem.RemoveItem.Subscribe(ci => {
                    this.Items.Remove(cartItem);
                });

                this.SelectedProduct = null;
                this.CurrentCartItem = new SalesInvoiceItemViewModel(this.SalesTaxRate) { Quantity = 1 };

            });
            this.Save = this.RegisterNavigationCommand(() =>{
                return new SalesInviceCheckoutViewModel(HostScreen, this);
            });
            this.Cancel = this.RegisterNavigationCommand(() => new SalesInvoiceViewModel(HostScreen));


            // get the customers, and put them in a list
            var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
            this.Customers.AddRange(accountService.GetPeople(
                models.PersonType.Camper
                | models.PersonType.Staff
                | models.PersonType.Other).Select(x => new transaction.CustomerViewModel(x as models.Person)));
        }

        private decimal SalesTaxRate { get; set; }

        public ReactiveList<transaction.CustomerViewModel> Customers { get; private set; }
        public ReactiveList<ProductViewModel> Products { get; private set; }
        public ReactiveList<SalesInvoiceItemViewModel> Items { get; private set; }

        public ReactiveCommand<object> AddItem { get; private set; }
        public ReactiveCommand<object> Save { get; private set; }
        public ReactiveCommand<object> Cancel { get; private set; }

        string _invoiceNumber;
        public string InvoiceNumber { get { return _invoiceNumber; } set { this.RaiseAndSetIfChanged(ref _invoiceNumber, value); } }
        transaction.CustomerViewModel _selectedCustomer;
        public transaction.CustomerViewModel SelectedCustomer { get { return _selectedCustomer; } set { this.RaiseAndSetIfChanged(ref _selectedCustomer, value); } }
        ProductViewModel _selectedProduct;
        public ProductViewModel SelectedProduct { get { return _selectedProduct; } set {
            this.RaiseAndSetIfChanged(ref _selectedProduct, value);
        } }
        string _productDescription;
        public string ProductDescription {
            get { return _productDescription; }
            set { this.RaiseAndSetIfChanged(ref _productDescription, value); }
        }

        SalesInvoiceItemViewModel _currentCartItem;
        public SalesInvoiceItemViewModel CurrentCartItem { get { return _currentCartItem; } set { this.RaiseAndSetIfChanged(ref _currentCartItem, value); } }

        DateTime _effectiveDate;
        public DateTime EffectiveDate { get { return _effectiveDate; } set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); } }

        int _seasonId;
        public int SeasonId { get { return _seasonId; } set { this.RaiseAndSetIfChanged(ref _seasonId, value); } }

        readonly ObservableAsPropertyHelper<decimal> _subTotal;
        public decimal SubTotal { get { return _subTotal.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _salesTax;
        public decimal SalesTax { get { return _salesTax.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _total;
        public decimal Total { get { return _total.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _balanceLessInvoice;
        public decimal BalanceLessInvoice { get { return _balanceLessInvoice.Value; } }

        public override string UrlPathSegment { get { return "posregister"; } }
    }
}
