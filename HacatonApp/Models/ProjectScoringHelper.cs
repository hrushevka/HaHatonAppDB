namespace HacatonApp.Models
{
    public static class ProjectScoringHelper
    {
        public static float CalculateTotalScore(IEnumerable<Criteria> criteria, IEnumerable<ProjectCriterionScore> scores)
        {
            var scoreMap = scores
                .GroupBy(score => score.CriteriaId)
                .ToDictionary(group => group.Key, group => group.Last().Score);

            float totalWeight = 0;
            float weightedNormalizedScore = 0;

            foreach (var criterion in criteria)
            {
                if (criterion.Weight <= 0 || criterion.MaxScore <= 0)
                {
                    continue;
                }

                totalWeight += criterion.Weight;
                var rawScore = scoreMap.TryGetValue(criterion.Id, out var value) ? value : 0;
                var normalizedScore = Math.Clamp(rawScore / criterion.MaxScore, 0, 1);
                weightedNormalizedScore += normalizedScore * criterion.Weight;
            }

            if (totalWeight <= 0)
            {
                return 0;
            }

            return MathF.Round(weightedNormalizedScore / totalWeight * 100f, 2);
        }

        public static float CalculateAverageScore(IEnumerable<ProjectReview> reviews)
        {
            var reviewList = reviews.ToList();
            if (!reviewList.Any())
            {
                return 0;
            }

            return MathF.Round(reviewList.Average(review => review.TotalScore), 2);
        }
    }
}
