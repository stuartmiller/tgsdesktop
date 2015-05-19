using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class OrderShipment {

        string[] _addresses;

        public OrderShipment() {
            this._addresses = new string[]{ null, null, null, null, null };
            this.Items = new List<OrderItem>();
        }

        public int OrderID { get; set; }

        public string ShippingLine1 { get { return _addresses[0]; } set { _addresses[0] = string.IsNullOrWhiteSpace(value) ? null : value; } }
        public string ShippingLine2 { get { return _addresses[1]; } set { _addresses[1] = string.IsNullOrWhiteSpace(value) ? null : value; } }
        public string ShippingLine3 { get { return _addresses[2]; } set { _addresses[2] = string.IsNullOrWhiteSpace(value) ? null : value; } }
        public string ShippingLine4 { get { return _addresses[3]; } set { _addresses[3] = string.IsNullOrWhiteSpace(value) ? null : value; } }
        public string ShippingLine5 { get { return _addresses[4]; } set { _addresses[4] = string.IsNullOrWhiteSpace(value) ? null : value; } }

        public List<OrderItem> Items { get; private set; }
        public class OrderItem {
            public int Quantity { get; set; }
            public string Description { get; set; }
        }

        public List<OrderItem> GetItems() { return this.Items; }
    }

    public class OrderShipments {

        [DataObjectMethod(DataObjectMethodType.Select)]
        public IList<models.OrderShipment> GetOrderShipments() {
            var orders = new List<models.OrderShipment> {
                new models.OrderShipment{
                    OrderID = 1001,
                    ShippingLine1 = "MRS JAMES MILLER",
                    ShippingLine2 = "21 CAMP GREYSTONE LN",
                    ShippingLine3 = "ZIRCONIA, NC 287831"
                },
                new models.OrderShipment{
                    OrderID = 1002,
                    ShippingLine1 = "MRS Johanna Johnson",
                    ShippingLine2 = "43 VICTORY LN",
                    ShippingLine3 = "MURPHYSBOROUGH, TN 287831"
                }
            };

            orders[0].Items.AddRange(new List<OrderShipment.OrderItem> {
                new OrderShipment.OrderItem {
                    Quantity = 2,
                    Description = "Classic Greystone Shirt (Green)"
                },
                new OrderShipment.OrderItem {
                    Quantity = 2,
                    Description = "Hinkster (Fox)"
                }
            });
            return orders;
        }

    }
}
