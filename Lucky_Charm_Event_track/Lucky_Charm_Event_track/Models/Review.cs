using Lucky_Charm_Event_track.Enums;

namespace Lucky_Charm_Event_track.Models
{
    public class Review
    {
        public int Id { get; set; }
        //ranking from 1-5 starts
        public int OverallExperience { get; set; }
        //ranking from 1-5 starts
        public int LikehoodToRecommendTheEvent { get; set; }
        public BasicFeedbackAnswers DidTheEventMeetExpectations { get; set; }
        public BasicFeedbackAnswers WasTheEventWorthTheCost { get; set; }
        public int EaseOfCheckinRanking { get; set; }
        public int SatisfactionRankingForVenue { get; set; }

        public int StaffRankingScore { get; set; }
        public string WhatCanBeImproved { get; set; }
        public string AdditionalComments { get; set; }
        public UserAccount UserAccount { get; set; }
        public int UserAccountID { get; set; }
        public Event Event { get; set; }
        public int EventID { get; set; }

    }
}
