using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTDailyReport;

/// <summary>
/// Summary description for APIRequest
/// </summary>
public class APIRequest
{

    public struct ActivityReport
    {
        public string _project;
        public int _storyID;
        public string _story;
        public string _story_type;
        public string _story_url;
        public int _commentID;
        public string _comments;
        public bool _has_attachment;
        public string _attachment_thumb;
        public string _attachment_url;
        public DateTime _updateDate;
        public DateTime _occurred_at;
    }

    public string GetPivotalTrackerJson(string _APIToken)
    {
        string _JsonString = null;
        try
        {
            string _date = HttpUtility.HtmlEncode(DateTime.Today.ToString("s"));
            string _apiRequestURL = "https://www.pivotaltracker.com/services/v5/my/activity?occurred_after=" + _date ;

            HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(new Uri(_apiRequestURL));
            _request.Headers.Add("X-TrackerToken", _APIToken);
            _request.ContentType = "application/json; charset=utf-8";
            _request.Method = "GET";

            // Log the response from RESTful service
            using (HttpWebResponse _response = (HttpWebResponse)_request.GetResponse())
            {
                if (_response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader _getStream = new StreamReader(_response.GetResponseStream()))
                    {
                        _JsonString = _getStream.ReadToEnd();
                    }
                }
            }
        }
        catch (Exception Ex)
        {
          HttpContext.Current.Response.Write(Ex.Message);
        }
        return _JsonString;
    }

    public List<Activity> GetMyActivities(string _JsonString)
    {
        List<Activity> _myActivities = new List<Activity>();
        if (!string.IsNullOrEmpty(_JsonString))
        {
            _myActivities = JsonConvert.DeserializeObject<List<Activity>>(_JsonString);
        }
        return _myActivities;
    }

    public void GetActivitiesComments(List<Activity> _myActivities)
    {
        List<ActivityReport> _myActivityReportList = new List<ActivityReport>();

        if (_myActivities.Count > 0)
        {
            foreach (Activity _myActivity in _myActivities)
            {
                if (_myActivity.kind.Contains("comment_create_activity") ||
                    _myActivity.kind.Contains("comment_update_activity"))
                {
                    ActivityReport _reportItem = new ActivityReport();
                    foreach (Changes _myChanges in _myActivity.changes)
                    {
                        if (_myChanges.kind == "comment" && _myChanges.change_type != "delete" && _myChanges.new_values.text != null)
                        {
                            _reportItem._commentID = _myChanges.id;
                            _reportItem._has_attachment = false;
                            _reportItem._comments = _myChanges.new_values.text;
                            _reportItem._updateDate = _myChanges.new_values.updated_at;

                        }
                        else if (_myChanges.kind == "file_attachment" && _myChanges.change_type != "delete")
                        {
                            _reportItem._commentID = _myChanges.id;
                            _reportItem._has_attachment = true;
                            _reportItem._updateDate = _myChanges.new_values.updated_at;
                            _reportItem._attachment_thumb = _myChanges.new_values.thumbnail_url;
                            _reportItem._attachment_url = _myChanges.new_values.big_url;
                        }
                    }
                    _reportItem._project = _myActivity.project.name;
                    _reportItem._storyID = _myActivity.primary_resources[0].id;
                    _reportItem._story = _myActivity.primary_resources[0].name;
                    _reportItem._story_type = _myActivity.primary_resources[0].story_type;
                    _reportItem._story_url = _myActivity.primary_resources[0].url;
                    _reportItem._occurred_at = _myActivity.occurred_at;
                    _myActivityReportList.Add(_reportItem);
                }
            }
            GetLatestCommentUpdateList(_myActivityReportList);
            //GenrateReport(_myActivityReportList);
        }
    }

    public void GetLatestCommentUpdateList(List<ActivityReport> _CommentsList)
    {
        List<ActivityReport> _myReportList = new List<ActivityReport>();

        foreach (ActivityReport _commentItem in _CommentsList)
        {
            if (_myReportList.Count > 0)
            {
                bool _addToList = false;
                for (int _itemIndex = 0; _itemIndex < _myReportList.Count; _itemIndex++)
                {
                    if (_commentItem._commentID == _myReportList[_itemIndex]._commentID)
                    {
                        int _dateCompare = DateTime.Compare(_commentItem._updateDate, _myReportList[_itemIndex]._updateDate);
                        if (_dateCompare > 0)
                        {
                            _myReportList.RemoveAt(_itemIndex);
                            _myReportList.Insert(_itemIndex, _commentItem);
                        }
                        _addToList = false;
                    }
                    else
                    {
                        _addToList = true;
                    }
                }
                if (_addToList) { _myReportList.Add(_commentItem); }
            }
            else
            {
                _myReportList.Add(_commentItem);
            }
        }
        GenrateReport(_myReportList);
    }
    public void GenrateReport(List<ActivityReport> _myReportList)
    {
        foreach (ActivityReport _reportItem in _myReportList)
        {
            if (!_reportItem._has_attachment)
            {
                string _header = "<p><b>[" + _reportItem._project.ToUpper() + "]</b> " + _reportItem._story + " [#" + _reportItem._storyID + "] [" + _reportItem._story_type + "]</p>";
                string _Comments = (string.IsNullOrEmpty(_reportItem._comments)) ? string.Empty : _reportItem._comments.Replace("\n\n", "<br />");
                _Comments = _Comments.Replace("\n", "<br />");
                HttpContext.Current.Response.Write(_header);
                HttpContext.Current.Response.Write(_Comments + "<br />");
                HttpContext.Current.Response.Write("Link : <a href='" + _reportItem._story_url + "'>" + _reportItem._story_url + "</a><br />");
                HttpContext.Current.Response.Write("<hr />");
            }
            else
            {
                string _header = "<p><b>[" + _reportItem._project.ToUpper() + "]</b> " + _reportItem._story + " [#" + _reportItem._storyID + "] [Attachmets]</p>";
                string _attachment = @"<a href="+_reportItem._attachment_url+" target='_blank'><img src=" + _reportItem._attachment_thumb + "  /></a>";
                HttpContext.Current.Response.Write(_header);
                HttpContext.Current.Response.Write(_attachment + "<br />");
                HttpContext.Current.Response.Write("<hr />");
            }
        }
    }

    public void GetMyInfo(string _JsonString)
    {
        MyInfo _myInfo = new MyInfo();
        if (_JsonString != null)
        {
            _myInfo = JsonConvert.DeserializeObject<MyInfo>(_JsonString);
        }
    }


    public void WriteToFile(string _JsonString)
    {
        try
        {
            using (FileStream _responseFile = new FileStream(HttpContext.Current.Server.MapPath("~/Activity.json"), FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter _WriteFile = new StreamWriter(_responseFile))
                {
                    _WriteFile.Write(_JsonString);
                }
            }
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.Message);
        }
    }


}