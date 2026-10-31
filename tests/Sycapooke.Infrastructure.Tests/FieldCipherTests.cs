// Acceptance Test
// Traces to: L2-054
// Description: Personal fields are sealed with AES-GCM under the current key, read back after a key
// rotation, left alone when plain, and never decrypted with a missing key or a tampered value.

using System.Security.Cryptography;
using Sycapooke.Infrastructure.Privacy;

namespace Sycapooke.Infrastructure.Tests;

public sealed class FieldCipherTests
{
    private static readonly byte[] KeyOne = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
    private static readonly byte[] KeyTwo = Enumerable.Range(40, 32).Select(i => (byte)i).ToArray();

    [Fact]
    public void A_value_is_sealed_and_read_back_and_two_seals_differ()
    {
        var cipher = new FieldCipher([("k1", KeyOne)]);

        var first = cipher.Protect("12 Mango Lane")!;
        var second = cipher.Protect("12 Mango Lane")!;

        Assert.StartsWith("enc1:k1:", first, StringComparison.Ordinal);
        Assert.DoesNotContain("Mango", first, StringComparison.Ordinal);
        Assert.NotEqual(first, second);
        Assert.Equal("12 Mango Lane", cipher.Unprotect(first));
    }

    [Fact]
    public void After_a_rotation_old_values_still_read_and_new_ones_use_the_new_key()
    {
        var before = new FieldCipher([("k1", KeyOne)]).Protect("Kingston")!;
        var rotated = new FieldCipher([("k2", KeyTwo), ("k1", KeyOne)]);

        Assert.Equal("Kingston", rotated.Unprotect(before));
        Assert.StartsWith("enc1:k2:", rotated.Protect("Kingston"), StringComparison.Ordinal);
    }

    [Fact]
    public void Plain_values_and_nulls_pass_through_and_no_key_means_no_sealing()
    {
        var cipher = new FieldCipher([("k1", KeyOne)]);
        var open = new FieldCipher([]);

        Assert.Equal("legacy plain text", cipher.Unprotect("legacy plain text"));
        Assert.Null(cipher.Protect(null));
        Assert.Equal("unchanged", open.Protect("unchanged"));
        Assert.False(open.IsEnabled);
    }

    [Fact]
    public void A_missing_key_or_a_tampered_value_is_never_decrypted()
    {
        var sealedValue = new FieldCipher([("k1", KeyOne)]).Protect("secret")!;
        var tampered = sealedValue[..^4] + "AAAA";

        Assert.Throws<CryptographicException>(() => new FieldCipher([("k2", KeyTwo)]).Unprotect(sealedValue));
        Assert.ThrowsAny<CryptographicException>(() => new FieldCipher([("k1", KeyOne)]).Unprotect(tampered));
    }

    [Fact]
    public void Configuration_must_be_id_and_a_32_byte_base64_key()
    {
        var good = FieldCipher.FromConfiguration("k1:" + Convert.ToBase64String(KeyOne));

        Assert.True(good.IsEnabled);
        Assert.Throws<InvalidOperationException>(() => FieldCipher.FromConfiguration("k1:" + Convert.ToBase64String(new byte[16])));
        Assert.Throws<InvalidOperationException>(() => FieldCipher.FromConfiguration("nokey"));
        Assert.False(FieldCipher.FromConfiguration(null).IsEnabled);
    }
}
