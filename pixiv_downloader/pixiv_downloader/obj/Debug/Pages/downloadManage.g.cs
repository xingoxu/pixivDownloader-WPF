﻿#pragma checksum "..\..\..\Pages\downloadManage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "549A0FE02BD2C1A9C23CB432EF38D34E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Converters;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace pixiv_downloader {
    
    
    /// <summary>
    /// downloadManage
    /// </summary>
    public partial class downloadManage : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView tasklistview;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button newTask;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button startTask;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancelTask;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button loadTask;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button saveTask;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Pages\downloadManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button deleteTask;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/pixiv_downloader;component/pages/downloadmanage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\downloadManage.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.tasklistview = ((System.Windows.Controls.ListView)(target));
            
            #line 10 "..\..\..\Pages\downloadManage.xaml"
            this.tasklistview.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.tasklistview_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.newTask = ((System.Windows.Controls.Button)(target));
            
            #line 21 "..\..\..\Pages\downloadManage.xaml"
            this.newTask.Click += new System.Windows.RoutedEventHandler(this.newTask_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.startTask = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\..\Pages\downloadManage.xaml"
            this.startTask.Click += new System.Windows.RoutedEventHandler(this.startTask_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.cancelTask = ((System.Windows.Controls.Button)(target));
            
            #line 23 "..\..\..\Pages\downloadManage.xaml"
            this.cancelTask.Click += new System.Windows.RoutedEventHandler(this.cancelTask_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.loadTask = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\..\Pages\downloadManage.xaml"
            this.loadTask.Click += new System.Windows.RoutedEventHandler(this.loadTask_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.saveTask = ((System.Windows.Controls.Button)(target));
            
            #line 25 "..\..\..\Pages\downloadManage.xaml"
            this.saveTask.Click += new System.Windows.RoutedEventHandler(this.saveTask_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.deleteTask = ((System.Windows.Controls.Button)(target));
            
            #line 26 "..\..\..\Pages\downloadManage.xaml"
            this.deleteTask.Click += new System.Windows.RoutedEventHandler(this.deleteTask_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

