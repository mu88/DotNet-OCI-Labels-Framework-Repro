# .NET Framework OCI Labels Split Overload Verification

This repository contains a minimal reproduction case to verify that the proposed fix for [dotnet/sdk#52732](https://github.com/dotnet/sdk/issues/52732) works correctly with .NET Framework and MSBuild.

## Problem

The OCI metadata labels are broken when using URLs with colons (e.g., `https://`) in properties like `ContainerDocumentationUrl`.

When the code uses `.Split(':')[1]`, it only extracts the protocol part (`https`) instead of the full URL.

## Proposed Solution

Using the `Split` overload with a limit parameter: `.Split(':', 2)[1]`

This splits on **only the first colon**, ensuring the URL is properly extracted.

## Verification

This repo tests whether the `Split` overload with limit parameter is available in:
- ✅ .NET (modern)
- ❓ .NET Framework (via MSBuild)

## Structure

- `src/ContainerLabelFix/` - Minimal MSBuild targets file testing the Split logic
- `.github/workflows/` - CI workflow to test on various platforms
- Tests validate both old behavior (broken) and new behavior (fixed)

## Running Locally

```bash
dotnet build /p:TestSplitOverload=true
```

## GitHub Actions

The workflow runs on Windows and Linux to verify the fix works on both platforms with different MSBuild versions.
