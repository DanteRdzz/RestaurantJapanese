// Helpers/PasswordBoxHelper.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RestaurantJapanese.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BindablePasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindablePassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnBindablePasswordChanged));

        public static string GetBindablePassword(DependencyObject obj) =>
            (string)obj.GetValue(BindablePasswordProperty);

        public static void SetBindablePassword(DependencyObject obj, string value) =>
            obj.SetValue(BindablePasswordProperty, value);

        private static void OnBindablePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox pwd) return;
            var newVal = e.NewValue as string ?? string.Empty;

            // Evita loop
            if (pwd.Password != newVal)
                pwd.Password = newVal;

            pwd.PasswordChanged -= PwdOnPasswordChanged;
            pwd.PasswordChanged += PwdOnPasswordChanged;
        }

        private static void PwdOnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var pwd = (PasswordBox)sender;
            // Refleja lo escrito por el usuario hacia el VM
            SetBindablePassword(pwd, pwd.Password);
        }
    }
}
