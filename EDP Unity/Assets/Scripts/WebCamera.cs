using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera : MonoBehaviour
{
  private Texture2D backgroundTexture;
  private ARKit.Camera capture;
  public ARKit.FeaturePoints fp = null;
  public ARKit.InitialFrame ip = null;
  public bool initialMatchDone = false;
  public InstantiateBoard MT;
  private int counter = 0;

  /*
  private bool camAvailable;
  private WebCamTexture webCam;
  private Texture defaultBackground;
  */

  public RawImage background;
  public AspectRatioFitter fit;
  public Canvas canvas;

  private void Start()
  {
    // defaultBackground = background.texture;
    // WebCamDevice[] devices = WebCamTexture.devices;
    /*
    if (devices.Length == 0)
    {
        Debug.Log("No camera detected");
        camAvailable = false;
        return;
    }

    for (int i = 0; i < devices.Length; i++)
    {
        if (devices[i].isFrontFacing) {
            webCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
        }
    }

    if(webCam == null)
    {
        Debug.Log("Unable to find camera");
        return;
    }

    webCam.Play();
    background.texture = webCam;

    camAvailable = true;
    */

    // ARKit.Frame frame;

    this.capture = new ARKit.Camera(0, new ARKit.Size(1080, 720));

    //setting the matched image
    Emgu.CV.Mat frame = Emgu.CV.CvInvoke.Imread("match-external-webcam.jpg");

    // ARKit.Frame newFrame = new ARKit.Frame(ARKit.Memory.Frame.Clone().Height, ARKit.Memory.Frame.Clone().Width, ARKit.Memory.Frame.Clone().)


    //frame = ARKit.Memory.Frame.

    //frame = this.capture.GetNextFrame();

    this.backgroundTexture = new Texture2D(frame.Width, frame.Height);
    background.texture = this.backgroundTexture; // set texture to webcam frames
    // referenced from https://answers.unity.com/questions/23891/resizing-an-object.html
    canvas.transform.localScale = new Vector3((float)frame.Width / (float)frame.Height, 1, 1); // fix resolution of plane

    if (System.IO.File.Exists("intrinsics.yml"))
    {
      this.ip = new ARKit.InitialFrame();
      this.ip.ReadFromFile();
    }
    else
    {
      this.ip = new ARKit.InitialFrame(this.capture, new ARKit.Size(4, 7), 30);
      this.ip.Start();
    }

    ARKit.FeaturePoints.ComputeAndSave("simpsons-orig.jpg", "Assets/keypoints.yml");
    this.fp = ARKit.FeaturePoints.ReadData("Assets/keypoints.yml");
    ARKit.Memory.Frame = frame;
  }


  private void Update()
  {
        /*
        if (!camAvailable)
            return;

        float ratio = (float)webCam.width / (float)webCam.height;

        fit.aspectRatio = ratio;

        float scaleY = webCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        */
        GameObject g = GameObject.FindGameObjectWithTag("MainTrigger");
        MT = g.GetComponent<InstantiateBoard>();

        bool tracking = false;

    if (ip != null && fp != null)
    {
      //if (!ip.Homography.IsEmpty)
      {
        //  var frame = this.capture.GetNextFrame();

        //if (!initialMatchDone)
        //{
        //  fp.ComputeAndMatch();
        //  initialMatchDone = true;
        //}
        //else
        //  tracking = fp.TrackObject();

        //if (fp.FindObject(!tracking))
        //{
        //  if (fp.GetPose(ip.CameraMatrix, ip.DistortionCoefficients, out Emgu.CV.Mat rotations, out Emgu.CV.Mat translations))
        //  {
        //    frame = fp.DrawObjectBorder(true, ip.CameraMatrix, ip.DistortionCoefficients, rotations, translations);
        //  }
        //  else
        //    frame = fp.DrawObjectBorder();

      this.fp.ComputeAndMatch();

        bool found = this.fp.FindObject();
        MT.objectFound = found;
        print("object found " + found);
        ARKit.Frame f = this.fp.DrawObjectBorder();


        //using (Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> nextFrame = ARKit.Memory.Frame.ToImage<Emgu.CV.Structure.Bgr, byte>())
        //{
        //  System.Drawing.Bitmap currentFrame = nextFrame.ToBitmap();
        //  System.IO.MemoryStream m = new System.IO.MemoryStream();
        //  System.Drawing.Imaging.ImageFormat format = currentFrame.RawFormat;

        //  /*
        //   * was throwing encoder cannot be null for formats that are MemoryBmp,
        //   * found and fixed by referring to https://stackoverflow.com/questions/25242728/image-save-throws-exception-value-cannot-be-null-r-nparameter-name-encoder
        //   */
        //  currentFrame.Save(m, format);
        //}



        this.backgroundTexture.LoadImage(f.Image);



        Camera cam = Camera.main;
        if (this.fp.GetHomography(out Emgu.CV.Mat H))
        {
          Emgu.CV.Matrix<double> H_mat = new Emgu.CV.Matrix<double>(3, 3);
          Emgu.CV.Matrix<double> cam_mat = new Emgu.CV.Matrix<double>(3, 3);
          for (int i = 0; i < 3; i++)
          {
            for (int j = 0; j < 3; j++)
            {
              double val = ARKit.MatExtension.GetValue(H, i, j);

              H_mat[i, j] = val;

              val = ARKit.MatExtension.GetValue(ip.CameraMatrix, i, j);
              cam_mat[i, j] = val;
            }
          }
          Emgu.CV.Matrix<double> proj = fp.projection_mat(H_mat, cam_mat);


          Matrix4x4 proj_mat = Matrix4x4.identity;

          print("rows " + proj.Rows + " cols " + proj.Cols);

          for (int i = 0; i < 4; i++)
          {
            for (int j = 0; j < 4; j++)
            {
              double val = proj[i, j];
              int val_proj = (int)val;

              print("i: " + i.ToString() + " j: " + j.ToString());

              proj_mat[i, j] = val_proj;
            }
          }

          cam.projectionMatrix = proj_mat;
          print("projection matrix set");
          print("euler angles " + cam.projectionMatrix.rotation.ToString());

        }
      }
    }
  }
}
