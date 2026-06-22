namespace BankingPipeline.Helpers;

public static class PiiMasker
{
    public static string MaskAccount(string account) => "ACC-****";
}
