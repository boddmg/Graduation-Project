using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroMQ;
using System.Threading;

namespace Microsoft.Samples.Kinect.WpfViewers
{
    public class ResultReceiveEventArgs : EventArgs
    {
        private string message;

        public string Message
        {
          get { return message; }
          set { message = value; }
        }
        public ResultReceiveEventArgs(string s)
        {
            Message = s;
        }
    }

    class SkeletonSender
    {
        private ZContext context;
        private ZSocket pushDataSocket;
        private ZSocket pullResultSocket;
        private Thread receiveThread;
        private int pushCounter;
        private int pullCounter;
        private int messageLength;
        private static readonly string[] MOTION_TABLE = new string[]{
            //"敲打","打电话","站立","坐下","刷牙","跑步"
            "敲打","打电话","站立","刷牙","跑步"
        };
        public event EventHandler<ResultReceiveEventArgs> ResultReceiveEvent;

        public SkeletonSender(string address, string pushDataPort, string pullResultPort)
        {
            context = new ZContext();
            pushDataSocket = new ZSocket(context, ZSocketType.PUSH);
            pushDataSocket.Connect("tcp://" + address + ":" + pushDataPort);

            pullResultSocket = new ZSocket(context, ZSocketType.PULL);
            pullResultSocket.Connect("tcp://" + address + ":" + pullResultPort);

            receiveThread = new Thread(() =>
            {
                string newMessage;
                while (true)
                {
                    newMessage = "";
                    newMessage += "发送帧数" + pushCounter + "\r\n";
                    newMessage += "接收帧数" + pullCounter++ + "\r\n";
                    newMessage += MOTION_TABLE[Convert.ToInt32(pullResultSocket.ReceiveFrame().ReadString())];
                    ResultReceiveEvent(this, new ResultReceiveEventArgs(newMessage));

                }
            });
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        public void SendFrame(string data)
        {
            pushDataSocket.Send(new ZFrame(data));
        }
        private String Quaternion2MatrixString(Vector4 quaternion)
        {
            double x = quaternion.X, y = quaternion.Y, z = quaternion.Z, w = quaternion.W;
            double[] matrix = new double[]{
              1-2*y*y-2*z*z,   2*x*y-2*z*w,   2*x*z+2*y*w,
               2*x*y+2*z*w, 1-2*x*x-2*z*z,   2*y*z-2*x*w,
               2*x*z-2*y*w,   2*y*z+2*x*w, 1-2*x*x-2*y*y
            };
            string strToOut = "";
            for (int i = 0; i < 9; i++)
            {
                strToOut += matrix[i];
                strToOut += ",";
            }
            return strToOut;
        }
        static byte[] GetBytes(double[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        public void SendSkeletonFrame(Skeleton skeleton)
        {
            string skeletonString = "";
            double[] skeletonData = new double[144];
            UInt32 index = 0;
            List<JointType> openNIsdkConvList = new List<JointType>();
            openNIsdkConvList.Add(JointType.Head);
            openNIsdkConvList.Add(JointType.ShoulderCenter);
            openNIsdkConvList.Add(JointType.Spine);
            openNIsdkConvList.Add(JointType.ShoulderLeft);
            openNIsdkConvList.Add(JointType.ElbowLeft);
            openNIsdkConvList.Add(JointType.ShoulderRight);
            openNIsdkConvList.Add(JointType.ElbowRight);
            openNIsdkConvList.Add(JointType.HipLeft);
            openNIsdkConvList.Add(JointType.KneeLeft);
            openNIsdkConvList.Add(JointType.HipRight);
            openNIsdkConvList.Add(JointType.KneeRight);
            openNIsdkConvList.Add(JointType.HandLeft);
            openNIsdkConvList.Add(JointType.HandRight);
            openNIsdkConvList.Add(JointType.FootLeft);
            openNIsdkConvList.Add(JointType.FootRight);

            for (int i = 0; i < 15; i++ )
            {
                Joint j = skeleton.Joints[openNIsdkConvList[i]];
                if(i<=10)
                {
                    skeletonString += Quaternion2MatrixString(skeleton.BoneOrientations[openNIsdkConvList[i]].AbsoluteRotation.Quaternion);
                    //double[] temp = Quaternion2MatrixString(skeleton.BoneOrientations[openNIsdkConvList[i]].AbsoluteRotation.Quaternion);
                    //foreach(double singleData in temp)
                    //{

                    //    skeletonData[index++] = singleData;
                    //}
                }
                skeletonString += j.Position.X + ",";
                skeletonString += j.Position.Y + ",";
                //skeletonData[index++] = j.Position.X;
                //skeletonData[index++] = j.Position.Y;
                //skeletonData[index++] = j.Position.Z;
                if(i == 14)
                {
                    skeletonString += j.Position.Z;
                }
                else
                {
                    skeletonString += j.Position.Z + ",";
                }
            }
            //compress

            //skeletonString = pushCounter++ + ";" + skeletonString;
            //Byte[] data = GetBytes(skeletonData);
            //data = Utilities.CompressBytes(data);
            //messageLength = data.Length;
            //pushDataSocket.SendBytes(data, 0, data.Length);
            pushCounter++;
            SendFrame(skeletonString);
        }
    }
}
