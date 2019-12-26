using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using 爬虫demo;
using Newtonsoft;
using Demo.Models;
using Demo.App_Start;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using static 爬虫demo.HttpHelper;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        HttpHelper http = new HttpHelper();
        public const string Url = "http://192.168.1.100/v1/MEGBOX";
        public int pageToken = 0;
        public ActionResult Index()
        {
            RetData<FaceData> ss = GetFaceslist();
            return View();
        }

        #region 取得人脸列表信息

        /// <summary>
        /// 取得人脸库列表
        /// </summary>
        /// <param name="pageOff"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public RetData<FaceData> GetFaceslist(int pageOff = 0, int pageSize = 10)
        {
            string retStr = http.HttpGet(Url + "/faces?pageOffset=" + pageOff + "&pageSize=" + pageSize);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<RetData<FaceData>>(retStr);
        }

        #endregion

        #region 添加人脸至人脸库

        [ActionName("ImportOrder")]
        [HttpPost]
        public object ImportOrder()
        {
            var ssss = Request.RequestContext;
            HttpPostedFileBase oFile = HttpContext.Request.Files["txt_file"];


            InsertFaces(oFile, "111");
            return new { lstOrderImport = "" };
        }

        /// <summary>
        /// 添加人脸至人脸库
        /// </summary>
        /// <param name="ImgVal">图片二进制流</param>
        /// <param name="description">图片说明 （不超过100字节对照片文件的描述，UTF-8编码）</param>
        /// <returns></returns>
        public string InsertFaces(HttpPostedFileBase ImgVal, string description)
        {
            Md5Helper md5 = new Md5Helper();

            byte[] bytes = new byte[ImgVal.InputStream.Length];
            ImgVal.InputStream.Read(bytes, 0, (int)ImgVal.InputStream.Length);
            ImgVal.InputStream.Close();



            List<FormItemModel> lstPara = new List<FormItemModel>();


            FormItemModel model = new FormItemModel();
            model.Key = "image";
            model.FileType = "image/jpeg";
            model.FileName = ImgVal.FileName;

            Stream stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            model.FileContent = stream;
            lstPara.Add(model);



            model = new FormItemModel();
            model.Key = "description";
            model.Value = description;
            lstPara.Add(model);


            model = new FormItemModel();
            model.Key = "imageMd5";
            //model.Value = "YygygxHNJFqhsF1nWzi7TA==";
            Stream streamMD5 = new MemoryStream(bytes);
            streamMD5.Seek(0, SeekOrigin.Begin);

            string sss = md5.GenerateMD5(streamMD5);
            model.Value = MD5EncryptTo16(sss);
            lstPara.Add(model);

            string ret = http.PostForm(Url + "/faces", lstPara, null, null, Encoding.UTF8);
            return "";
        }
        public string MD5EncryptTo16(string EncryptString)
        {
            string ret = string.Empty;
            try
            {
                byte[] bt = new byte[16];
                int j = 0;
                for (int i = 0; i < 32; i = i + 2)
                {
                    string str = EncryptString.Substring(i, 2);
                    int strInt = Convert.ToInt32(str, 16);
                    bt[j] = Convert.ToByte(strInt);
                    j++;
                }
                ret = Convert.ToBase64String(bt);

            }
            catch (Exception ex)
            {

            }
            return ret.Replace('+', '-').Replace('/', '_');
        }


        #endregion



        #region 视频部分接入

        public ActionResult Video()
        {
            string retStr = http.HttpGet(Url + "/videos?/videoNo=5&videoName=&url=&subUrl=&username=admin&password=admin&rotation=0");
            RetData<VideoData> view = Newtonsoft.Json.JsonConvert.DeserializeObject<RetData<VideoData>>(retStr);

            //int videoNo = 1;
            //string videoName = string.Empty;
            //string url = "192.168.1.55";
            //string subUrl = string.Empty;
            //string username = "admin";
            //string password = "admin";
            //int rotation = 0;
            //VideoInput vedio = new VideoInput();
            //vedio.videoNo = videoNo;
            //vedio.videoName = videoName;
            //vedio.url = url;
            //vedio.subUrl = subUrl;
            //vedio.username = username;
            //vedio.password = password;
            //vedio.rotation = rotation;
            // string retStr = http.HttpPost(Url + "/videos", Newtonsoft.Json.JsonConvert.SerializeObject(vedio));
            return View(view);
        }

        #endregion
    }
}