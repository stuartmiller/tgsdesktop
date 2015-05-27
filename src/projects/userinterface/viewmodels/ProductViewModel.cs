using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public class ProductViewModel : ReactiveObject {

        public ProductViewModel() {
            // default this to true
            this.IsTaxable = true;
        }
        public ProductViewModel(models.Product product) : this() {

            this.ProductId = product.ProductId;
            this.ItemId = product.ItemId;
            this.Name = product.Name;
            this.Price = product.Price;
            this.Cost = product.Cost;
            this.IsTaxable = product.IsTaxable;
        }

        int? _productId;
        public int? ProductId { get { return _productId; } set { this.RaiseAndSetIfChanged(ref _productId, value); } }

        int? _itemId;
        public int? ItemId { get { return _itemId; } set { this.RaiseAndSetIfChanged(ref _itemId, value); } }

        string _name;
        public string Name { get { return _name; } set { this.RaiseAndSetIfChanged(ref _name, value); } }

        decimal? _price;
        public decimal? Price { get { return _price; } set { this.RaiseAndSetIfChanged(ref _price, value); } }

        decimal? _cost;
        public decimal? Cost { get { return _cost; } set { this.RaiseAndSetIfChanged(ref _cost, value); } }

        bool _isTaxable;
        public bool IsTaxable { get { return _isTaxable; } set { this.RaiseAndSetIfChanged(ref _isTaxable, value); } }

        string _displayName;
        public string DisplayName { get { return _displayName; } set { this.RaiseAndSetIfChanged(ref _displayName, value); } }
    }

    public class SalesInvoiceItemViewModel: ReactiveObject {

        public SalesInvoiceItemViewModel(decimal taxRate) {

            this.Quantity = 1;
            this.IsTaxable = true;

            var total = this.WhenAny(
                vm => vm.Quantity,
                vm => vm.UnitPrice,
                (q, p) => {
                    return q.GetValue().HasValue && p.GetValue().HasValue ?
                        p.GetValue().Value * q.GetValue().Value : 0;
                });
            this._total = total.ToProperty(this, vm => vm.Total);
            //var tax = this.WhenAny(
            //    vm => vm.Quantity,
            //    vm => vm.UnitPrice,
            //    vm => vm.IsTaxable,
            //    (q, p, t) => {
            //        if (!t.GetValue())
            //            return 0;
            //        return q.GetValue().HasValue && p.GetValue().HasValue ?
            //            decimal.Round(((p.GetValue().Value * q.GetValue().Value) * taxRate),2) : 0;
            //    });
            //this._tax = tax.ToProperty(this, vm => vm.Tax);

            this.WhenAnyValue(vm => vm.UnitPrice, vm => vm.Quantity, vm => vm.Description, vm => vm.Description2)
                .Select(x => x.Item1.HasValue && x.Item2.HasValue && (!string.IsNullOrEmpty(x.Item3) || !string.IsNullOrEmpty(x.Item4)))
                .ToProperty(this, x => x.IsComplete, out _isComplete);

            this.RemoveItem = ReactiveCommand.Create();
        }

        int? _productId;
        public int? ProductId {
            get { return _productId; }
            set {
                this.RaiseAndSetIfChanged(ref _productId, value);
            }
        }

        int? _itemId;
        public int? ItemId { get { return _itemId; } set { this.RaiseAndSetIfChanged(ref _itemId, value); } }

        int? _invoiceId;
        public int? InvoiceId { get { return _invoiceId; } set { this.RaiseAndSetIfChanged(ref _invoiceId, value); } }

        string _description;
        public string Description { get { return _description; } set { this.RaiseAndSetIfChanged(ref _description, value); } }

        string _description2;
        public string Description2 { get { return _description2; } set {
            this.RaiseAndSetIfChanged(ref _description2, value);
        } }

        decimal? _unitCost;
        public decimal? UnitCost { get { return _unitCost; } set { this.RaiseAndSetIfChanged(ref _unitCost, value); } }

        decimal? _unitPrice;
        public decimal? UnitPrice { get { return _unitPrice; } set { this.RaiseAndSetIfChanged(ref _unitPrice, value); } }

        decimal _discount;
        public decimal Discount { get { return _discount; } set { this.RaiseAndSetIfChanged(ref _discount, value); } }

        bool _isTaxable;
        public bool IsTaxable { get { return _isTaxable; } set { this.RaiseAndSetIfChanged(ref _isTaxable, value); } }

        int? _quantity;
        public int? Quantity { get { return _quantity; } set { this.RaiseAndSetIfChanged(ref _quantity, value); } }

        readonly ObservableAsPropertyHelper<decimal> _total;
        public decimal Total { get { return _total.Value; } }

        readonly ObservableAsPropertyHelper<bool> _isComplete;
        public bool IsComplete { get { return _isComplete.Value; } }


        public ReactiveCommand<object> RemoveItem { get; private set; }
        
    }
}
