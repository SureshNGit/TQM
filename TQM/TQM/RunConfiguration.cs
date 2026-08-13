namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Test balance serail no: DS85222849198
        //Current test balance serail no: H2500064125

        private string balanceSerialNo = "H2500119628";
        private string tqmAppUserID = "avaneetha";
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
