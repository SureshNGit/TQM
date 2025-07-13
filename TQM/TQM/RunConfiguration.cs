namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Test balance serail no: DS85222849198
        private string balanceSerialNo = "H2500081654";
        private string tqmAppUserID = "tqmuser";
        private bool moveReportToCloud = false;
        private bool requireCSV = true;
        private bool noilsAutoCorrection = false;
        private bool apercentAutoCorrection = false;
        private bool stretchAutoCorrection = false;

        public string getBalanceSerialNo()
        {
            return balanceSerialNo;
        }

        public string getTQMAppUserID()
        {
            return tqmAppUserID;
        }

        public bool getMoveReportToCloud()
        {
            return moveReportToCloud;
        }

        public bool getCSVReportStatus()
        {
            return requireCSV;
        }

        public bool getNoilsAutoCorrection()
        {
            return noilsAutoCorrection;
        }

        public bool getApercentAutoCorrection()
        {
            return apercentAutoCorrection;
        }

        public bool getStretchAutoCorrection()
        {
            return stretchAutoCorrection;
        }
    }
}
