﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RuiJi.Slice.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShowSTL3D stlModel = null;

        private int middleSpeed = 10;     //滚轮的速度
        private bool m_worldArrow;        //是否存在世界坐标

        public MainWindow()
        {
            InitializeComponent();
        }


        /*
         * 打开文件对话框按钮
         * 返回值：所选的文件的路径
         */
        private void ButtonOpenStlFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDlg = new System.Windows.Forms.OpenFileDialog();
            fileDlg.InitialDirectory = "D:\\";
            fileDlg.Filter = "STL file(*.stl)|*.stl|All files(*.*)|*.*";
            fileDlg.FilterIndex = 0;

            fileDlg.ShowDialog();

            if (!string.IsNullOrEmpty(fileDlg.FileName))
            {

                Task.Run(new Action(() =>
              {
                  Dispatcher.Invoke(new Action(() =>
                  {
                      path.Text = fileDlg.FileName;

                      var loading = new Loading();
                      loading.VerticalAlignment = VerticalAlignment.Center;
                      loading.MinWidth = 800;
                      loading.MinHeight = 150;
                      main_panel.Children.Insert(main_panel.Children.Count - 1, loading);
                      main_panel.RegisterName("stl_loading", loading);
                      
                  }));
                  Thread.Sleep(2000);
                  Task.Run(new Action(() =>
                      {
                          Dispatcher.Invoke(new Action(() =>
                          {
                              if (fileDlg.FileName != null && fileDlg.FileName.ToLower().EndsWith(".stl"))
                              {
                                  base_panel.Visibility = Visibility.Visible;
                                  bt_panel.Visibility = Visibility.Visible;
                                  btn_send.Visibility = Visibility.Visible;
                                  ShowSTLModel();
                              }
                              var findloading = FindName("stl_loading") as Loading;
                              main_panel.Children.Remove(findloading);
                              main_panel.UnregisterName("stl_loading");
                          }));
                      }));



              }));
            }
        }


        /*
         * 生成Gcode按钮
         * 问题：参数未配置
         */
        //private void ButtonMakeGcode_Click(object sender, RoutedEventArgs e)
        //{
        //    if (stlModel == null)
        //    {
        //        MessageBox.Show("请先加载STL文件", "无法生成");
        //        return;
        //    }
        //    ParameterizedThreadStart threadStart = new ParameterizedThreadStart(MakeThreadProc);
        //    Thread thread = new Thread(threadStart);
        //    thread.Start(filePath);
        //}
        //public static void MakeThreadProc(object obj)
        //{
        //    MakeSTLGcode makeSTLGcode = new MakeSTLGcode(obj.ToString());
        //}

        /*
         * 调用ShowSTL3D类，显示3D图形
         */
        public void ShowSTLModel()
        {
            stlModel = new ShowSTL3D(path.Text);

            myViewport3D.Children.Clear();
            myViewport3D.Children.Add(stlModel.GetMyModelVisual3D());
            myViewport3D.Children.Add(stlModel.myModelVisual3D());
            //myViewport3D.Children.Add(stlModel.myModelVisual3D2());//加第二个光源
            myViewport3D.Camera = stlModel.MyCamera();
        }

        //滚轮事件
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (stlModel != null && e.LeftButton == MouseButtonState.Released)  //放大缩小
            {
                myViewport3D.Camera = stlModel.nearerCamera(e.Delta / 120 * middleSpeed * (-1));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && stlModel != null)
            {
                Transform3D transfrom3D = trackBallDec.Transform;
                myViewport3D.Children.Remove(stlModel.GetModelVisual3D());
                myViewport3D.Children.Add(stlModel.TransModelVisual3D(transfrom3D));
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                middleSpeed += 5;
                if (middleSpeed > 20)
                {
                    middleSpeed = 10;
                }
            }
        }

        private void btn_world_Click(object sender, RoutedEventArgs e)
        {
            if (stlModel != null)
            {
                if (!m_worldArrow)
                {
                    myViewport3D.Children.Add(stlModel.DrawWroldLine());
                }
                //                 else
                //                 {
                //                     int index = myViewport3D.Children.IndexOf(stlModel.GetWorldLine());
                //                     myViewport3D.Children.RemoveAt(index);
                //                 }
                m_worldArrow = !m_worldArrow;
            }
        }
    }
}