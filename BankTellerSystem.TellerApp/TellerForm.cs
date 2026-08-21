namespace BankTellerSystem.TellerApp;

public partial class TellerForm : Form
{
    private readonly ApiClient _apiClient;

    // Cached list so a combo box selection can be mapped back to an account id.
    private List<AccountDto> _accounts = [];

    public TellerForm(ApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
        Load += async (_, _) => await RefreshDataAsync();
    }

    private async void btnRefresh_Click(object sender, EventArgs e) => await RefreshDataAsync();

    // Loads accounts (for the transfer dropdowns) and currencies (for the rate dropdown) from the server.
    private async Task RefreshDataAsync()
    {
        try
        {
            _accounts = await _apiClient.GetAccountsAsync();
            var rates = await _apiClient.GetExchangeRatesAsync();

            PopulateAccountCombo(cmbFromAccount);
            PopulateAccountCombo(cmbToAccount);
            PopulateCurrencyCombo(rates);

            Log("Data refreshed.");
        }
        catch (Exception ex)
        {
            Log($"Refresh failed: {ex.Message}");
        }
    }

    private void PopulateAccountCombo(ComboBox combo)
    {
        var selectedId = (combo.SelectedItem as AccountDto)?.Id;
        combo.Items.Clear();
        combo.Items.AddRange([.. _accounts]);
        if (combo.Items.Count == 0) return;

        var indexToSelect = selectedId is null ? 0 : _accounts.FindIndex(a => a.Id == selectedId);
        combo.SelectedIndex = Math.Max(indexToSelect, 0);
    }

    private void PopulateCurrencyCombo(List<ExchangeRateDto> rates)
    {
        var selectedCode = (cmbCurrency.SelectedItem as ExchangeRateDto)?.CurrencyCode;
        cmbCurrency.Items.Clear();
        cmbCurrency.Items.AddRange([.. rates]);
        if (cmbCurrency.Items.Count == 0) return;

        var indexToSelect = selectedCode is null ? 0 : rates.FindIndex(r => r.CurrencyCode == selectedCode);
        cmbCurrency.SelectedIndex = Math.Max(indexToSelect, 0);
    }

    private async void btnCallNext_Click(object sender, EventArgs e)
    {
        try
        {
            var ticket = await _apiClient.CallNextAsync((int)numCounterId.Value);
            lblCurrentTicket.Text = ticket is null ? "Current ticket: (queue empty)" : $"Current ticket: {ticket.Number}";
            Log(ticket is null ? "No waiting tickets." : $"Called {ticket.Number} to counter {numCounterId.Value}.");
        }
        catch (Exception ex)
        {
            Log($"Call next failed: {ex.Message}");
        }
    }

    private async void btnComplete_Click(object sender, EventArgs e)
    {
        try
        {
            var ticket = await _apiClient.CompleteCurrentAsync((int)numCounterId.Value);
            if (ticket is null)
            {
                Log("This counter has no ticket in progress.");
                return;
            }

            lblCurrentTicket.Text = "Current ticket: -";
            Log($"Completed {ticket.Number}.");
        }
        catch (Exception ex)
        {
            Log($"Complete failed: {ex.Message}");
        }
    }

    private async void btnTransfer_Click(object sender, EventArgs e)
    {
        if (cmbFromAccount.SelectedItem is not AccountDto from || cmbToAccount.SelectedItem is not AccountDto to)
        {
            Log("Pick both accounts first.");
            return;
        }

        if (!decimal.TryParse(txtAmount.Text, out var amount))
        {
            Log("Amount must be a number.");
            return;
        }

        try
        {
            var transaction = await _apiClient.TransferAsync(from.Id, to.Id, amount);
            Log($"Transferred {transaction.Amount:N2} from account {transaction.FromAccountId} to {transaction.ToAccountId}.");
            txtAmount.Clear();
            await RefreshDataAsync(); // balances changed
        }
        catch (Exception ex)
        {
            Log($"Transfer failed: {ex.Message}");
        }
    }

    private async void btnUpdateRate_Click(object sender, EventArgs e)
    {
        if (cmbCurrency.SelectedItem is not ExchangeRateDto currentRate)
        {
            Log("Pick a currency first.");
            return;
        }

        if (!decimal.TryParse(txtBuyRate.Text, out var buy) || !decimal.TryParse(txtSellRate.Text, out var sell))
        {
            Log("Buy/sell rate must be numbers.");
            return;
        }

        try
        {
            var updated = await _apiClient.UpdateRateAsync(currentRate.CurrencyCode, buy, sell);
            Log($"{updated.CurrencyCode} rate updated: buy {updated.BuyRate}, sell {updated.SellRate}.");
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            Log($"Rate update failed: {ex.Message}");
        }
    }

    private void cmbCurrency_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbCurrency.SelectedItem is ExchangeRateDto rate)
        {
            txtBuyRate.Text = rate.BuyRate.ToString();
            txtSellRate.Text = rate.SellRate.ToString();
        }
    }

    private void Log(string message) => txtLog.AppendText($"[{DateTime.Now:T}] {message}{Environment.NewLine}");
}