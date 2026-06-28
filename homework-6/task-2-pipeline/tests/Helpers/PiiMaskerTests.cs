using BankingPipeline.Helpers;
using Xunit;

namespace BankingPipeline.Tests.Helpers;

public class PiiMaskerTests
{
    [Fact]
    public void MaskAccount_ReturnsAccMask()
    {
        Assert.Equal("ACC-****", PiiMasker.MaskAccount("ACC-1001"));
    }

    [Fact]
    public void MaskAccount_ReturnsMaskForAnyInput()
    {
        Assert.Equal("ACC-****", PiiMasker.MaskAccount("ACC-9999"));
        Assert.Equal("ACC-****", PiiMasker.MaskAccount("SOME-OTHER-ACCOUNT"));
    }

    [Fact]
    public void MaskAccount_HandlesEmptyString()
    {
        Assert.Equal("ACC-****", PiiMasker.MaskAccount(string.Empty));
    }
}
