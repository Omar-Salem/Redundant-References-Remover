// Guids.cs
// MUST match guids.h
using System;

namespace OmarGameel.RedundantReferencesRemover
{
    internal static class GuidList
    {
        public const string guidRedundantReferencesRemoverPkgString = "138bcc59-3a71-437e-ab3f-021f445ac140";
        public const string guidRedundantReferencesRemoverCmdSetString = "77e81af3-b9bd-454a-a0b0-e47d17102da7";
        public const string guidToolWindowPersistanceString = "2ecb952f-fa3c-4904-bb23-cd5f3edcc694";

        public static readonly Guid guidRedundantReferencesRemoverCmdSet = new Guid(guidRedundantReferencesRemoverCmdSetString);
    };
}