using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace tgsdesktop.viewmodels {

    public interface ISalesInvoiceViewModel : IRoutableViewModel {
        ReactiveList<transaction.CustomerViewModel> Customers { get; }
        ReactiveList<ProductViewModel> Products { get; }

        transaction.CustomerViewModel SelectedCustomer { get; set; }
        ProductViewModel SelectedProduct { get; set; }
        SalesInvoiceItemViewModel CurrentCartItem { get; }

        ReactiveCommand<object> AddItem { get; }
        ReactiveCommand<object> Save { get; }
    }

    public class SalesInvoiceViewModel : ViewModelBase, ISalesInvoiceViewModel {

        public SalesInvoiceViewModel(IScreen screen)
            : base(screen) {

            this.InvoiceNumber = DateTime.Now.ToString("Hmmssff");
            this.SeasonId = infrastructure.IocContainer.Resolve<infrastructure.IGlobalSettingsAccessor>().CurrentSeasonId;
            this.EffectiveDate = DateTime.Now;

            this.Customers = new ReactiveList<transaction.CustomerViewModel>();
            this.WhenAnyValue(vm => vm.SelectedCustomer).Subscribe(c => {
                if (c == null)
                    this.CustomerInfo = null;
                else {
                    if (c.PersonModel.IsCamper) {
                        var camper = c.PersonModel as models.Camper;
                        var age = camper.Dob.GetAge();
                        var sessionName = camper.Session.Name;
                        var cabinName = camper.Cabin == null ? null : camper.Cabin.Name;
                        this.CustomerInfo = new CustomerInfoViewModel {
                            InfoHeader = c.Name,
                            InfoLine1 = sessionName + (cabinName == null ? string.Empty : " / " + cabinName),
                            InfoLine2 = age.HasValue ? (camper.Dob.Value.ToShortDateString() + " / " + camper.Dob.GetAge().Value.ToString()) : string.Empty,
                            InfoLine3 = camper.Household.City + ", " + camper.Household.StateProvince
                        };
                    }
                }
            });

            var posService = infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>();
            this.Products = new ReactiveList<ProductViewModel>();
            this.Products.AddRange(posService.GetProducts().Select(p => new ProductViewModel(p)));

            this.Items = new ReactiveList<SalesInvoiceItemViewModel>();
            this.Items.ChangeTrackingEnabled = true;

            var settingsAccessor = infrastructure.IocContainer.Resolve<infrastructure.IGlobalSettingsAccessor>();
            this.SalesTaxRate = settingsAccessor.SalesTaxRate;
            this.CurrentCartItem = new SalesInvoiceItemViewModel(this.SalesTaxRate);

            this.WhenAnyObservable(vm => vm.Items.CountChanged)
                .Select(_ => this.Items.Count == 0 ? 0m : decimal.Round((this.Items.Where(i => i.IsTaxable).Sum(i => i.Total) * this.SalesTaxRate), 2, MidpointRounding.AwayFromZero))
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
                    this.CurrentCartItem.ProductId = this.SelectedProduct == null ? null : this.SelectedProduct.Id;
                    this.CurrentCartItem.Description = this.SelectedProduct == null ? null : this.SelectedProduct.Name;
                    this.CurrentCartItem.UnitPrice = this.SelectedProduct == null ? null : this.SelectedProduct.Price;
                    this.CurrentCartItem.UnitCost = this.SelectedProduct == null ? null : this.SelectedProduct.Cost;
                    this.CurrentCartItem.IsTaxable = this.SelectedProduct == null ? true : this.SelectedProduct.IsTaxable;
                });

            this.SelectedProduct = new ProductViewModel();
            this.AddItem = ReactiveCommand.Create(this.WhenAny(
                vm => vm.CurrentCartItem.Description,
                vm => vm.CurrentCartItem.Description2,
                vm => vm.CurrentCartItem.UnitPrice,
                vm => vm.CurrentCartItem.Quantity,
                (d1, d2, p, q) => {
                    return (!string.IsNullOrEmpty(d1.GetValue()) || !string.IsNullOrEmpty(d2.GetValue()))
                        && p.GetValue().HasValue
                        && q.GetValue().HasValue;
                }));
            this.AddItem.Subscribe(_ => {
                var cartItem = new SalesInvoiceItemViewModel(this.SalesTaxRate) {
                    ProductId = this.CurrentCartItem.ProductId,
                    Description = string.IsNullOrEmpty(this.CurrentCartItem.Description) ? this.CurrentCartItem.Description2 : this.CurrentCartItem.Description,
                    InvoiceId = this.CurrentCartItem.InvoiceId,
                    IsTaxable = this.CurrentCartItem.IsTaxable,
                    Quantity = this.CurrentCartItem.Quantity,
                    UnitCost = this.CurrentCartItem.UnitCost,
                    UnitPrice = this.CurrentCartItem.UnitPrice
                };
                var existingCartItem = this.Items.SingleOrDefault(ci => ci.Description == cartItem.Description
                    && ci.Description2 == cartItem.Description2
                    && ci.UnitPrice == cartItem.UnitPrice);
                if (existingCartItem != null) {
                    cartItem.Quantity += existingCartItem.Quantity;
                    var index = this.Items.IndexOf(existingCartItem);
                    this.Items.Remove(existingCartItem);
                    this.Items.Insert(index, cartItem);
                } else
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

            this.RefreshCustomers();

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

        SalesInvoiceItemViewModel _currentCartItem;
        public SalesInvoiceItemViewModel CurrentCartItem { get { return _currentCartItem; } set { this.RaiseAndSetIfChanged(ref _currentCartItem, value); } }

        DateTime _effectiveDate;
        public DateTime EffectiveDate { get { return _effectiveDate; } set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); } }

        int _seasonId;
        public int SeasonId { get { return _seasonId; } set { this.RaiseAndSetIfChanged(ref _seasonId, value); } }

        readonly ObservableAsPropertyHelper<decimal> _subTotal;
        public decimal SubTotal { get { return _subTotal.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _discounts;
        public decimal Discounts { get { return _discounts.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _salesTax;
        public decimal SalesTax { get { return _salesTax.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _total;
        public decimal Total { get { return _total.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _balanceLessInvoice;
        public decimal BalanceLessInvoice { get { return _balanceLessInvoice.Value; } }

        public override string UrlPathSegment { get { return "posregister"; } }

        CustomerInfoViewModel _customerInfo;
        public CustomerInfoViewModel CustomerInfo {
            get { return _customerInfo; }
            set { this.RaiseAndSetIfChanged(ref _customerInfo, value); }
        }

        void RefreshCustomers() {
            var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
            this.Customers.Clear();
            var customers = accountService.GetPeople(models.PersonType.Camper | models.PersonType.Staff)
                .Where(c => {
                    if (c.IsStaff)
                        return true;
                    else {
                        var camper = c as models.Camper;
                        if (camper != null)
                            if(camper.Session.Key == 369)
                                return true;
                    }
                    return false;
                });

            this.Customers.AddRange(customers.Select(x => new transaction.CustomerViewModel(x as models.Person)));
            this.Customers.Reset();
        }

        public class CustomerInfoViewModel {

            public string InfoHeader { get; set; }
            public string InfoLine1 { get; set; }
            public System.Windows.Visibility InfoLine1Visibility { get { return string.IsNullOrEmpty(this.InfoLine1) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
            public string InfoLine2 { get; set; }
            public System.Windows.Visibility InfoLine2Visibility { get { return string.IsNullOrEmpty(this.InfoLine2) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
            public string InfoLine3 { get; set; }
            public System.Windows.Visibility InfoLine3Visibility { get { return string.IsNullOrEmpty(this.InfoLine3) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible; } }
        }
    }
}
