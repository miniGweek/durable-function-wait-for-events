namespace Models
{
    public class OrchestrationStatusResponse<T>
    {
       
            public DateTimeOffset CreatedTime { get; set; }
            public string CustomStatus { get; set; }
            public string InstanceId { get; set; }
            public DateTimeOffset LastUpdatedTime { get; set; }

            public string Name { get; set; }
            //public object Output { get; set; }
            public OrchestrationRuntimeStatus RuntimeStatus { get; set; }
            public T? Output { get; set; }

    }

    public enum OrchestrationRuntimeStatus
    {
        /// <summary>
        /// The status of the orchestration could not be determined.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The orchestration is running (it may be actively running or waiting for input).
        /// </summary>
        Running = 0,

        /// <summary>
        /// The orchestration ran to completion.
        /// </summary>
        Completed = 1,

        /// <summary>
        /// The orchestration completed with ContinueAsNew as is in the process of restarting.
        /// </summary>
        ContinuedAsNew = 2,

        /// <summary>
        /// The orchestration failed with an error.
        /// </summary>
        Failed = 3,

        /// <summary>
        /// The orchestration was canceled.
        /// </summary>
        Canceled = 4,

        /// <summary>
        /// The orchestration was terminated via an API call.
        /// </summary>
        Terminated = 5,

        /// <summary>
        /// The orchestration was scheduled but has not yet started.
        /// </summary>
        Pending = 6,

        /// <summary>
        /// The orchestration was suspended
        /// </summary>
        Suspended = 7,
    }
}
