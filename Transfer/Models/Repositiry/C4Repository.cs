using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repositiry
{
    public class C4Repository : IC4Repository, IDbEvent
    {
        #region 其他
        private Common common = new Common();

        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public C4Repository()
        {
            this.db = new IFRS9Entities();
        }

        public void SaveChange()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.db != null)
                {
                    this.db.Dispose();
                    this.db = null;
                }
            }
        }
        #endregion

        #region Get Data

        public void test(string filePath, string fileName)
        {
            string s =
              "START - OF - FILE \r\n" +
              "REPLYFILENAME = GetCompany_20170726.csv \r\n" +
              "PROGRAMNAME = getcompany \r\n" +
              "PROGRAMFLAG = one - shot \r\n" +
              "FIRMNAME = dl221 \r\n" +
              "CREDITRISK = yes \r\n" +
              "SECID = TICKER \r\n" +
              " \r\n" +
              "START - OF - FIELDS \r\n" +
              "ID_BB_COMPANY \r\n" +
              "LONG_COMP_NAME \r\n" +
              "COUNTRY_ISO \r\n" +
              "INDUSTRY_GROUP \r\n" +
              "INDUSTRY_SECTOR \r\n" +
              "RTG_SP_LT_LC_ISSUER_CREDIT \r\n" +
              "RTG_SP_LT_LC_ISS_CRED_RTG_DT \r\n" +
              "RTG_SP_LT_FC_ISSUER_CREDIT \r\n" +
              "RTG_SP_LT_FC_ISS_CRED_RTG_DT \r\n" +
              "RTG_MDY_FC_CURR_ISSUER_RATING \r\n" +
              "RTG_MDY_FC_CURR_ISSUER_RTG_DT \r\n" +
              "RTG_MDY_ISSUER \r\n" +
              "RTG_MDY_ISSUER_RTG_DT \r\n" +
              "RTG_MOODY_LONG_TERM \r\n" +
              "RTG_MOODY_LONG_TERM_DATE \r\n" +
              "RTG_MDY_SEN_UNSECURED_DEBT \r\n" +
              "RTG_FITCH_LT_ISSUER_DEFAULT \r\n" +
              "RTG_FITCH_LT_ISSUER_DFLT_RTG_DT \r\n" +
              "RTG_FITCH_LT_FC_ISSUER_DEFAULT \r\n" +
              "RTG_FITCH_LT_FC_ISS_DFLT_RTG_DT \r\n" +
              "RTG_FITCH_LT_LC_ISSUER_DEFAULT \r\n" +
              "RTG_FITCH_LT_LC_ISS_DFLT_RTG_DT \r\n" +
              "RTG_FITCH_SEN_UNSECURED \r\n" +
              "RTG_FITCH_SEN_UNSEC_RTG_DT \r\n" +
              "END - OF - FIELDS \r\n" +
              "                 \r\n" +
              "START - OF - DATA \r\n" +
              "0323060D CH Equity| TICKER \r\n" +
              "0348888D TT Equity| TICKER \r\n" +
              "2845 TT Equity| TICKER \r\n" +
              "AGRBTZ TT Equity | TICKER \r\n" +
              "0812668D CN Equity| TICKER \r\n" +
              "ABBV US Equity | TICKER \r\n" +
              "116657Z TT Equity | TICKER \r\n" +
              "3352Z US Equity | TICKER \r\n" +
              "CPC TT Equity | TICKER \r\n" +
              "END - OF - DATA \r\n" +
              "END - OF - FILE \r\n";
            //try
            //{
            //    File.AppendAllText(Path.Combine(filePath , (fileName+ "1.req")) , "\r\n" + s);
            //}
            //catch
            //{

            //}
            try
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(filePath , (fileName + "1.req")), true))
                {
                    sw.WriteLine(s);
                    sw.Close();
                }
                common.Interval = 10000;
                Action f = () => putSFTPFile(filePath, fileName);
                common.Start(f);
            }
            catch
            {

            }

        }
        private void putSFTPFile(string filePath, string fileName)
        {
            //string _ErrorInfo = null;
            //_ErrorInfo = "HIHITEST";
            //string ip = "";
            //string account = "";
            //string password = "";

            //SFTP _SFTPUtility = new SFTP(ip, account, password);

            //_SFTPUtility.Put(filePath, fileName, out _ErrorInfo);

            //if (_ErrorInfo != null)
            //{
            //    //MessageBox.Show(_ErrorInfo);
            //}
            //else
            //{
            //    //MessageBox.Show("SFTP 上傳成功");
            //}
            File.AppendAllText(Path.Combine(filePath , (fileName + "2.req")), "123" );
            //return _ErrorInfo;
        }

        #endregion

        #region Save Db

        #endregion

        #region Excel 部分

        #endregion

        #region Private Function


        #endregion
    }
}