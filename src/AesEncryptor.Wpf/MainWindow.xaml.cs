// <copyright file="MainWindow.xaml.cs" company="Davide Nava">
// Copyright (c) Davide Nava. All rights reserved.
// </copyright>

using System.Windows;

namespace AesEncryptor.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Click event of the BtnDecrypt control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>>
    private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TxtDecryptResult.Text = !string.IsNullOrWhiteSpace(TxtPws.Text)
                ? AesEncryptionService.DecryptByPassword(TxtDecrypt.Text, TxtPws.Text)
                : AesEncryptionService.DecryptByKey(TxtDecrypt.Text, TxtKey.Text, TxtIv.Text);
        }
        catch (Exception ex)
        {
            TxtDecryptResult.Text = TxtDecrypt.Text;
            _ = MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Click event of the BtnEncrypt control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TxtEncryptResult.Text = !string.IsNullOrWhiteSpace(TxtPws.Text)
                ? AesEncryptionService.EncryptByPassword(TxtEncrypt.Text, TxtPws.Text)
                : AesEncryptionService.EncryptByKey(TxtEncrypt.Text, TxtKey.Text);
        }
        catch (Exception ex)
        {
            TxtEncryptResult.Text = TxtEncrypt.Text;
            _ = MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the TextChanged event of the TxtEncrypt control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void TxtEncrypt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        BtnEncrypt.IsEnabled = !string.IsNullOrWhiteSpace(TxtEncrypt.Text) && BtnIsEnabled();

    /// <summary>
    /// Handles the TextChanged event of the TxtDecrypt control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void TxtDecrypt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        BtnDecrypt.IsEnabled = !string.IsNullOrWhiteSpace(TxtDecrypt.Text) && BtnIsEnabled();

    /// <summary>
    /// Handles the TextChanged event of the TxtIv control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void TxtIv_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        BtnEncrypt.IsEnabled = BtnDecrypt.IsEnabled = BtnIsEnabled();

    /// <summary>
    /// Handles the TextChanged event of the TxtKey control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void TxtKey_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        BtnEncrypt.IsEnabled = BtnDecrypt.IsEnabled = BtnIsEnabled();

    /// <summary>
    /// Handles the TextChanged event of the TxtPws control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void TxtPws_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        BtnEncrypt.IsEnabled = BtnDecrypt.IsEnabled = BtnIsEnabled();

    /// <summary>
    /// Determines whether the encrypt and decrypt buttons should be enabled.
    /// </summary>
    /// <returns>true if the buttons should be enabled; otherwise, false.</returns>
    private bool BtnIsEnabled() => !string.IsNullOrWhiteSpace(TxtPws.Text) ||
                                   (!string.IsNullOrWhiteSpace(TxtKey.Text) && !string.IsNullOrWhiteSpace(TxtIv.Text));
}
