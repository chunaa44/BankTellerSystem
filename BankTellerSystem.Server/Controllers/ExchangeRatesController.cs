using BankTellerSystem.Server.Contracts;
using BankTellerSystem.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankTellerSystem.Server.Controllers;

[ApiController]
[Route("api/exchange-rates")]
public class ExchangeRatesController(ExchangeRateService exchangeRateService) : ControllerBase
{
    // Blazor currency-rate display: fetches all current rates.
    [HttpGet]
    public async Task<ActionResult<List<ExchangeRateDto>>> GetAll(CancellationToken ct)
    {
        var rates = await exchangeRateService.GetAllAsync(ct);
        return Ok(rates.Select(ExchangeRateDto.FromDomain).ToList());
    }

    // Teller app: updates a currency's buy/sell rate.
    [HttpPut("{currencyCode}")]
    public async Task<ActionResult<ExchangeRateDto>> UpdateRate(string currencyCode, [FromBody] UpdateExchangeRateRequestDto request, CancellationToken ct)
    {
        if (!string.Equals(currencyCode, request.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Route currency code and body currency code must match.");

        try
        {
            var rate = await exchangeRateService.UpdateRateAsync(request.CurrencyCode, request.BuyRate, request.SellRate, ct);
            return Ok(ExchangeRateDto.FromDomain(rate));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}