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
        private static extern void getCameraTexture(IntPtr camera, IntPtr data);
        [DllImport("ImageProcessingCPP")]
        private static extern bool setDescriptors(IntPtr data, int id);
        [DllImport("ImageProcessingCPP")]
        private static extern bool setKeypoints(IntPtr array_px, IntPtr array_py, int id);

        [DllImport("ImageProcessingCPP")]
        private static extern int findObject();
        [DllImport("ImageProcessingCPP")]
        private static extern void setTrackingPattern();
        [DllImport("ImageProcessingCPP")]
        private static extern void getTransfromMatrix(IntPtr matrix);
        [DllImport("ImageProcessingCPP")]
        private static extern void getProcessingTime(double time);


        public static bool loadFeatures(System.Data.DataTable descTable, System.Data.DataTable pointsTable)
        {
            //テーブルに中身がない場合は終了
            if (descTable == null || pointsTable == null)
            {
                return false;
            }

            /* 特徴量の読み込み */
            System.Data.DataRow datarow = descTable.Rows[0];    //データテーブルのIDを取得
            System.String descNum;
            int id;                     //DB内でのID
            for (int k = 0; k < descTable.Rows.Count; k += 200)
            {
                //特徴量データを格納
                byte[] array = new byte[200 * 64];

                //テーブルの行数まで繰り返す
                for (int i = 0; i < 200; i++)
                {
                    //i行を取得
                    datarow = descTable.Rows[i + k];

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
                bool flag = setDescriptors(pnt, id);
                if (flag == false)
                    return false;

                //確保した領域の解放
                Marshal.FreeHGlobal(pnt);
            }


            /* 特徴点の読み込み */
            System.Data.DataRow pointsRow = pointsTable.Rows[0];

            //テーブルの行数まで繰り返す
            for (int i = 0; i < pointsTable.Rows.Count; i += 200)
            {
                pointsRow = pointsTable.Rows[i];
                int pointId = System.Convert.ToInt32(pointsRow["ID"]);

                float[] array_points_x = new float[200];
                float[] array_points_y = new float[200];

                for (int j = 0; j < 200; j++)
                {
                    
                    array_points_x[j] = System.Convert.ToSingle(pointsRow["px"].ToString());
                    array_points_y[j] = System.Convert.ToSingle(pointsRow["py"].ToString());
                }

                //float[]→IntPtr
                int xSize = Marshal.SizeOf(array_points_x[0]) * array_points_x.Length;
                IntPtr x_pnt = Marshal.AllocHGlobal(xSize);
                Marshal.Copy(array_points_x, 0, x_pnt, array_points_x.Length);

                int ySize = Marshal.SizeOf(array_points_y[0]) * array_points_y.Length;
                IntPtr y_pnt = Marshal.AllocHGlobal(ySize);
                Marshal.Copy(array_points_y, 0, y_pnt, array_points_y.Length);

                //特徴量を保存
                bool flag = setKeypoints(y_pnt, x_pnt, pointId);
                if (flag == false)
                    return false;

                //確保した領域の解放
                Marshal.FreeHGlobal(x_pnt);
                Marshal.FreeHGlobal(y_pnt);
            }

            return true;
        }

    }
}
