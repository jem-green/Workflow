using System;
using System.Collections;
using TracerLibrary;

namespace WorkflowLibrary
{
    /// <summary>
    /// Job class derived from a standard Activity
    /// </summary>
    public class Job : Activity, IActivity, IEnumerable, ICloneable
    {
        #region Fields

        private static int jobId;
        #endregion
        #region Constructors

        public Job() : base()
        {
            jobId = jobId + 1;
            _id = "job_" + jobId.ToString();
        }

        public Job(string id) : base(id)
        {
            _id = id;
            if (_id.StartsWith("job_"))
            {
                if (jobId < Convert.ToInt16(_id.Substring(4)))
                {
                    jobId = Convert.ToInt16(_id.Substring(4));
                }
            }
        }

        #endregion Constructors
        #region Properties

        #endregion Properites
        #region Methods

        #endregion Methods

    }
}
