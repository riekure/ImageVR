using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GyroController : MonoBehaviour
{
    // キーボードでの操作用
#if UNITY_EDITOR || UNITY_STANDALONE
    private Vector3 rotate;
#endif
    public Vector3 fixrot;  // 視点リセット用修正角度変数
    public Vector3 nowrot;  // 視点リセット用現在角度変数
    public Vector3 delrot;  // 視点リセット用オフセット変数

    public bool modeFlag;                   // VR操作モード:true/タッチ操作モード:false
    public GameObject targetObject;

    public float minCameraAngleX = 340.0f;  // カメラの最小角度
    public float maxCameraAngleX = 20.0f;   // カメラの最大角度
    public float swipeTurnSpeed = 10.0f;    // スワイプで回転するときの回転スピード

    private Vector3 baseMousePos;           // 基準となるタップの座標
    private Vector3 baseCameraPos;          // 基準となるカメラの座標
    private bool isMouseDown = false;       // マウスが押されているかのフラグ

    public Button modeButton;
    public Sprite grayModeButton_image;
    public Sprite whiteModeButton_image;

    // Use this for initialization
    void Start()
    {
        Debug.Log("stated");    // 動作確認用のログ
        modeFlag = true;
        fixrot = new Vector3(0, 0, 0);
        nowrot = new Vector3(0, 0, 0);
        delrot = new Vector3(0, 0, 0);

#if UNITY_EDITOR || UNITY_STANDALONE
        rotate = transform.eulerAngles;
        Debug.Log("non-smartphone");    // 動作環境の判別用のログ
#else
        Input.gyro.enabled = true;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // PCの場合はキーボードで視点変更、スマホはジャイロで視点変更
#if UNITY_EDITOR || UNITY_STANDALONE
        float speed = Time.deltaTime * 100.0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotate.y -= speed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotate.y += speed;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rotate.x -= speed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rotate.x += speed;
        }
        transform.rotation = Quaternion.Euler(rotate);
#else
        if (modeFlag)
        {
            Quaternion gattitude = Input.gyro.attitude;
            gattitude.x *= -1;
            gattitude.y *= -1;
            transform.localRotation = Quaternion.Euler(90, -fixrot.y, 0) * gattitude;   // Y軸に対して修正角度を反映
        } else
        {
            // タップの種類判定 & 対応処理
            // GetMouseButtonDown(0) = 画面にタッチした
            // GetMouseButtonUp(0) = 画面から指が離れた
            if ((Input.touchCount == 1 && !isMouseDown) || Input.GetMouseButtonDown(0))
            {
                baseMousePos = Input.mousePosition;
                isMouseDown = true;
            }

            // 指を離したときの処理
            if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
            }

            // スワイプ回転処理
            if (isMouseDown)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 distanceMousePos = (mousePos - baseMousePos);
                float angleX = targetObject.transform.eulerAngles.x - distanceMousePos.y * swipeTurnSpeed * 0.01f;
                float angleY = targetObject.transform.eulerAngles.y + distanceMousePos.x * swipeTurnSpeed * 0.01f;

                if ((angleX >= -10f && angleX <= maxCameraAngleX) || (angleX >= minCameraAngleX && angleX <= 370f))
                {
                    targetObject.transform.eulerAngles = new Vector3(angleX, angleY, 0);
                }
                else
                {
                    targetObject.transform.eulerAngles = new Vector3(targetObject.transform.eulerAngles.x, angleY, 0);
                }
                baseMousePos = mousePos;
            }
        }

#endif
    }

    public void OnClickResetButton()
    {
        Debug.Log("Reset");  // 動作確認用のログ

        nowrot = transform.localEulerAngles;
        fixrot += (nowrot + delrot);  // 修正角度変数に、現在の角度を加算
    }

    public void OnClickModeButton()
    {
        // 動作確認用ログ
        Debug.Log("Mode");

        if (modeFlag)
        {
            modeFlag = false;
            modeButton.image.sprite = whiteModeButton_image;
        }
        else
        {
            modeFlag = true;
            modeButton.image.sprite = grayModeButton_image;
        }
    }
}