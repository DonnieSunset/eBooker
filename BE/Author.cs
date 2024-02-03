namespace BE
{
    public class Author
    {
        private string lastName;

        private string firstName;

        public Author(string transferredName)
        {
            var tokens = transferredName.Split(",").ToList();
            if (tokens.Count == 1)
            {
                // transferred name is in the form Boris Helmut Pramotarov
                var names = tokens.First().Trim().Split(" ").ToList();
                if (names.Count() < 2)
                {
                    throw new EbookerException($"Could not tokenize name <{transferredName}>");
                }
                lastName = names.Last().Trim();
                names.RemoveAt(names.Count - 1);
                names.ForEach(x => x = x.Trim());
                firstName = string.Join(' ', names);
            }
            else if (tokens.Count == 2)
            {
                // transferred name is in the form Pramotarov, Boris Helmut
                lastName = tokens.First().Trim();
                firstName = tokens.Last().Trim();
            }
            else
            {
                throw new EbookerException($"Could not tokenize name <{transferredName}>");
            }
        }

        public string DisplayName => $"{firstName} {lastName}";

        public string SortName => $"{lastName}, {firstName}";
    }
}
