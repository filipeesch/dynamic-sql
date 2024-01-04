namespace DynamicSQL.Parser;

using System.Text.RegularExpressions;

public static class RegexDefinitions
{
    public static readonly Regex TemplateTag = new(
        """
        <<                    # Match the opening '<<'
        (                     # Start of capturing group for the entire content
            (?>
                [^<>]+        # Match any characters except '<' or '>'
                |
                <(?!<)        # Match a single '<' not followed by another '<'
                |
                >(?!>)        # Match a single '>' not followed by another '>'
                |
                <<(?<Depth>)  # Match '<<', increment the 'Depth' counter
                |
                >>(?<-Depth>) # Match '>>', decrement the 'Depth' counter
            )*
            (?(Depth)(?!))    # If 'Depth' is not zero, fail the match
        )
        >>                    # Match the closing '>>'
        """,
        RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

    public static readonly Regex ConditionalOperator = new(
        @"\s*{(\d+)}\s*\?\s*([^:]+)\s*(?::(.+))?",
        RegexOptions.Singleline);

    public static readonly Regex Parameters = new(
        @"(?:\s*(=|<>|>|<|>=|<=|\w+)\s+)?{(\d+)}",
        RegexOptions.Singleline);
}
