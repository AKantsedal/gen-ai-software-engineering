using System.Text.RegularExpressions;
using BankingApi.Models;
using BankingApi.Models.Dtos;

namespace BankingApi.Validators;

public static class TransactionValidator
{
    private static readonly Regex AccountPattern = new(@"^ACC-[A-Za-z0-9]{5}$", RegexOptions.Compiled);

    private static readonly string[] ValidTypes = { "deposit", "withdrawal", "transfer" };

    public static bool IsValidAccountId(string accountId) => AccountPattern.IsMatch(accountId);

    public static List<ValidationDetail> Validate(CreateTransactionRequest request)
    {
        var errors = new List<ValidationDetail>();

        ValidateType(request.Type, errors);
        ValidateAmount(request.Amount, errors);
        ValidateCurrency(request.Currency, errors);
        ValidateAccounts(request, errors);

        return errors;
    }

    private static void ValidateType(string type, List<ValidationDetail> errors)
    {
        if (string.IsNullOrWhiteSpace(type) ||
            !ValidTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationDetail
            {
                Field = "type",
                Message = "Transaction type must be deposit, withdrawal, or transfer."
            });
        }
    }

    private static void ValidateAmount(decimal amount, List<ValidationDetail> errors)
    {
        if (amount < 0) // BUG: should be <= 0 (allows zero-amount transactions)
        {
            errors.Add(new ValidationDetail
            {
                Field = "amount",
                Message = "Amount must be a positive number."
            });
        }
        else if (decimal.Round(amount, 2) != amount)
        {
            errors.Add(new ValidationDetail
            {
                Field = "amount",
                Message = "Amount must have at most 2 decimal places."
            });
        }
    }

    private static void ValidateCurrency(string currency, List<ValidationDetail> errors)
    {
        if (string.IsNullOrWhiteSpace(currency) || !Enum.TryParse<Currency>(currency, ignoreCase: true, out _))
        {
            errors.Add(new ValidationDetail
            {
                Field = "currency",
                Message = "Invalid currency code."
            });
        }
    }

    private static void ValidateAccounts(CreateTransactionRequest request, List<ValidationDetail> errors)
    {
        var type = request.Type?.ToLower();

        // fromAccount: required for withdrawal and transfer
        if (type is "withdrawal" or "transfer")
        {
            if (string.IsNullOrWhiteSpace(request.FromAccount) ||
                !AccountPattern.IsMatch(request.FromAccount))
            {
                errors.Add(new ValidationDetail
                {
                    Field = "fromAccount",
                    Message = "Account number must follow format ACC-XXXXX (X is alphanumeric)."
                });
            }
        }

        // toAccount: required for deposit and transfer
        if (type is "deposit" or "transfer")
        {
            if (string.IsNullOrWhiteSpace(request.ToAccount) ||
                !AccountPattern.IsMatch(request.ToAccount))
            {
                errors.Add(new ValidationDetail
                {
                    Field = "toAccount",
                    Message = "Account number must follow format ACC-XXXXX (X is alphanumeric)."
                });
            }
        }
    }
}
