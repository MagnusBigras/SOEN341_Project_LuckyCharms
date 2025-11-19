using Lucky_Charm_Event_track.Models;
using Xunit;

namespace Lucky_Charm_Event_track.Tests
{
    public class ReviewUnitTest
    {
        // Test that the OverallExperience rating falls within the valid range (1–5)
        [Fact]
        public void Review_OverallExperience_IsWithinValidRange()
        {
            var review = new Review { OverallExperience = 4 };
            Assert.InRange(review.OverallExperience, 1, 5);
        }

        // Test that the recommendation rating also falls within the valid range (1–5)
        [Fact]
        public void Review_RecommendationRating_IsWithinValidRange()
        {
            var review = new Review { LikehoodToRecommendTheEvent = 5 };
            Assert.InRange(review.LikehoodToRecommendTheEvent, 1, 5);
        }
    }
}
