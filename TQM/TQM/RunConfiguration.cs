namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Test balance serail no: DS85222849198
        private string balanceSerialNo = "G85223860077";
        private string tqmAppUserID = "lsspinning";
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
