namespace WorkflowLibrary
{
    public class State
    {
        protected StateType _state = StateType.None;

        public enum StateType
        {
            None = 0,
            Inactive = 1,
            Ready = 2,
            Active = 3,
            Completing = 4,
            Completed = 5,
            Withdrawn = 6,
            Failing = 7,
            Terminating = 8,
            Compensating = 9,
            Compensated = 10,
            Terminated = 11,
            Failed = 12,
            Closed = 13,
        }

        public enum StageType
        {
            Process = 0,
            Job = 1,
            Task = 2,
            Item = 3
        }

        public enum DataKind
        {
            none = 0,
            echo = 1,
            multiple = 2,
            single = 3,
            value = 4,
            pipe = 5,
            raw =6
        }

        public enum DataType
        {
            String = 0,
            Integer = 1,
            Double = 2
        }

        public enum ItemType
        {
            Process = 0,
            sql = 1
        }

        public int OutputDescription(DataKind type)
        {
            int output = 0;

            switch (type)
            {
                case DataKind.none:
                    {
                        output = 0; 
                    }
                    break;
                case DataKind.echo:
                    {
                        output = 1;
                    }
                    break;
                case DataKind.multiple:
                    {
                        output = 2;
                    }
                    break;
                case DataKind.single:
                    {
                        output = 3;
                    }
                    break;
                case DataKind.value:
                    {
                        output = 4;
                    }
                    break;
            }
            return (output);
        }

        public string StateDescription(StateType state)
        {
            string stateName = "";
            switch ((int)state)
            {
                case 0:
                    stateName = "None";
                    break;
                case 1:
                    stateName = "Inactive";
                    break;
                case 2:
                    stateName = "Ready";
                    break;
                case 3:
                    stateName = "Active";
                    break;
                case 4:
                    stateName = "Completing";
                    break;
                case 5:
                    stateName = "Completed";
                    break;
                case 6:
                    stateName = "Withdrawn";
                    break;
                case 7:
                    stateName = "Failing";
                    break;
                case 8:
                    stateName = "Terminating";
                    break;
                case 9:
                    stateName = "Compensating";
                    break;
                case 10:
                    stateName = "Compensated";
                    break;
                case 11:
                    stateName = "Terminated";
                    break;
                case 12:
                    stateName = "Failed";
                    break;
                case 13:
                    stateName = "Closed";
                    break;
                default:
                    stateName = "Unknown";
                    break;
            }
            return (stateName);
        }
    }
}
