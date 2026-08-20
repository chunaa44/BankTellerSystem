namespace BankTellerSystem.DispenserApp;

// The single screen a customer sees at the bank entrance: one button to
// take a number, a label showing what they got (or an error).
public partial class DispenserForm : Form
{
    private readonly TicketApiClient _apiClient;

    public DispenserForm(TicketApiClient apiClient)
    {
        InitializeComponent(); // builds the button/label - see DispenserForm.Designer.cs
        _apiClient = apiClient;
    }

    private async void _takeNumberButton_Click(object? sender, EventArgs e)
    {
        // Disable the button while the request is in flight so an impatient
        // customer can't fire off duplicate tickets by mashing it.
        _takeNumberButton.Enabled = false;
        _resultLabel.ForeColor = Color.Black;
        _resultLabel.Text = "Printing...";

        try
        {
            var ticket = await _apiClient.IssueTicketAsync();
            _resultLabel.Text = $"Your number: {ticket.Number}";
        }
        catch (Exception ex)
        {
            // Anything from a dead network to a server error lands here -
            // the customer just needs to know to try again.
            _resultLabel.ForeColor = Color.Red;
            _resultLabel.Text = "Could not print a ticket. Please try again.";
            Console.Error.WriteLine(ex);
        }
        finally
        {
            _takeNumberButton.Enabled = true;
        }
    }
}