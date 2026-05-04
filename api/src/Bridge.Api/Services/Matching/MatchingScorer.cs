using Bridge.Api.Dtos.Matching;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;

namespace Bridge.Api.Services.Matching;

/// <summary>
/// 案件×技術者のマッチングスコアを計算する純粋関数群。
/// I/O なし、依存なし。テストが書きやすい構造。
/// </summary>
public static class MatchingScorer
{
    // ---- スコアリング係数(全式の重みを1箇所に集約) ----
    private const int RequiredSkillMatchWeight = 3;
    private const int PreferredSkillMatchWeight = 1;
    private const double RequiredYearsAdequacyBonus = 0.5;
    private const int YearsShortagePenaltyWeight = 1;
    private const int CategoryPreferenceBonus = 2;

    /// <summary>
    /// 案件と技術者の組み合わせに対して、スコアと内訳を計算する。
    /// </summary>
    public static MatchScoreResult Score(
        Project project,
        Engineer engineer)
    {
        // ---- skillEvaluations の生成 ----
        var evaluations = BuildSkillEvaluations(project, engineer);

        // ---- スコア内訳の計算 ----
        var skillMatchScore = CalculateSkillMatchScore(evaluations);
        var yearsAdequacyScore = CalculateYearsAdequacyScore(evaluations);
        var categoryMatchCount = CountCategoryPreferenceMatches(project, engineer);
        var categoryMatchScore = categoryMatchCount * CategoryPreferenceBonus;

        // ---- 生スコアの合算 ----
        var rawScore = skillMatchScore + yearsAdequacyScore + categoryMatchScore;
        if (rawScore < 0) rawScore = 0;

        // ---- 最大可能スコアで正規化 ----
        var maxPossible = CalculateMaxPossibleScore(project);
        var normalizedScore = maxPossible > 0
            ? (int)Math.Round((rawScore / maxPossible) * 100)
            : 0;

        // 内訳も正規化(画面表示用、合計が100に収まるように)
        var breakdown = new ScoreBreakdown
        {
            SkillMatch = NormalizeBreakdownPart(skillMatchScore, maxPossible),
            YearsAdequacy = NormalizeBreakdownPart(yearsAdequacyScore, maxPossible),
            CategoryMatch = NormalizeBreakdownPart(categoryMatchScore, maxPossible),
        };

        return new MatchScoreResult(
            Score: Math.Clamp(normalizedScore, 0, 100),
            MaxPossibleScore: 100,
            Breakdown: breakdown,
            SkillEvaluations: evaluations,
            CategoryPreferenceMatched: categoryMatchCount > 0);
    }

    // ---- 内部:skillEvaluations の生成 ----
    public static List<SkillEvaluation> BuildSkillEvaluations(Project project, Engineer engineer)
    {
        var engineerSkillMap = engineer.Skills.ToDictionary(s => s.SkillId);
        var result = new List<SkillEvaluation>(project.RequiredSkills.Count);

        foreach (var required in project.RequiredSkills
                     .OrderBy(rs => rs.RequirementType)
                     .ThenBy(rs => rs.SkillId))
        {
            var actualYears = engineerSkillMap.TryGetValue(required.SkillId, out var es)
                ? es.Years
                : 0;

            string status;
            if (actualYears == 0) status = "unmet";
            else if (actualYears >= required.RequiredYears) status = "matched";
            else status = "insufficient";

            result.Add(new SkillEvaluation
            {
                SkillId = required.SkillId,
                SkillName = required.Skill.Name,
                Requirement = required.RequirementType == RequirementType.Required ? "required" : "preferred",
                RequiredYears = required.RequiredYears,
                ActualYears = actualYears,
                Status = status,
            });
        }

        return result;
    }

    // ---- 内部:スキル一致のスコア ----
    private static int CalculateSkillMatchScore(List<SkillEvaluation> evaluations)
    {
        var requiredMatched = evaluations.Count(e => e.Requirement == "required" && e.Status == "matched");
        var preferredMatched = evaluations.Count(e => e.Requirement == "preferred" && e.Status == "matched");

        return requiredMatched * RequiredSkillMatchWeight + preferredMatched * PreferredSkillMatchWeight;
    }

    // ---- 内部:年数充足/不足のスコア ----
    private static double CalculateYearsAdequacyScore(List<SkillEvaluation> evaluations)
    {
        double score = 0;

        foreach (var e in evaluations)
        {
            // 必須スキルのみ対象(歓迎の年数差は無視)
            if (e.Requirement != "required") continue;

            if (e.Status == "matched")
            {
                score += RequiredYearsAdequacyBonus;
            }
            else if (e.Status == "insufficient")
            {
                // 不足分はペナルティ
                var shortage = e.RequiredYears - e.ActualYears;
                score -= shortage * YearsShortagePenaltyWeight;
            }
            // unmet はスキル一致スコアで既に評価していないので、ここでは追加減点なし
        }

        return score;
    }

    // ---- 内部:希望カテゴリ一致 ----
    private static int CountCategoryPreferenceMatches(Project project, Engineer engineer)
    {
        // 案件の必須・歓迎スキルが属するカテゴリの集合
        var projectCategories = project.RequiredSkills
            .Select(rs => rs.Skill.Category)
            .Distinct()
            .ToHashSet();

        // 技術者の希望カテゴリ
        var preferredCategories = engineer.PreferredCategories
            .Select(pc => pc.Category)
            .ToHashSet();

        return projectCategories.Intersect(preferredCategories).Count();
    }

    // ---- 内部:最大可能スコアの計算 ----
    public static double CalculateMaxPossibleScore(Project project)
    {
        // 全必須スキルが年数まで充足 + 全歓迎スキルが一致 + 全カテゴリが希望と一致する状態を満点とする。
        var requiredCount = project.RequiredSkills.Count(rs => rs.RequirementType == RequirementType.Required);
        var preferredCount = project.RequiredSkills.Count(rs => rs.RequirementType == RequirementType.Preferred);
        var categoryCount = project.RequiredSkills
            .Select(rs => rs.Skill.Category)
            .Distinct()
            .Count();

        var max = requiredCount * RequiredSkillMatchWeight
                + preferredCount * PreferredSkillMatchWeight
                + requiredCount * RequiredYearsAdequacyBonus
                + categoryCount * CategoryPreferenceBonus;

        return Math.Max(max, 1.0);  // ゼロ除算回避
    }

    // ---- 内部:内訳の各パートを正規化 ----
    private static int NormalizeBreakdownPart(double part, double max)
    {
        if (max <= 0) return 0;
        var normalized = (int)Math.Round((part / max) * 100);
        return Math.Clamp(normalized, 0, 100);
    }
}

public record MatchScoreResult(
    int Score,
    int MaxPossibleScore,
    ScoreBreakdown Breakdown,
    List<SkillEvaluation> SkillEvaluations,
    bool CategoryPreferenceMatched);