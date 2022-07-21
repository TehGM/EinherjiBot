using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = TehGM.Analyzers.StatusPlaceholder.Tests.CSharpCodeFixVerifier<
    TehGM.Analyzers.StatusPlaceholder.StatusPlaceholderAnalyzer,
    TehGM.Analyzers.StatusPlaceholder.MissingInterfaceCodeFixProvider>;

namespace TehGM.Analyzers.StatusPlaceholder.Tests
{
    [TestClass]
    public class StatusPlaceholderUnitTest
    {
    }
}
