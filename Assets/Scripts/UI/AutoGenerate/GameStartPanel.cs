/*----------------------------------
 *Title:UI自动化组件生成代码生成工具
 *Date:2025/12/27 21:40:41
 *Description:变量需要以[Text]括号加组件类型的格式进行声明,然后右键窗口物体——一键生成UI数据组件脚本即可
 *注意:自动生成的内容仅追加/移除，不会覆盖手写逻辑；新增/删除组件后再次生成，会标记对应代码行
----------------------------------*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyFrameWork
{

	public class GameStartPanel:BasePanel
	{
		//字段声明
		public Button btnStart;
		public Button btnOptions;
		public Button btnQuit;
		// 留给新增的字段
		//自己手写的字段声明，不会被覆盖
		public GameObject gameLogic;

		//重写Awake方法
		protected override void Awake()
		{
			base.Awake();
			//自己手写的Awake逻辑，不会被覆盖
		}

		//重写Start方法,完成事件绑定
		protected override void Start()
		{
			base.Start();
			//组件事件绑定
			btnStart.onClick.AddListener(()=>OnbtnStartButtonClick());
			btnOptions.onClick.AddListener(()=>OnbtnOptionsButtonClick());
			btnQuit.onClick.AddListener(()=>OnbtnQuitButtonClick());
			// 留给新增的组件事件绑定
			//自己手写的Start逻辑，不会被覆盖
		}

		public override void ShowMe()
		{
			//自己手写的ShowMe逻辑，不会被覆盖
		}

		public override void HideMe()
		{
			//自己手写的HideMe逻辑，不会被覆盖
			this.gameObject.SetActive(false);
		}

		private void OnbtnStartButtonClick()
		{
			//自己手写的OnbtnStartButtonClick逻辑，不会被覆盖
			// Debug.Log("OnbtnStartButtonClick");
			// UIMgr.Instance.HidePanel<GameStartPanel>();
			this.HideMe();
			gameLogic.SetActive(true);
		}

		private void OnbtnOptionsButtonClick()
		{
			//自己手写的OnbtnOptionsButtonClick逻辑，不会被覆盖
			Debug.Log("OnbtnOptionsButtonClick");
		}

		private void OnbtnQuitButtonClick()
		{
			//自己手写的OnbtnQuitButtonClick逻辑，不会被覆盖
			Debug.Log("OnbtnQuitButtonClick");
		}

		// 留给新增的组件事件绑定函数
		//自己写的内容
	}
}