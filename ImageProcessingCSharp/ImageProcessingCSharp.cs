using System;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Text;
/*
using MySql.Data;
using MySql.Data.MySqlClient;
*/
namespace ImageProcessingCSharp
{
    public class ImageProcessingCSharp
    {
        [DllImport("ImageProcessingCPP")]
        private static extern IntPtr getCamera(int CameraIndex);
        [DllImport("ImageProcessingCPP")]
        private static extern void releaseCamera(IntPtr camera);
        [DllImport("ImageProcessingCPP")]
        private static extern void getCameraTexture(IntPtr camera, IntPtr data, int width, int height);
        [DllImport("ImageProcessingCPP")]
        private static extern bool setDescriptors(IntPtr data, int id);
        [DllImport("ImageProcessingCPP")]
        private static extern int findObject();
        [DllImport("ImageProcessingCPP")]
        private static extern void setTrackingPattern();
        [DllImport("ImageProcessingCPP")]
        private static extern void getTransfromMatrix(IntPtr matrix);
        [DllImport("ImageProcessingCPP")]
        private static extern void getProcessingTime(double time);


        public static bool loadFeatures(System.Data.DataTable table)
        {
            //テーブルに中身がない場合は終了
            if (table == null)
            {
                return false;
            }

            System.Data.DataRow datarow;
            System.String descNum;

            datarow = table.Rows[0];    //データテーブルのIDを取得
            int id;                     //DB内でのID
            	

            for (int k = 0; k < table.Rows.Count; k += 200)
            {
                //特徴量データを格納
                byte[] array = new byte[200 * 64];


                //テーブルの行数まで繰り返す
                for (int i = 0; i < 200; i++)
                {
                    //i行を取得
                    datarow = table.Rows[i + k];

                    //i行j列の要素を一次元配列に格納
                    for (int j = 0; j < 64; j++)
                    {
                        descNum = System.Convert.ToString(j + 1);
                        array[i * 64 + j] = System.Convert.ToByte(datarow[descNum].ToString());
                    }

                }
                id = System.Convert.ToInt32(datarow["ID"]);

                //byte[]→IntPtr
                int size = Marshal.SizeOf(array[0]) * array.Length;
                IntPtr pnt = Marshal.AllocHGlobal(size);
                Marshal.Copy(array, 0, pnt, array.Length);

                //特徴量を保存
                setDescriptors(pnt, id);

                //確保した領域の解放
                Marshal.FreeHGlobal(pnt);
            }


            return true;
        }
    }
}
