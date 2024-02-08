namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Test balance serail no: DS85222849198
        private string balanceSerialNo = "G85223891414";
        private string tqmAppUserID = "lsmillsunita";
        private bool requireCSV = false;

        public string getBalanceSerialNo()
        {
            return balanceSerialNo;
        }

        public string getTQMAppUserID()
        {
            return tqmAppUserID;
        }

        public bool getCSVReportStatus()
        {
            return requireCSV;
        }
    }
}
