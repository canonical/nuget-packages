// Copyright (C) 2026 Canonical Ltd.
//
// SPDX-License-Identifier: GPL-3.0-only
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License version 3, as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY
// QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along with this
// program.  If not, see http://www.gnu.org/licenses/.

namespace Canonical.Dpkg.UnitTests;

public class DpkgVersionToStringFormatTests
{
    // Versions used across tests:
    //   full:     "1:2+really1-3ubuntu4"  → epoch=1, upstream=2+really1, revision=3ubuntu4
    //   no-epoch: "2+really1-3ubuntu4"    → epoch omitted, upstream=2+really1, revision=3ubuntu4
    //   native:   "1:2"                   → epoch=1, upstream=2, no revision
    //   plain:    "2"                     → epoch omitted, upstream=2, no revision

    private static DpkgVersion Full    => Parse("1:2+really1-3ubuntu4");
    private static DpkgVersion NoEpoch => Parse("2+really1-3ubuntu4");
    private static DpkgVersion Native  => Parse("1:2");
    private static DpkgVersion Plain   => Parse("2");

    // ── G (general / passthrough) ──────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("G")]
    [InlineData("E:U-R")]
    public void ToString_NullEmptyOrG_ReturnsSameAsToString(string? format)
    {
        Assert.Equal(Full.ToString(), Full.ToString(format));
        Assert.Equal(NoEpoch.ToString(), NoEpoch.ToString(format));
    }

    // ── G specifier inside a format string ────────────────────────────────────

    [Fact]
    public void ToString_GSpecifier_InsertsOriginalString()
    {
        Assert.Equal("1:2+really1-3ubuntu4", Full.ToString("G"));
    }

    // ── epoch delimiter ':' ───────────────────────────────────────────────────

    [Fact]
    public void ToString_EpochDelimiter_AppendedOnlyWhenEpochPresent()
    {
        Assert.Equal(":", Full.ToString(":"));
        Assert.Equal("", NoEpoch.ToString(":"));
    }

    // ── revision delimiter '-' ────────────────────────────────────────────────

    [Fact]
    public void ToString_RevisionDelimiter_AppendedOnlyWhenRevisionPresent()
    {
        Assert.Equal("2+really1-3ubuntu4", Full.ToString("U-R"));
        Assert.Equal("2",                  Plain.ToString("U-R"));
    }

    // ── E (Epoch string, null-safe) ───────────────────────────────────────────

    [Fact]
    public void ToString_E_AppendsEpochStringOrNothing()
    {
        Assert.Equal("1",  Full.ToString("E"));
        Assert.Equal("",   Plain.ToString("E"));
    }

    [Theory]
    [InlineData("E1", "1")]        // 1 digit padding → "1"
    [InlineData("E2", "01")]       // pad to 2 → "01"
    [InlineData("E3", "001")]      // pad to 3 → "001"
    public void ToString_EWithDigit_PadsEpochWhenPresent(string format, string expected)
    {
        Assert.Equal(expected, Full.ToString(format));
    }

    [Fact]
    public void ToString_EWithDigit_EpochEmptyWhenEpochAbsent()
    {
        for (int i = 0; i < 10; ++i)
        {
            Assert.Equal(string.Empty, NoEpoch.ToString($"E{i}"));
            Assert.Equal(string.Empty, Plain.ToString($"E{i}"));
        }

    }

    // ── e (EpochValue numeric) ────────────────────────────────────────────────

    [Fact]
    public void ToString_LowercaseE_AppendsEpochValue()
    {
        Assert.Equal("1", Full.ToString("e"));
        Assert.Equal("0", Plain.ToString("e"));   // DEFAULT_EPOCH_VALUE = 0
    }

    [Theory]
    [InlineData("e0", "1", "")]
    [InlineData("e1", "1", "0")]
    [InlineData("e2", "01", "00")]
    [InlineData("e3", "001", "000")]
    public void ToString_LowercaseEWithDigit_Pads(string format, string expectedFull, string expectedPlain)
    {
        Assert.Equal(expectedFull, Full.ToString(format));
        Assert.Equal(expectedPlain, Plain.ToString(format));
    }

    // ── U / U0–U4 (upstream version variants) ────────────────────────────────

    [Fact]
    public void ToString_U_AppendsFullUpstreamVersion()
    {
        // U and U0 are equivalent
        Assert.Equal("2+really1", Full.ToString("U"));
        Assert.Equal("2+really1", Full.ToString("U0"));

        Assert.Equal("2", Plain.ToString("U"));
        Assert.Equal("2", Plain.ToString("U0"));
    }

    [Fact]
    public void ToString_U1_AppendsRevertedUpstreamVersion()
    {
        Assert.Equal("2",  Full.ToString("U1"));       // has +really → reverted part
        Assert.Equal("",   Plain.ToString("U1"));      // no +really → null → empty
    }

