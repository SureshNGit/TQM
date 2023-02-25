namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Test balance serail no: G85222825615
        private string balanceSerialNo = "G85222849193";
        private string loadCellSerailNo = "INTEL";
        private string tqmAppUserID = "tqmuser";

        public string getBalanceSerialNo()
        {
            return balanceSerialNo;
        }

        public string getLoadCellSerailNo()
        {
            return loadCellSerailNo;
        }

        public string getTQMAppUserID()
        {
            return tqmAppUserID;
        }
    }
}
