using QueryQuest.Api.Models;

namespace QueryQuest.Api.Services;

public interface IChallengeValidator
{
    SubmitResultDto Validate(Challenge challenge, QueryResultDto userResult, QueryResultDto expectedResult);
}

public class ChallengeValidator : IChallengeValidator
{
    public SubmitResultDto Validate(Challenge challenge, QueryResultDto userResult, QueryResultDto expectedResult)
    {
        var result = new SubmitResultDto
        {
            UserResult = userResult,
            ExpectedResult = expectedResult,
            ConceptExplanation = challenge.Explanation
        };

        if (!userResult.Success)
        {
            result.IsCorrect = false;
            result.Message = $"Your SQL query returned an error: {userResult.Error}";
            return result;
        }

        // 1. Column count verification
        if (userResult.Columns.Count != expectedResult.Columns.Count)
        {
            result.IsCorrect = false;
            result.Message = $"Column count mismatch: Your query returned {userResult.Columns.Count} column(s) ({string.Join(", ", userResult.Columns)}), but {expectedResult.Columns.Count} column(s) were expected ({string.Join(", ", expectedResult.Columns)}).";
            return result;
        }

        // 2. Column names verification (case-insensitive)
        for (var i = 0; i < expectedResult.Columns.Count; i++)
        {
            var expCol = expectedResult.Columns[i];
            var userCol = userResult.Columns[i];
            if (!string.Equals(expCol, userCol, StringComparison.OrdinalIgnoreCase))
            {
                result.IsCorrect = false;
                result.Message = $"Column name mismatch at column {i + 1}: expected '{expCol}', but got '{userCol}'. Check your column selections or aliases (AS {expCol}).";
                return result;
            }
        }

        // 3. Row count verification
        if (userResult.RowCount != expectedResult.RowCount)
        {
            result.IsCorrect = false;
            result.Message = $"Row count mismatch: Your query produced {userResult.RowCount} row(s), but the expected solution produces {expectedResult.RowCount} row(s). Check your WHERE filters or JOIN conditions.";
            return result;
        }

        // 4. Value comparison
        if (challenge.RequiresExactOrdering)
        {
            // Exact row-by-row sequence comparison
            for (var r = 0; r < expectedResult.RowCount; r++)
            {
                var userRow = userResult.Rows[r];
                var expRow = expectedResult.Rows[r];

                for (var c = 0; c < expectedResult.Columns.Count; c++)
                {
                    if (!AreValuesEqual(userRow[c], expRow[c]))
                    {
                        result.IsCorrect = false;
                        result.Message = $"Incorrect value or ordering at row {r + 1}, column '{expectedResult.Columns[c]}': expected '{FormatVal(expRow[c])}', but got '{FormatVal(userRow[c])}'. This challenge requires exact ORDER BY ordering.";
                        return result;
                    }
                }
            }
        }
        else
        {
            // Set-based equivalence (ignoring row ordering)
            var userRowSignatures = userResult.Rows.Select(GetRowSignature).OrderBy(s => s, StringComparer.Ordinal).ToList();
            var expRowSignatures = expectedResult.Rows.Select(GetRowSignature).OrderBy(s => s, StringComparer.Ordinal).ToList();

            for (var i = 0; i < expRowSignatures.Count; i++)
            {
                if (!string.Equals(userRowSignatures[i], expRowSignatures[i], StringComparison.Ordinal))
                {
                    result.IsCorrect = false;
                    result.Message = "Your query returned the correct number of rows and columns, but the result values do not match the expected solution. Check your calculation expressions or boundary filters.";
                    return result;
                }
            }
        }

        // All checks passed!
        result.IsCorrect = true;
        result.Message = "Outstanding! Your query executed flawlessly and produced the exact expected dataset.";
        result.PointsEarned = challenge.Points;
        return result;
    }

    private static string GetRowSignature(List<object?> row)
    {
        return string.Join("|#|", row.Select(NormalizeValueForSignature));
    }

    private static string NormalizeValueForSignature(object? val)
    {
        if (val == null) return "NULL";

        if (val is double d)
            return Math.Round(d, 4).ToString("0.####");
        if (val is float f)
            return Math.Round((double)f, 4).ToString("0.####");
        if (val is long l)
            return l.ToString();
        if (val is int n)
            return n.ToString();

        var str = val.ToString() ?? "";
        if (double.TryParse(str, out var parsedDouble))
        {
            return Math.Round(parsedDouble, 4).ToString("0.####");
        }

        return str.Trim();
    }

    private static bool AreValuesEqual(object? v1, object? v2)
    {
        if (v1 == null && v2 == null) return true;
        if (v1 == null || v2 == null) return false;

        if (TryGetDouble(v1, out var d1) && TryGetDouble(v2, out var d2))
        {
            return Math.Abs(d1 - d2) < 0.0001;
        }

        return string.Equals(v1.ToString()?.Trim(), v2.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetDouble(object val, out double result)
    {
        if (val is double d) { result = d; return true; }
        if (val is float f) { result = f; return true; }
        if (val is long l) { result = l; return true; }
        if (val is int i) { result = i; return true; }
        return double.TryParse(val.ToString(), out result);
    }

    private static string FormatVal(object? val) => val == null ? "NULL" : val.ToString() ?? "NULL";
}
