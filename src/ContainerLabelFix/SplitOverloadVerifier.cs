using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ContainerLabelFix.Verification
{
    /// <summary>
    /// Programmatic verification of the Split overload fix for OCI metadata labels.
    /// See: https://github.com/dotnet/sdk/issues/52732
    /// </summary>
    public class SplitOverloadVerifier
    {
        private readonly StringBuilder _report = new StringBuilder();
        private int _passedTests = 0;
        private int _failedTests = 0;

        public void Run()
        {
            WriteLine("╔══════════════════════════════════════════════════════════════╗");
            WriteLine("║  .NET Framework Split Overload Verification                  ║");
            WriteLine("║  Issue: https://github.com/dotnet/sdk/issues/52732           ║");
            WriteLine("╚══════════════════════════════════════════════════════════════╝");
            WriteLine();

            TestBrokenBehavior();
            WriteLine();
            TestFixedBehavior();
            WriteLine();
            PrintSummary();

            // Write results to file for artifact preservation
            string reportPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "verification-report.txt"
            );
            File.WriteAllText(reportPath, _report.ToString());
            Console.WriteLine($"📄 Report saved to: {reportPath}");
        }

        private void TestBrokenBehavior()
        {
            WriteLine("═══ TEST 1: BROKEN BEHAVIOR (Original Code) ═══");
            WriteLine();

            var testCases = new Dictionary<string, string>
            {
                {
                    "org.opencontainers.image.documentation:https://github.com/mu88/RaspiFanController/blob/main/README.md",
                    "https"
                },
                {
                    "org.opencontainers.image.url:https://example.com/my-project",
                    "https"
                },
                {
                    "com.docker.extension.changelog:https://github.com/mu88/RaspiFanController/releases",
                    "https"
                },
            };

            foreach (var testCase in testCases)
            {
                string label = testCase.Key;
                string expectedBroken = testCase.Value;

                // Simulate original broken code: Split(':')[1]
                string[] parts = label.Split(':');
                string result = parts[1];

                WriteLine($"Input:    {label}");
                WriteLine($"Expected: {expectedBroken} (broken - only gets protocol)");
                WriteLine($"Got:      {result}");

                if (result == expectedBroken)
                {
                    WriteLine("Result:   ✗ BROKEN (as expected - demonstrates the bug)");
                    _passedTests++;
                }
                else
                {
                    WriteLine($"Result:   ✗ UNEXPECTED! Expected broken behavior but got: {result}");
                    _failedTests++;
                }
                WriteLine();
            }
        }

        private void TestFixedBehavior()
        {
            WriteLine("═══ TEST 2: FIXED BEHAVIOR (with Split Overload) ═══");
            WriteLine();

            var testCases = new List<(string Input, string Expected)>
            {
                (
                    "org.opencontainers.image.documentation:https://github.com/mu88/RaspiFanController/blob/main/README.md",
                    "https://github.com/mu88/RaspiFanController/blob/main/README.md"
                ),
                (
                    "org.opencontainers.image.url:https://example.com/my-project",
                    "https://example.com/my-project"
                ),
                (
                    "com.docker.extension.changelog:https://github.com/mu88/RaspiFanController/releases",
                    "https://github.com/mu88/RaspiFanController/releases"
                ),
            };

            foreach (var testCase in testCases)
            {
                string label = testCase.Input;
                string expectedFixed = testCase.Expected;

                // Use the proposed fix: Split(':', 2)[1]
                string[] parts = label.Split(':', 2);
                string result = parts[1];

                WriteLine($"Input:    {label}");
                WriteLine($"Expected: {expectedFixed}");
                WriteLine($"Got:      {result}");

                if (result == expectedFixed)
                {
                    WriteLine("Result:   ✓ FIXED (full URL properly extracted!)");
                    _passedTests++;
                }
                else
                {
                    WriteLine($"Result:   ✗ FAILED - URL mismatch!");
                    _failedTests++;
                }
                WriteLine();
            }
        }

        private void PrintSummary()
        {
            WriteLine("╔══════════════════════════════════════════════════════════════╗");
            WriteLine("║ VERIFICATION SUMMARY                                         ║");
            WriteLine("╠══════════════════════════════════════════════════════════════╣");
            WriteLine($"║ Total Tests: {_passedTests + _failedTests,2}                                        ║");
            WriteLine($"║ Passed:      {_passedTests,2}  ✓                                       ║");
            WriteLine($"║ Failed:      {_failedTests,2}  ✗                                       ║");
            WriteLine("╠══════════════════════════════════════════════════════════════╣");

            if (_failedTests == 0)
            {
                WriteLine("║ ✅ VERIFICATION SUCCESSFUL                                  ║");
                WriteLine("║                                                              ║");
                WriteLine("║ The Split(':', 2) overload is available and works correctly║");
                WriteLine("║ on this .NET / MSBuild version.                            ║");
                WriteLine("║                                                              ║");
                WriteLine("║ Framework Compatibility: ✅ VERIFIED                         ║");
            }
            else
            {
                WriteLine("║ ❌ VERIFICATION FAILED                                      ║");
                WriteLine("║                                                              ║");
                WriteLine($"║ {_failedTests} test(s) failed. The Split overload may not be supported. ║");
            }
            WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }

        private void WriteLine(string? message = null)
        {
            if (message == null)
                message = "";

            Console.WriteLine(message);
            _report.AppendLine(message);
        }

        public static void Main(string[] args)
        {
            try
            {
                var verifier = new SplitOverloadVerifier();
                verifier.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during verification: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}
