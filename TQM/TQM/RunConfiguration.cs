namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Test balance serail no: DS85222849198
        private string balanceSerialNo = "G85223874667";
        private string tqmAppUserID = "lsmillsunit1";
        private bool requireCSV = false;
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