    [Fact]
    public void ToString_U2_AppendsRealUpstreamDelimiterWhenPresent()
    {
        Assert.Equal("+really", Full.ToString("U2"));
        Assert.Equal("",        Plain.ToString("U2"));
    }

    [Fact]
    public void ToString_U3_AppendsRealUpstreamVersion()
    {
        Assert.Equal("1",  Full.ToString("U3"));       // real upstream = "1"
        Assert.Equal("",   Plain.ToString("U3"));      // no +really → null → empty
    }

    [Fact]
    public void ToString_U4_AppendsEffectiveUpstreamVersion()
    {
        // EffectiveUpstreamVersion = RealUpstreamVersion ?? UpstreamVersion
        Assert.Equal("1",        Full.ToString("U4"));    // RealUpstreamVersion = "1"
        Assert.Equal("2",        Plain.ToString("U4"));   // RealUpstreamVersion = null → UpstreamVersion
    }

    [Fact]
    public void ToString_U5_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Full.ToString("U5"));
    }

    // ── R / R0–R2 (revision variants) ────────────────────────────────────────

    [Fact]
    public void ToString_R_AppendsFullRevision()
    {
        Assert.Equal("3ubuntu4", Full.ToString("R"));
        Assert.Equal("3ubuntu4", Full.ToString("R0"));
        Assert.Equal("",         Native.ToString("R"));   // no revision
    }

    [Fact]
    public void ToString_R1_AppendsDebianRevision()
    {
        Assert.Equal("3",  Full.ToString("R1"));
        Assert.Equal("",   Native.ToString("R1"));
    }

    [Fact]
    public void ToString_R2_AppendsUbuntuRevisionDelimiter()
    {
        Assert.Equal("ubuntu",  Full.ToString("R2"));
        Assert.Equal("",   Native.ToString("R2"));
    }

    [Fact]
    public void ToString_R3_AppendsUbuntuRevision()
    {
        Assert.Equal("4",  Full.ToString("R3"));
        Assert.Equal("",   Native.ToString("R3"));
    }

    [Fact]
    public void ToString_R4_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Full.ToString("R4"));
    }

    // ── escape sequences ──────────────────────────────────────────────────────

    [Fact]
    public void ToString_BackslashEscape_InsertsLiteralCharacter()
    {
        Assert.Equal("E", Full.ToString(@"\E"));    // \E → literal 'E', not epoch
        Assert.Equal("R", Full.ToString(@"\R"));
        Assert.Equal("U", Full.ToString(@"\U"));
        Assert.Equal(":", Full.ToString(@"\:"));
        Assert.Equal("-", Full.ToString(@"\-"));
    }

    [Fact]
    public void ToString_BackslashAtEndOfFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Full.ToString(@"\"));
    }

    [Fact]
    public void ToString_SingleQuotedLiteral_InsertedVerbatim()
    {
        Assert.Equal("hello", Full.ToString("'hello'"));
        Assert.Equal("E",     Full.ToString("'E'"));    // quoted → literal, not epoch
    }

    [Fact]
    public void ToString_DoubleQuotedLiteral_InsertedVerbatim()
    {
        Assert.Equal("world", Full.ToString("\"world\""));
        Assert.Equal("R",     Full.ToString("\"R\""));
    }

    [Fact]
    public void ToString_UnclosedQuote_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Full.ToString("'unclosed"));
        Assert.Throws<FormatException>(() => Full.ToString("\"unclosed"));
    }

    // ── unknown specifier ─────────────────────────────────────────────────────

    [Fact]
    public void ToString_UnknownSpecifier_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Full.ToString("X"));
        Assert.Throws<FormatException>(() => Full.ToString("Z"));
    }

    // ── composite format strings ──────────────────────────────────────────────

    [Fact]
    public void ToString_CompositeFormat_BuildsExpectedString()
    {
        // epoch:upstreamVersion-revision  (canonical form)
        Assert.Equal("1:2+really1-3ubuntu4", Full.ToString("E:U-R"));
        Assert.Equal("2+really1-3ubuntu4",   NoEpoch.ToString("E:U-R"));
        Assert.Equal("1:2",                  Native.ToString("E:U-R"));
    }

    [Fact]
    public void ToString_CompositeWithPaddedEpoch_BuildsExpectedString()
    {
        Assert.Equal("001:2+really1-3ubuntu4", Full.ToString("E3:U-R"));
        Assert.Equal("2+really1-3ubuntu4", NoEpoch.ToString("E3:U-R"));
    }

    [Theory]
    [InlineData("1:2+really1-3ubuntu4", "\"Version: '\"E:U-R'\\''", "Version: '1:2+really1-3ubuntu4'")]
    public void ToString_WithFormatString_BuildsExpectedString(string versionString, string format, string expected)
    {
        var version = Parse(versionString);
        Assert.Equal(expected, version.ToString(format));
    }

    private static DpkgVersion Parse(string version) => DpkgVersion.Parse(version);
}
