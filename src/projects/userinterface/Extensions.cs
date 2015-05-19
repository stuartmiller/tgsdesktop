using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace tgsdesktop {
    public static class Extensions {

        public static string ConvertToUnSecureString(this System.Security.SecureString secstrPassword) {
            if (secstrPassword == null)
                return null;
            IntPtr unmanagedString = IntPtr.Zero;
            try {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secstrPassword);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
            } finally {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
        public static void ToPerson(this models.Person person, models.Person target){
            if (person == null)
                return;
            if (target == null)
                target = new models.Person();
            target.Id = person.Id;
            target.LastName = person.LastName;
            target.FirstName = person.FirstName;
            target.NickName = person.NickName;
            target.Household = person.Household;
            target.IsCamper = person.IsCamper;
            target.IsStaff = person.IsStaff;
            target.IsParent = person.IsParent;
            target.Dob = person.Dob;
            target.GenderId = person.GenderId;
            target.Type = person.Type;
            target.Balance = person.Balance;
        }

    }

    public static class FocusExtension {

        public static bool GetIsFocused(DependencyObject obj) {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value) {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
             "IsFocused", typeof(bool), typeof(FocusExtension),
             new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d != null && d is Control) {
                var _Control = d as Control;
                if ((bool)e.NewValue) {
                    // To set false value to get focus on control. if we don't set value to False then we have to set all binding
                    //property to first False then True to set focus on control.
                    OnLostFocus(_Control, null);
                    _Control.Focus(); // Don't care about false values.
                }
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e) {
            if (sender != null && sender is Control) {
                (sender as Control).SetValue(IsFocusedProperty, false);
            }
        }
    }
}
