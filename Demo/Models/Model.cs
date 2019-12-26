using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo.Models
{
    public class RetData<T>
    {
        public int code { get; set; }
        public string message { get; set; }

        public string timestamp { get; set; }
        public int time_cost { get; set; }
        public T data { get; set; }
    }


    public class RetListData<T>
    {
        public int code { get; set; }
        public string message { get; set; }

        public string timestamp { get; set; }
        public int time_cost { get; set; }
        public List<T> data { get; set; }
    }

    #region video

    public class VideoData
    {
        public List<VideosData> videos { get; set; }
        
    }

        public class VideosData
    {

        /// <summary>
        /// 必填，视频通道编号：支持16路视频，值为1 ~16，这将是此布点的唯一标识，后续操作都依赖这个名称
        /// </summary>
        public int videoNo { get; set; }

        /// <summary>
        /// 非必填，摄像头布点名称，由用户起名, 以下规则：
        /// 支持4-16字节长度
        /// 支持英文大/小写字母、数字、半角下划线
        /// 建议正则/^[a-zA-Z0-9_]{4,16}
        /// </summary>
        public string videoName { get; set; }

        /// <summary>
        /// 必填，视频流地址,通常为局域网内的RTSP地址，限制长度为128字节，超过则报错。
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 必填，视频流子码流地址,通常为局域网内的RTSP地址，限制长度为128字节，超过则报错。
        /// </summary>
        public string subUrl { get; set; }
        /// <summary>
        /// IPC接入的用户名
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// IPC接入的密码
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// 必填，图像旋转的角度，为配合不同的摄像头安装角度，目前只支持4个值：0，90.180，270。分别代表顺时针旋转的角度0°，90°，
        /// 180°，270°，如错误将检测不到人脸。如输入不为以上4个值，将默认为0°来处理。
        /// </summary>
        public int rotation { get; set; }


        public int livenessThreshold { get; set; }

        public bool surveillanceEnabled { get; set; }
        public filters filter { get; set; }

        //public List<string> group { get; set; }

        //public List<string> triggerSet { get; set; }

    }

    public class filters
    {
        public int roll { get; set; }
        public int yaw { get; set; }
        public int pitch { get; set; }
        public int blurness { get; set; }
        public int highBrightness { get; set; }
        public int lowBrightness { get; set; }
        public int deviation { get; set; }
        public int faceMin { get; set; }
    }

    #endregion


    #region Face



    /// <summary>
    /// FaceData
    /// </summary>
    public class FaceData
    {
        public int nextPageToken { get; set; }
        public int totalFaces { get; set; }
        public List<face> face { get; set; }
    }

    public class face
    {
        public string imageId { get; set; }
        public string faceToken { get; set; }
        public string description { get; set; }
    }

    #endregion
}