using BE;
using BE.MetaData;

namespace BE_iTest
{
    [TestFixture]
    internal class AuthorTests
    {
        [TestCase(" Marie-Agnes Strack-Zimmermann ", "Marie-Agnes Strack-Zimmermann", "Strack-Zimmermann, Marie-Agnes")]
        [TestCase(" Strack-Zimmermann,    Marie-Agnes ", "Marie-Agnes Strack-Zimmermann", "Strack-Zimmermann, Marie-Agnes")]
        [TestCase("Hela von Sinnens", "Hela von Sinnens", "Sinnens, Hela von")]
        public void Autor_ValidNames(string transferredName, string expectedDisplayName, string expectedSortName)
        {
            var autor = new Author(transferredName);

            Assert.That(autor.DisplayName, Is.EqualTo(expectedDisplayName));
            Assert.That(autor.SortName, Is.EqualTo(expectedSortName));
        }

        [TestCase("Marie-Agnes, Strack, Zimmermann")]
        [TestCase("Strack-Zimmermann")]
        [TestCase("")]
        public void Autor_InvalidNames(string transferredName)
        {
            Assert.That(() => new Author(transferredName), Throws.TypeOf<EbookerException>());
        }
    }
}
