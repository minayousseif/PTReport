using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
///  Pivotal Tracker Activities Structure 
/// </summary>
/// 
namespace PTDailyReport
{
    public struct Changes_NewValues
    {
        public int story_id { get; set; }
        public string text { get; set; }
        public string big_url { get; set; }
        public string thumbnail_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public struct Changes_OriginalValues
    {
        public int story_id { get; set; }
        public string text { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public struct Changes
    {
        public string kind { get; set; }
        public string change_type { get; set; }
        public int id { get; set; }
        public Changes_NewValues new_values { get; set; }
        public Changes_OriginalValues original_values { get; set; }
    }

    public struct Primary_Resources
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string story_type { get; set; }
        public string url { get; set; }
    }

    public struct Project
    {
        public int id { get; set; }
        public string name { get; set; }
    }


    public struct PerformedBy
    {
        public int id { get; set; }
        public string name { get; set; }
        public string initials { get; set; }
    }


    public class Activity
    {
        public string kind { get; set; }
        public string message { get; set; }
        public string highlight { get; set; }
        public DateTime occurred_at { get; set; }

        public List<Changes> changes { get; set; }
        public List<Primary_Resources> primary_resources { get; set; }
        public Project project { get; set; }
        public PerformedBy performed_by { get; set; }
    }
}
