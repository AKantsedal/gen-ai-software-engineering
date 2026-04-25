using System.Text.RegularExpressions;
using BankingApi.Models.Dtos;
using BankingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers;

[ApiController]
[Route("accounts")]
public class AccountsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public AccountsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{accountId}/balance")]
    [ProducesResponseType(typeof(AccountBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetBalance(string accountId)
    {
        accountId = accountId.Trim();
        if (!Regex.IsMatch(accountId, @"^ACC-[A-Za-z0-9]{5}$"))
            return BadRequest(new ValidationErrorResponse
            {
                Details = new List<ValidationDetail>
                {
                    new() { Field = "accountId", Message = "Account number must follow format ACC-XXXXX (X is alphanumeric)." }
                }
            });

        var balance = _transactionService.GetBalance(accountId);

        return Ok(new AccountBalanceResponse
        {
            AccountId = accountId,
            Balance = balance,
            Currency = "USD"
        });
    }

    [HttpGet("{accountId}/summary")]
    [ProducesResponseType(typeof(AccountSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetSummary(string accountId)
    {
        accountId = accountId.Trim();
        if (!Regex.IsMatch(accountId, @"^ACC-[A-Za-z0-9]{5}$"))
            return BadRequest(new ValidationErrorResponse
            {
                Details = new List<ValidationDetail>
                {
                    new() { Field = "accountId", Message = "Account number must follow format ACC-XXXXX (X is alphanumeric)." }
                }
            });

        return Ok(_transactionService.GetSummary(accountId));
    }
}
