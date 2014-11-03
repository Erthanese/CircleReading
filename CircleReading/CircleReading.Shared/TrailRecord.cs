using System;
using System.Collections.Generic;
using System.Text;

namespace CircleReading
{
	public enum CircleLayout
	{
		Overflow,
		Crop,
		Justify,
	}

    public class TrailRecord
    {
		public string		UserId {get; set;}
		public bool			IsNative {get; set;}
		public bool			IsMale {get; set;}
		public CircleLayout	Layout {get; set;}
		public DateTime		TimeStemp {get; set;}
		public TimeSpan		BaseLineTestTime {get; set;}
		public TimeSpan		DetailReadingTime {get; set;}
		public TimeSpan		SearchReadingTime1 {get; set;}
		public TimeSpan		SearchReadingTime2 {get; set;}
		public TimeSpan		SearchReadingTime3 {get; set;}
		public double			GeneralRating { get; set; }
	}

	public class ReadingPageRequestParameter
	{
		public CircleLayout Layout { get; set; }
		public string ArticleName { get; set; }
	}

	public enum TestingState
	{
		Initializing,
		UserInformation,
		BaselineReadingPrepare,
		BaselineReading,
		DetailReadingPrepare,
		DetailReading,
		SearchReadingPrepare1,
		SearchReading1,
		SearchReadingPrepare2,
		SearchReading2,
		SearchReadingPrepare3,
		SearchReading3,
		Rating,
		Finish,
	}
}
