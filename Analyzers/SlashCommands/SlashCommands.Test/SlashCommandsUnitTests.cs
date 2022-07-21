using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = SlashCommands.Test.CSharpCodeFixVerifier<
    TehGM.Analyzers.SlashCommands.SlashCommandsAnalyzer,
    TehGM.Analyzers.SlashCommands.NotEinherjiBaseClassCodeFixProvider>;

namespace TehGM.Analyzers.SlashCommands.Tests
{
    [TestClass]
    public class SlashCommandsUnitTest
    {
    }
}
