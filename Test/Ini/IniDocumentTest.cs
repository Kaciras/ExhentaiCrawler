using System.IO;
using System.Threading.Tasks;
using Cli.Ini;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Ini
{
	[TestClass]
	public class IniDocumentTest
	{
		[TestMethod]
		public async Task ReadIniFile()
		{
			using var reader = new StreamReader("WebArchive/IniTokenizerTest.ini");
			var doc = await IniDocument.Parse(reader, 32);
			Assert.AreEqual(2, doc.Sections.Count);

			var kacirasSection = doc.Sections["Kaciras"];
			Assert.AreEqual("Handsome", kacirasSection["Looks"]);
		}
	}
}
