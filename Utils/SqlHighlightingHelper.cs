using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Xml;

namespace SqlIdeProject.Utils
{
    public static class SqlHighlightingHelper
    {
        public static void Register()
        {
            // Якщо вже зареєстровано - виходимо
            if (HighlightingManager.Instance.GetDefinition("CustomSQL") != null)
                return;

            string sqlXshd = @"
<SyntaxDefinition name=""SQL"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Comment"" foreground=""Green"" />
    <Color name=""String"" foreground=""Red"" />
    
    <RuleSet>
        <Span color=""Comment"" begin=""--"" />
        
        
        <Span color=""Comment"" multiline=""true"" begin=""/\*"" end=""\*/"" />
        
        <Span color=""String"">
            <Begin>'</Begin>
            <End>'</End>
        </Span>

        <Keywords foreground=""Blue"" fontWeight=""bold"">
            <Word>SELECT</Word>
            <Word>FROM</Word>
            <Word>WHERE</Word>
            <Word>INSERT</Word>
            <Word>INTO</Word>
            <Word>UPDATE</Word>
            <Word>DELETE</Word>
            <Word>VALUES</Word>
            <Word>CREATE</Word>
            <Word>TABLE</Word>
            <Word>DROP</Word>
            <Word>IF</Word>
            <Word>EXISTS</Word>
            <Word>NOT</Word>
            <Word>NULL</Word>
            <Word>PRIMARY</Word>
            <Word>KEY</Word>
            <Word>INT</Word>
            <Word>INTEGER</Word>
            <Word>TEXT</Word>
            <Word>REAL</Word>
            <Word>VARCHAR</Word>
            <Word>AND</Word>
            <Word>OR</Word>
            <Word>ORDER</Word>
            <Word>BY</Word>
            <Word>LIMIT</Word>
        </Keywords>
        
        <Rule foreground=""DarkRed"">
            \b0[xX][0-9a-fA-F]+|(\b\d+(\.[0-9]+)?|\.[0-9]+)([eE][+-]?[0-9]+)?
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

            using (var reader = new StringReader(sqlXshd.Trim()))
            {
                using (var xmlReader = XmlReader.Create(reader))
                {
                    var customHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
                    HighlightingManager.Instance.RegisterHighlighting("CustomSQL", new string[] { ".sql" }, customHighlighting);
                }
            }
        }
    }
}