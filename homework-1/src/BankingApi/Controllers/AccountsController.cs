using BankingApi.Models;
using BankingApi.Models.Dtos;
using BankingApi.Services;
using BankingApi.Validators;
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
        if (!TransactionValidator.IsValidAccountId(accountId))
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
            Currency = Currency.USD
        });
    }

    [HttpGet("{accountId}/summary")]
    [ProducesResponseType(typeof(AccountSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetSummary(string accountId)
    {
        accountId = accountId.Trim();
        if (!TransactionValidator.IsValidAccountId(accountId))
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
