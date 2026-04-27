using BankingApi.Models;
using BankingApi.Models.Dtos;
using BankingApi.Services;
using BankingApi.Validators;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateTransactionRequest request)
    {
        var validationErrors = TransactionValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return BadRequest(new ValidationErrorResponse { Details = validationErrors });
        }

        var type = Enum.Parse<TransactionType>(request.Type, ignoreCase: true);
        var currency = Enum.Parse<Currency>(request.Currency, ignoreCase: true);

        var transaction = _transactionService.Create(
            request.FromAccount,
            request.ToAccount,
            request.Amount,
            currency,
            type);

        var response = TransactionResponse.FromTransaction(transaction);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll(
        [FromQuery] string? accountId,
        [FromQuery] string? type,
        [FromQuery] string? from,
        [FromQuery] string? to)
    {
        var errors = new List<ValidationDetail>();

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            accountId = accountId.Trim();
            if (!TransactionValidator.IsValidAccountId(accountId))
                errors.Add(new ValidationDetail { Field = "accountId", Message = "Account number must follow format ACC-XXXXX (X is alphanumeric)." });
        }

        TransactionType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(type))
        {
            type = type.Trim();
            if (Enum.TryParse<TransactionType>(type, ignoreCase: true, out var t))
                parsedType = t;
            else
                errors.Add(new ValidationDetail { Field = "type", Message = "Type must be deposit, withdrawal, or transfer." });
        }

        DateTime? fromDate = null;
        if (!string.IsNullOrWhiteSpace(from))
        {
            if (DateTime.TryParse(from.Trim(), out var f))
                fromDate = DateTime.SpecifyKind(f, DateTimeKind.Utc);
            else
                errors.Add(new ValidationDetail { Field = "from", Message = "Invalid date format. Use yyyy-MM-dd." });
        }

        DateTime? toDate = null;
        if (!string.IsNullOrWhiteSpace(to))
        {
            if (DateTime.TryParse(to.Trim(), out var t))
                toDate = DateTime.SpecifyKind(t, DateTimeKind.Utc).Date.AddDays(1);
            else
                errors.Add(new ValidationDetail { Field = "to", Message = "Invalid date format. Use yyyy-MM-dd." });
        }

        if (errors.Count > 0)
            return BadRequest(new ValidationErrorResponse { Details = errors });

        var transactions = _transactionService.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(accountId))
            transactions = transactions.Where(t => t.FromAccount == accountId || t.ToAccount == accountId);

        if (parsedType.HasValue)
            transactions = transactions.Where(t => t.Type == parsedType.Value);

        if (fromDate.HasValue)
            transactions = transactions.Where(t => t.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            transactions = transactions.Where(t => t.Timestamp < toDate.Value);

        return Ok(transactions.Select(TransactionResponse.FromTransaction));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(string id)
    {
        id = id.Trim();
        var transaction = _transactionService.GetById(id);

        if (transaction == null)
            return NotFound(new { error = "Transaction not found." });

        return Ok(TransactionResponse.FromTransaction(transaction));
    }
}
