namespace TQM
{
    internal class RunConfiguration
    {
        //Demo balance serail no: G85219651634
        //Demo loadCellSerailNo: "SASTHA-01";
        //Test balance serail no: DS85222849197
        private string balanceSerialNo = "G85223867538";
        private string loadCellSerailNo = "SASTHA-02";
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
