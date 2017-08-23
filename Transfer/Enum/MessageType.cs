using System.ComponentModel;

namespace Transfer.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 回傳訊息格式統一
        /// </summary>
        public enum Message_Type
        {
            /// <summary>
            /// 資料已經儲存過了
            /// </summary>
            [Description("資料已經儲存過了!")]
            already_Save,

            /// <summary>
            /// 儲存成功
            /// </summary>
            [Description("儲存成功!")]
            save_Success,

            /// <summary>
            /// 儲存失敗
            /// </summary>
            [Description("儲存失敗!")]
            save_Fail,

            /// <summary>
            /// 刪除成功
            /// </summary>
            [Description("刪除成功!")]
            delete_Success,

            /// <summary>
            /// 刪除失敗
            /// </summary>
            [Description("刪除失敗!")]
            delete_Fail,

            /// <summary>
            /// 沒有找到資料
            /// </summary>
            [Description("沒有找到資料!")]
            not_Find_Any,

            /// <summary>
            /// 沒有找到搜尋的資料
            /// </summary>
            [Description("沒有找到搜尋的資料!")]
            query_Not_Find,

            /// <summary>
            /// 下載成功
            /// </summary>
            [Description("下載成功!")]
            download_Success,

            /// <summary>
            /// 下載失敗
            /// </summary>
            [Description("下載失敗!")]
            download_Fail,

            /// <summary>
            /// 上傳成功
            /// </summary>
            [Description("上傳成功!")]
            upload_Success,

            /// <summary>
            /// 上傳失敗
            /// </summary>
            [Description("上傳失敗!")]
            upload_Fail,

            /// <summary>
            /// 請選擇上傳檔案
            /// </summary>
            [Description("請選擇上傳檔案!")]
            upload_Not_Find,

            /// <summary>
            /// 請確認檔案為Excel檔案或無資料!
            /// </summary>
            [Description("請確認檔案為Excel檔案或無資料!")]
            excel_Validate,

            /// <summary>
            /// 無比對到資料!
            /// </summary>
            [Description("無比對到資料!")]
            data_Not_Compare,

            /// <summary>
            /// 傳入參數錯誤!
            /// </summary>
            [Description("傳入參數錯誤!")]
            parameter_Error,

            /// <summary>
            /// 時間停滯太久請重新上一動作!
            /// </summary>
            [Description("時間停滯太久請重新上一動作!")]
            time_Out
        }
    }
}